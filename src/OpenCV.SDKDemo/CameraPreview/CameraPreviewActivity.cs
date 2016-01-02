
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Views;
using OpenCV.SDKDemo.Utilities;
using OpenCV.Core;
using OpenCV.Android;
using OpenCV;

namespace OpenCV.SDKDemo.CameraPreview
{
    [Activity(Label = ActivityTags.CameraPreview,
        ScreenOrientation=ScreenOrientation.Landscape,
        ConfigurationChanges=ConfigChanges.KeyboardHidden|ConfigChanges.Orientation
        //,Theme="@android:style/Theme.NoTitleBar.FullScreen"
        )]
    public class CameraPreviewActivity : Activity, ILoaderCallbackInterface, CameraBridgeViewBase.ICvCameraViewListener
    {
        private CameraBridgeViewBase _openCvCameraView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            SetContentView(Resource.Layout.CameraPreview);
            _openCvCameraView = FindViewById<CameraBridgeViewBase>(Resource.Id.surfaceView);
            _openCvCameraView.Visibility = ViewStates.Visible;
            _openCvCameraView.SetCvCameraViewListener(this);
        }

        protected override void OnPause()
        {
            base.OnPause();
            if(_openCvCameraView != null)
            {
                _openCvCameraView.DisableView();
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            if(!OpenCVLoader.InitDebug())
            {
                Log.Debug(ActivityTags.CameraPreview, "Internal OpenCV library not found. Using OpenCV Manager for initialization");
                OpenCVLoader.InitAsync(OpenCVLoader.OpencvVersion300, this, this);
            }
            else
            {
                Log.Debug(ActivityTags.CameraPreview, "OpenCV library found inside package. Using it!");
                OnManagerConnected(LoaderCallbackInterface.Success);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(_openCvCameraView != null)
            {
                _openCvCameraView.DisableView();
            }
        }

        public void OnManagerConnected(int p0)
        {
            switch (p0)
            {
                case LoaderCallbackInterface.Success:
                    Log.Info(ActivityTags.CameraPreview, "OpenCV loaded successfully");
                    _openCvCameraView.EnableView();
                    break;
                default:
                    break;
            }
        }

        public void OnPackageInstall(int p0, IInstallCallbackInterface p1)
        {

        }

        public void OnCameraViewStarted(int p0, int p1)
        {
            
        }

        public void OnCameraViewStopped()
        {
            
        }

        public Mat OnCameraFrame(Mat p0)
        {
            return p0;
        }
    }
}
