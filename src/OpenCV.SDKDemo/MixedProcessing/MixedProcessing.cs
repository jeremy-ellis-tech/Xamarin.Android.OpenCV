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
using OpenCV.Core;
using OpenCV.Android;
using Android.Util;
using Java.Lang;
using OpenCV.ImgProc;
using System.Runtime.InteropServices;

namespace OpenCV.SDKDemo.MixedProcessing
{
    [Activity(Label = "MixedProcessing")]
    public class MixedProcessingActivity : Activity, CameraBridgeViewBase.ICvCameraViewListener2
    {
        public const string TAG = "OCVSample::Activity";

        private const int VIEW_MODE_RGBA = 0;
        private const int VIEW_MODE_GRAY = 1;
        private const int VIEW_MODE_CANNY = 2;
        private const int VIEW_MODE_FEATURES = 5;

        private int mViewMode;
        private Mat mRgba;
        private Mat mIntermediateMat;
        private Mat mGray;

        private IMenuItem mItemPreviewRGBA;
        private IMenuItem mItemPreviewGray;
        private IMenuItem mItemPreviewCanny;
        private IMenuItem mItemPreviewFeatures;

        public CameraBridgeViewBase mOpenCvCameraView { get; private set; }

        private BaseLoaderCallback mLoaderCallback;

        public MixedProcessingActivity()
        {
            Log.Info(TAG, "Instantiated new " + GetType().ToString());
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Info(TAG, "called onCreate");
            base.OnCreate(savedInstanceState);
            mLoaderCallback = new Callback(this, this);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            SetContentView(Resource.Layout.tutorial2_surface_view);

            mOpenCvCameraView = FindViewById<CameraBridgeViewBase>(Resource.Id.tutorial2_activity_surface_view);
            mOpenCvCameraView.Visibility = ViewStates.Visible;
            mOpenCvCameraView.SetCvCameraViewListener2(this);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            Log.Info(TAG, "called onCreateOptionsMenu");
            mItemPreviewRGBA = menu.Add("Preview RGBA");
            mItemPreviewGray = menu.Add("Preview GRAY");
            mItemPreviewCanny = menu.Add("Canny");
            mItemPreviewFeatures = menu.Add("Find features");
            return true;
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

        public void OnCameraViewStarted(int width, int height)
        {
            mRgba = new Mat(height, width, CvType.Cv8uc4);
            mIntermediateMat = new Mat(height, width, CvType.Cv8uc4);
            mGray = new Mat(height, width, CvType.Cv8uc4);
        }

        public void OnCameraViewStopped()
        {
            mRgba.Release();
            mGray.Release();
            mIntermediateMat.Release();
        }

        public Mat OnCameraFrame(CameraBridgeViewBase.ICvCameraViewFrame inputFrame)
        {
            int viewMode = mViewMode;
            switch (viewMode)
            {
                case VIEW_MODE_GRAY:
                    // input frame has gray scale format
                    Imgproc.CvtColor(inputFrame.Gray(), mRgba, Imgproc.ColorGray2rgba, 4);
                    break;
                case VIEW_MODE_RGBA:
                    // input frame has RBGA format
                    mRgba = inputFrame.Rgba();
                    break;
                case VIEW_MODE_CANNY:
                    // input frame has gray scale format
                    mRgba = inputFrame.Rgba();
                    Imgproc.Canny(inputFrame.Gray(), mIntermediateMat, 80, 100);
                    Imgproc.CvtColor(mIntermediateMat, mRgba, Imgproc.ColorGray2rgba, 4);
                    break;
                case VIEW_MODE_FEATURES:
                    // input frame has RGBA format
                    mRgba = inputFrame.Rgba();
                    mGray = inputFrame.Gray();
                    FindFeatures(JNIEnv.Handle, JNIEnv.FindClass(typeof(Java.Lang.Object)), mGray.NativeObjAddr, mRgba.NativeObjAddr);
                    break;
            }

            return mRgba;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Log.Info(TAG, "called onOptionsItemSelected; selected item: " + item);

            if (item == mItemPreviewRGBA)
            {
                mViewMode = VIEW_MODE_RGBA;
            }
            else if (item == mItemPreviewGray)
            {
                mViewMode = VIEW_MODE_GRAY;
            }
            else if (item == mItemPreviewCanny)
            {
                mViewMode = VIEW_MODE_CANNY;
            }
            else if (item == mItemPreviewFeatures)
            {
                mViewMode = VIEW_MODE_FEATURES;
            }

            return true;
        }

        [DllImport("libmixed_sample", EntryPoint = "Java_org_opencv_samples_tutorial2_Tutorial2Activity_FindFeatures")]
        public static extern void FindFeatures(IntPtr jenv, IntPtr jclass, long matAddrGr, long matAddrRgba);
    }

    class Callback : BaseLoaderCallback
    {
        private readonly MixedProcessingActivity _activity;
        public Callback(Context context, MixedProcessingActivity activity)
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
                        Log.Info(MixedProcessingActivity.TAG, "OpenCV loaded successfully");

                        // Load native library after(!) OpenCV initialization
                        JavaSystem.LoadLibrary("mixed_sample");

                        _activity.mOpenCvCameraView.EnableView();
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