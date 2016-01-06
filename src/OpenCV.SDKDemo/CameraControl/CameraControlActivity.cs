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
using Size = Android.Hardware.Camera.Size;
using Java.Text;
using Java.Util;
using Java.Lang;
using JavaObject = Java.Lang.Object;

namespace OpenCV.SDKDemo.CameraControl
{
    [Activity(Label = "CameraControlActivity")]
    public class CameraControlActivity : Activity, View.IOnTouchListener, CameraBridgeViewBase.ICvCameraViewListener2
    {
        public const string TAG = "OCVSample::Activity";

        private CameraControlView mOpenCvCameraView;
        private List<Size> mResolutionList;
        private IMenuItem[] mEffectMenuItems;
        private ISubMenu mColorEffectsMenu;
        private IMenuItem[] mResolutionMenuItems;
        private ISubMenu mResolutionMenu;
        private BaseLoaderCallback mLoaderCallback;

        public CameraControlActivity()
        {
            Log.Info(TAG, "Instantiated new " + GetType().ToString());
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Info(TAG, "called onCreate");
            base.OnCreate(savedInstanceState);

            Window.AddFlags(WindowManagerFlags.KeepScreenOn);

            SetContentView(Resource.Layout.tutorial3_surface_view);

            mOpenCvCameraView = FindViewById<CameraControlView>(Resource.Id.tutorial3_activity_java_surface_view);

            mOpenCvCameraView.Visibility = ViewStates.Visible;

            mOpenCvCameraView.SetCvCameraViewListener2(this);
            mLoaderCallback = new Callback(this, mOpenCvCameraView, this);
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
        }

        public void OnCameraViewStopped()
        {
        }

        public Mat OnCameraFrame(CameraBridgeViewBase.ICvCameraViewFrame inputFrame)
        {
            return inputFrame.Rgba();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            IList<string> effects = mOpenCvCameraView.getEffectList();

            if (effects == null)
            {
                Log.Error(TAG, "Color effects are not supported by device!");
                return true;
            }

            mColorEffectsMenu = menu.AddSubMenu("Color Effect");
            mEffectMenuItems = new IMenuItem[effects.Count];

            int idx = 0;
            foreach (var item in effects)
            {
                string element = item;
                mEffectMenuItems[idx] = mColorEffectsMenu.Add(1, idx, Menu.None, element);
                idx++;
            }

            mResolutionMenu = menu.AddSubMenu("Resolution");
            mResolutionList = mOpenCvCameraView.getResolutionList();
            mResolutionMenuItems = new IMenuItem[mResolutionList.Count];


            idx = 0;
            foreach (var item in mResolutionList)
            {
                Size element = item;
                mResolutionMenuItems[idx] = mResolutionMenu.Add(2, idx, Menu.None,
                        element.Width.ToString() + "x" + element.Height.ToString());
                idx++;
            }

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Log.Info(TAG, "called onOptionsItemSelected; selected item: " + item);
            if (item.GroupId == 1)
            {
                mOpenCvCameraView.setEffect((string)item.TitleFormatted.ToString());
                Toast.MakeText(this, mOpenCvCameraView.getEffect(), ToastLength.Short).Show();
            }
            else if (item.GroupId == 2)
            {
                int id = item.ItemId;
                Size resolution = mResolutionList[id];
                mOpenCvCameraView.setResolution(resolution);
                resolution = mOpenCvCameraView.getResolution();
                string caption = resolution.Width.ToString() + "x" + resolution.Height.ToString();
                Toast.MakeText(this, caption, ToastLength.Short).Show();
            }

            return true;
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            Log.Info(TAG, "onTouch event");
            SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd_HH-mm-ss");
            string currentDateandTime = sdf.Format(new Date());
            string fileName = global::Android.OS.Environment.ExternalStorageDirectory.Path +
                                   "/sample_picture_" + currentDateandTime + ".jpg";
            mOpenCvCameraView.TakePicture(fileName);
            Toast.MakeText(this, fileName + " saved", ToastLength.Short).Show();
            return false;
        }
    }

    class Callback : BaseLoaderCallback
    {
        private readonly Context _context;
        private readonly CameraControlView mOpenCvCameraView;
        private readonly View.IOnTouchListener _listener;

        public Callback(Context context, CameraControlView view, View.IOnTouchListener listener)
            : base(context)
        {
            _context = context;
            mOpenCvCameraView = view;
            _listener = listener;
        }
        public override void OnManagerConnected(int status)
        {
            switch (status)
            {
                case LoaderCallbackInterface.Success:
                    {
                        Log.Info(CameraControlActivity.TAG, "OpenCV loaded successfully");
                        mOpenCvCameraView.EnableView();
                        mOpenCvCameraView.SetOnTouchListener(_listener);
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