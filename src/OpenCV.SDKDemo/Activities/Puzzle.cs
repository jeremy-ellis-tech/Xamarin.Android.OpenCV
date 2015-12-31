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
using OpenCV.SDKDemo.Utilities;
using OpenCv.Android;
using Android.Util;

namespace OpenCV.SDKDemo.Activities
{
    [Activity(Label = ActivityTags.Puzzle)]
    public class Puzzle : Activity
    {
        private int _gameWidth;
        private int _gameHeight;
        private IMenuItem _itemHideNumbers;
        private IMenuItem _itemStartNewGame;
        private CameraBridgeViewBase _openCvCameraView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            Log.Debug(ActivityTags.Puzzle, "Creating and setting view");
            _openCvCameraView = new JavaCameraView(this, -1) as CameraBridgeViewBase;
        }
    }
}