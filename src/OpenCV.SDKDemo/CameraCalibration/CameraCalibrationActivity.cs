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

namespace OpenCV.SDKDemo.CameraCalibration
{
    [Activity(Label = ActivityTags.CameraCalibration)]
    public class CameraCalibrationActivity : Activity//, CameraBridgeViewBase.ICvCameraViewListener2, View.IOnTouchListener
    {
        //private CameraBridgeViewBase _openCvCameraView;
        //private CameraCalibrator _calibrator;
        //private OnCameraFrameRender _onCameraFrameReder;
        //private int _width;
        //private int _height;

        //protected override void OnCreate(Bundle savedInstanceState)
        //{
        //    base.OnCreate(savedInstanceState);
        //    Window.AddFlags(WindowManagerFlags.KeepScreenOn);
        //    SetContentView(Resource.Layout.CameraCalibration);
        //    _openCvCameraView = FindViewById<CameraBridgeViewBase>(Resource.Id.camera_calibration_java_surface_view);
        //    _openCvCameraView.Visibility = ViewStates.Visible;
        //    _openCvCameraView.SetCvCameraViewListener(this);
        //}

        //protected override void OnPause()
        //{
        //    base.OnPause();
        //    if (_openCvCameraView != null)
        //    {
        //        _openCvCameraView.DisableView();
        //    }
        //}

        //protected override void OnResume()
        //{
        //    base.OnResume();
        //    if (!OpenCVLoader.InitDebug())
        //    {
        //        Log.Debug(ActivityTags.CameraCalibration, "");
        //        OpenCVLoader.InitAsync(OpenCVLoader.OpencvVersion300, this, this);
        //    }
        //    else
        //    {
        //        Log.Debug(ActivityTags.CameraCalibration, "");
        //    }
        //}

        //public bool OnTouch(View v, MotionEvent e)
        //{
        //    throw new NotImplementedException();
        //}

        //public void OnCameraViewStarted(int p0, int p1)
        //{
        //    throw new NotImplementedException();
        //}

        //public void OnCameraViewStopped()
        //{
        //    throw new NotImplementedException();
        //}
    }
}