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
using OpenCV.Android;
using Android.Util;
using OpenCV.SDKDemo.Utilities;
using OpenCV.Core;
using System.Threading.Tasks;

namespace OpenCV.SDKDemo.CameraCalibration
{
    [Activity(Label = ActivityTags.CameraCalibration)]
    public class CameraCalibrationActivity : Activity, CameraBridgeViewBase.ICvCameraViewListener2, View.IOnTouchListener
    {
        public const string TAG = "OCVSample::Activity";

        public CameraBridgeViewBase mOpenCvCameraView { get; private set; }
        private CameraCalibrator mCalibrator;
        private OnCameraFrameRender mOnCameraFrameRender;
        private int mWidth;
        private int mHeight;
        private Callback mLoaderCallback;

        public CameraCalibrationActivity()
        {
            Log.Info(TAG, "Instantiated new " + GetType().ToString());
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Info(TAG, "called onCreate");
            base.OnCreate(savedInstanceState);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            SetContentView(Resource.Layout.camera_calibration_surface_view);

            mOpenCvCameraView = FindViewById<CameraBridgeViewBase>(Resource.Id.camera_calibration_java_surface_view);
            mOpenCvCameraView.Visibility = ViewStates.Visible;
            mOpenCvCameraView.SetCvCameraViewListener2(this);
            mLoaderCallback = new Callback(this, this, this);
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
                Log.Debug(TAG, "Internal OpenCV library not found. Using OpenCV Manager for initialization");
                OpenCVLoader.InitAsync(OpenCVLoader.OpencvVersion300, this, mLoaderCallback);
            }
            else
            {
                Log.Debug(TAG, "OpenCV library found inside package. Using it!");
                mLoaderCallback.OnManagerConnected(LoaderCallbackInterface.Success);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (mOpenCvCameraView != null)
                mOpenCvCameraView.DisableView();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            base.OnCreateOptionsMenu(menu);

            MenuInflater.Inflate(Resource.Menu.calibration, menu);

            return true;
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            base.OnPrepareOptionsMenu(menu);
            menu.FindItem(Resource.Id.preview_mode).SetEnabled(true);
            if (!mCalibrator.isCalibrated())
                menu.FindItem(Resource.Id.preview_mode).SetEnabled(false);

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.calibration:
                    mOnCameraFrameRender =
                        new OnCameraFrameRender(new CalibrationFrameRender(mCalibrator));
                    item.SetChecked(true);
                    return true;
                case Resource.Id.undistortion:
                    mOnCameraFrameRender =
                        new OnCameraFrameRender(new UndistortionFrameRender(mCalibrator));
                    item.SetChecked(true);
                    return true;
                case Resource.Id.comparison:
                    mOnCameraFrameRender =
                        new OnCameraFrameRender(new ComparisonFrameRender(mCalibrator, mWidth, mHeight, Resources));
                    item.SetChecked(true);
                    return true;
                case Resource.Id.calibrate:
                    var res = this.Resources;
                    if (mCalibrator.getCornersBufferSize() < 2)
                    {
                        (Toast.MakeText(this, res.GetString(Resource.String.more_samples), ToastLength.Short)).Show();
                        return true;
                    }

                    mOnCameraFrameRender = new OnCameraFrameRender(new PreviewFrameRender());

                    //OnPreExecute
                    var calibrationProgress = new ProgressDialog(this);
                    calibrationProgress.SetTitle(Resources.GetString(Resource.String.calibrating));
                    calibrationProgress.SetMessage(Resources.GetString(Resource.String.please_wait));
                    calibrationProgress.SetCancelable(false);
                    calibrationProgress.Indeterminate = true;
                    calibrationProgress.Show();

                    Task.Run(() => mCalibrator.calibrate())
                        //OnPostExecute
                        .ContinueWith(t =>
                        {
                            calibrationProgress.Dismiss();
                            mCalibrator.clearCorners();
                            mOnCameraFrameRender = new OnCameraFrameRender(new CalibrationFrameRender(mCalibrator));
                            String resultMessage = (mCalibrator.isCalibrated()) ?
                                    Resources.GetString(Resource.String.calibration_successful) + " " + mCalibrator.getAvgReprojectionError() :
                                    Resources.GetString(Resource.String.calibration_unsuccessful);
                            Toast.MakeText(this, resultMessage, ToastLength.Short).Show();

                            if (mCalibrator.isCalibrated())
                            {
                                CalibrationResult.save(this,
                                        mCalibrator.getCameraMatrix(), mCalibrator.getDistortionCoefficients());
                            }
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public void OnCameraViewStarted(int width, int height)
        {
            if (mWidth != width || mHeight != height)
            {
                mWidth = width;
                mHeight = height;
                mCalibrator = new CameraCalibrator(mWidth, mHeight);
                if (CalibrationResult.tryLoad(this, mCalibrator.getCameraMatrix(), mCalibrator.getDistortionCoefficients()))
                {
                    mCalibrator.setCalibrated();
                }

                mOnCameraFrameRender = new OnCameraFrameRender(new CalibrationFrameRender(mCalibrator));
            }
        }

        public void OnCameraViewStopped()
        {
        }

        public Mat OnCameraFrame(CameraBridgeViewBase.ICvCameraViewFrame inputFrame)
        {
            return mOnCameraFrameRender.Render(inputFrame);
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            Log.Debug(TAG, "onTouch invoked");

            mCalibrator.addCorners();
            return false;
        }
    }

    class Callback : BaseLoaderCallback
    {
        private readonly View.IOnTouchListener _listener;
        private readonly CameraCalibrationActivity _activity;
        public Callback(Context context, View.IOnTouchListener listener, CameraCalibrationActivity activity)
            : base(context)
        {
            _listener = listener;
            _activity = activity;
        }
        public override void OnManagerConnected(int status)
        {
            switch (status)
            {
                case LoaderCallbackInterface.Success:
                    {
                        Log.Info(CameraCalibrationActivity.TAG, "OpenCV loaded successfully");
                        _activity.mOpenCvCameraView.EnableView();
                        _activity.mOpenCvCameraView.SetOnTouchListener(_listener);
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