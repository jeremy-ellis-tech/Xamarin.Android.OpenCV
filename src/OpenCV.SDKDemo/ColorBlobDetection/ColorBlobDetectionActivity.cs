using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using OpenCV.Core;
using OpenCV.Android;
using Android.Util;
using OpenCV.SDKDemo.Utilities;
using OpenCV.ImgProc;
using Size = OpenCV.Core.Size;

namespace OpenCV.SDKDemo.ColorBlobDetection
{
    [Activity(Label = "ColorBlobDetectionActivity",
        ScreenOrientation = ScreenOrientation.Landscape,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden)]
    public class ColorBlobDetectionActivity : Activity, View.IOnTouchListener, CameraBridgeViewBase.ICvCameraViewListener2
    {
        private bool mIsColorSelected = false;
        private Mat mRgba;
        private Scalar mBlobColorRgba;
        private Scalar mBlobColorHsv;
        private ColorBlobDetector mDetector;
        private Mat mSpectrum;
        private Size SPECTRUM_SIZE;
        private Scalar CONTOUR_COLOR;

        public CameraBridgeViewBase mOpenCvCameraView { get; private set; }

        BaseLoaderCallback mLoaderCallback;

        public ColorBlobDetectionActivity()
        {
            Log.Info(ActivityTags.ColorBlobDetection, "Instantiated new " + GetType().ToString());
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Info(ActivityTags.ColorBlobDetection, "called onCreate");
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);

            Window.AddFlags(WindowManagerFlags.KeepScreenOn);

            SetContentView(Resource.Layout.color_blob_detection_surface_view);

            mOpenCvCameraView = FindViewById<CameraBridgeViewBase>(Resource.Id.color_blob_detection_activity_surface_view);
            mOpenCvCameraView.Visibility = ViewStates.Visible;
            mOpenCvCameraView.SetCvCameraViewListener2(this);
            mLoaderCallback = new Callback(this, this);
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (mOpenCvCameraView != null)
                mOpenCvCameraView.DisableView();
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (!OpenCVLoader.InitDebug())
            {
                Log.Debug(ActivityTags.ColorBlobDetection, "Internal OpenCV library not found. Using OpenCV Manager for initialization");
                OpenCVLoader.InitAsync(OpenCVLoader.OpencvVersion300, this, mLoaderCallback);
            }
            else
            {
                Log.Debug(ActivityTags.ColorBlobDetection, "OpenCV library found inside package. Using it!");
                mLoaderCallback.OnManagerConnected(LoaderCallbackInterface.Success);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (mOpenCvCameraView != null)
                mOpenCvCameraView.DisableView();
        }

        public void OnCameraViewStarted(int width, int height)
        {
            mRgba = new Mat(height, width, CvType.Cv8uc4);
            mDetector = new ColorBlobDetector();
            mSpectrum = new Mat();
            mBlobColorRgba = new Scalar(255);
            mBlobColorHsv = new Scalar(255);
            SPECTRUM_SIZE = new Size(200, 64);
            CONTOUR_COLOR = new Scalar(255, 0, 0, 255);
        }

        public void OnCameraViewStopped()
        {
            mRgba.Release();
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            int cols = mRgba.Cols();
            int rows = mRgba.Rows();

            int xOffset = (mOpenCvCameraView.Width - cols) / 2;
            int yOffset = (mOpenCvCameraView.Height - rows) / 2;

            int x = (int)e.GetX() - xOffset;
            int y = (int)e.GetY() - yOffset;

            Log.Info(ActivityTags.ColorBlobDetection, "Touch image coordinates: (" + x + ", " + y + ")");

            if ((x < 0) || (y < 0) || (x > cols) || (y > rows)) return false;

            Rect touchedRect = new Rect();

            touchedRect.X = (x > 4) ? x - 4 : 0;
            touchedRect.Y = (y > 4) ? y - 4 : 0;

            touchedRect.Width = (x + 4 < cols) ? x + 4 - touchedRect.X : cols - touchedRect.X;
            touchedRect.Height = (y + 4 < rows) ? y + 4 - touchedRect.Y : rows - touchedRect.Y;

            Mat touchedRegionRgba = mRgba.Submat(touchedRect);

            Mat touchedRegionHsv = new Mat();
            Imgproc.CvtColor(touchedRegionRgba, touchedRegionHsv, Imgproc.ColorRgb2hsvFull);

            // Calculate average color of touched region
            mBlobColorHsv = Core.Core.SumElems(touchedRegionHsv);
            int pointCount = touchedRect.Width * touchedRect.Height;
            for (int i = 0; i < mBlobColorHsv.Val.Count; i++)
                mBlobColorHsv.Val[i] /= pointCount;

            mBlobColorRgba = ConvertScalarHsv2Rgba(mBlobColorHsv);

            Log.Info(ActivityTags.ColorBlobDetection, "Touched rgba color: (" + mBlobColorRgba.Val[0] + ", " + mBlobColorRgba.Val[1] +
                    ", " + mBlobColorRgba.Val[2] + ", " + mBlobColorRgba.Val[3] + ")");

            mDetector.SetHsvColor(mBlobColorHsv);

            Imgproc.Resize(mDetector.Spectrum, mSpectrum, SPECTRUM_SIZE);

            mIsColorSelected = true;

            touchedRegionRgba.Release();
            touchedRegionHsv.Release();

            return false; // don't need subsequent touch events
        }

        public Mat OnCameraFrame(CameraBridgeViewBase.ICvCameraViewFrame inputFrame)
        {
            mRgba = inputFrame.Rgba();

            if (mIsColorSelected)
            {
                mDetector.Process(mRgba);
                List<MatOfPoint> contours = mDetector.Contours;
                Log.Error(ActivityTags.ColorBlobDetection, "Contours count: " + contours.Count);
                Imgproc.DrawContours(mRgba, contours, -1, CONTOUR_COLOR);

                Mat colorLabel = mRgba.Submat(4, 68, 4, 68);
                colorLabel.SetTo(mBlobColorRgba);

                Mat spectrumLabel = mRgba.Submat(4, 4 + mSpectrum.Rows(), 70, 70 + mSpectrum.Cols());
                mSpectrum.CopyTo(spectrumLabel);
            }

            return mRgba;
        }

        private Scalar ConvertScalarHsv2Rgba(Scalar hsvColor)
        {
            Mat pointMatRgba = new Mat();
            Mat pointMatHsv = new Mat(1, 1, CvType.Cv8uc3, hsvColor);
            Imgproc.CvtColor(pointMatHsv, pointMatRgba, Imgproc.ColorHsv2rgbFull, 4);

            return new Scalar(pointMatRgba.Get(0, 0));
        }
    }

    class Callback : BaseLoaderCallback
    {
        private readonly ColorBlobDetectionActivity _activity;
        public Callback(ColorBlobDetectionActivity activity, Context context)
            : base(context)
        {
            _activity = activity;
        }

        public override void OnManagerConnected(int status)
        {
            switch (status)
            {
                case LoaderCallbackInterface.Success:
                    {
                        Log.Info(ActivityTags.ColorBlobDetection, "OpenCV loaded successfully");
                        _activity.mOpenCvCameraView.EnableView();
                       _activity.mOpenCvCameraView.SetOnTouchListener(_activity);
                    }
                    break;
                default:
                    {
                        base.OnManagerConnected(status);
                    }
                    break;
            }
        }
    }

}