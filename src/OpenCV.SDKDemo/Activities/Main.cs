using Android.App;
using Android.OS;
using Android.Widget;

namespace OpenCV.SDKDemo.Activities
{
    [Activity(Label = "OpenCV.SDKDemo", MainLauncher = true, Icon = "@drawable/icon")]
    public class Main : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            Button button;
            button = FindViewById<Button>(Resource.Id.cameraPreview);
            button.Click += (s, e) => StartActivity(typeof(CameraPreview));
        }
    }
}

