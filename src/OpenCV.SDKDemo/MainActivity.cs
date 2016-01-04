using Android.App;
using Android.OS;
using Android.Widget;
using OpenCV.SDKDemo.CameraCalibration;
using OpenCV.SDKDemo.CameraPreview;
using OpenCV.SDKDemo.ColorBlobDetection;
using OpenCV.SDKDemo.Puzzle;

namespace OpenCV.SDKDemo
{
    [Activity(Label = "OpenCV.SDKDemo", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            FindViewById<Button>(Resource.Id.cameraPreview)
                .Click += (s, e) => StartActivity(typeof(CameraPreviewActivity));

            FindViewById<Button>(Resource.Id.mixedProcessing)
                .Click += (s, e) => Toast.MakeText(this, "Not implemented", ToastLength.Short);

            FindViewById<Button>(Resource.Id.cameraControl)
                .Click += (s, e) => Toast.MakeText(this, "Not implemented", ToastLength.Short);

            FindViewById<Button>(Resource.Id.imageManipulations)
                .Click += (s, e) => Toast.MakeText(this, "Not implemented", ToastLength.Short);

            FindViewById<Button>(Resource.Id.faceDetection)
                .Click += (s, e) => Toast.MakeText(this, "Not implemented", ToastLength.Short);

            FindViewById<Button>(Resource.Id.colorBlobDetection)
                .Click += (s, e) => StartActivity(typeof(ColorBlobDetectionActivity));

            FindViewById<Button>(Resource.Id.cameraCalibration)
                .Click += (s, e) => StartActivity(typeof(CameraCalibrationActivity));

            FindViewById<Button>(Resource.Id.puzzle)
                .Click += (s, e) => StartActivity(typeof(PuzzleActivity));
        }
    }
}

