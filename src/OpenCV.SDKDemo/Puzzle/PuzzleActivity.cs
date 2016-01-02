
using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using OpenCV.Android;
using OpenCV.Core;
using OpenCV.SDKDemo.Puzzle;
using OpenCV.SDKDemo.Utilities;

namespace OpenCV.SDKDemo.Puzzle
{
    [Activity(Label = ActivityTags.Puzzle)]
    public class PuzzleActivity : Activity, CameraBridgeViewBase.ICvCameraViewListener, ILoaderCallbackInterface, View.IOnTouchListener
    {
        private int _gameWidth;
        private int _gameHeight;
        private IMenuItem _itemHideNumbers;
        private IMenuItem _itemStartNewGame;
        private CameraBridgeViewBase _openCvCameraView;
        private Puzzle15Processor _puzzle15;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            Log.Debug(ActivityTags.Puzzle, "Creating and setting view");
            _openCvCameraView = new JavaCameraView(this, -1) as CameraBridgeViewBase;
            SetContentView(_openCvCameraView);
            _openCvCameraView.Visibility = ViewStates.Visible;
            _openCvCameraView.SetCvCameraViewListener(this);
            _puzzle15 = new Puzzle15Processor();
            _puzzle15.PrepareNewGame();
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (_openCvCameraView != null)
            {
                _openCvCameraView.DisableView();
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (!OpenCVLoader.InitDebug())
            {
                Log.Debug(Utilities.ActivityTags.Puzzle, "Internal OpenCV library not found. Using OpenCV Manager for initialization");
                OpenCVLoader.InitAsync(OpenCVLoader.OpencvVersion300, this, this);
            }
            else
            {
                Log.Debug(Utilities.ActivityTags.Puzzle, "OpenCV library found inside package. Using it!");
                OnManagerConnected(LoaderCallbackInterface.Success);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_openCvCameraView != null)
            {
                _openCvCameraView.DisableView();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            Log.Info(ActivityTags.Puzzle, "called onCreateOptionsMenu");
            _itemHideNumbers = menu.Add("Show/hide tile members");
            _itemStartNewGame = menu.Add("Start new game");
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Log.Info(ActivityTags.Puzzle, "Menu Item selected " + item);

            if (item == _itemStartNewGame)
            {
                _puzzle15.PrepareNewGame();
            }
            else if (item == _itemHideNumbers)
            {
                _puzzle15.ToggleTileNumbers();
            }

            return true;
        }

        public Mat OnCameraFrame(Mat p0)
        {
            return _puzzle15.PuzzleFrame(p0);
        }

        public void OnCameraViewStarted(int p0, int p1)
        {
            _gameWidth = p0;
            _gameHeight = p1;
            _puzzle15.PrepareGameSize(p0, p1);
        }

        public void OnCameraViewStopped()
        {
        }

        public void OnManagerConnected(int p0)
        {
            switch (p0)
            {
                case LoaderCallbackInterface.Success:
                    Log.Info(ActivityTags.Puzzle, "OpenCV loaded successfully");
                    _openCvCameraView.SetOnTouchListener(this);
                    _openCvCameraView.EnableView();
                    break;
                default:
                    break;
            }
        }

        public void OnPackageInstall(int p0, IInstallCallbackInterface p1)
        {
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            int xpos, ypos;

            xpos = (v.Width - _gameWidth) / 2;
            xpos = (int)e.GetX() - xpos;

            ypos = (v.Height - _gameHeight) / 2;
            ypos = (int)e.GetY() - ypos;

            if (xpos >= 0 && xpos <= _gameWidth && ypos >= 0 && ypos <= _gameHeight)
            {
                /* click is inside the picture. Deliver this event to processor */
                _puzzle15.DeliverTouchEvent(xpos, ypos);
            }

            return false;
        }
    }
}
