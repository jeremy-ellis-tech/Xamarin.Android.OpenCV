using Android.Content.Res;
using OpenCV.Android;
using OpenCV.Core;
using OpenCV.ImgProc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCV.SDKDemo.CameraCalibration
{
    abstract class FrameRender
    {
        protected CameraCalibrator mCalibrator;

        public abstract Mat Render(CameraBridgeViewBase.ICvCameraViewFrame inputFrame);
    }

    class PreviewFrameRender : FrameRender
    {
        public override Mat Render(CameraBridgeViewBase.ICvCameraViewFrame inputFrame)
        {
            return inputFrame.Rgba();
        }
    }

    class CalibrationFrameRender : FrameRender
    {
        public CalibrationFrameRender(CameraCalibrator calibrator)
        {
            mCalibrator = calibrator;
        }

        public override Mat Render(CameraBridgeViewBase.ICvCameraViewFrame inputFrame)
        {
            Mat rgbaFrame = inputFrame.Rgba();
            Mat grayFrame = inputFrame.Gray();
            mCalibrator.processFrame(grayFrame, rgbaFrame);

            return rgbaFrame;
        }
    }

    class UndistortionFrameRender : FrameRender
    {
        public UndistortionFrameRender(CameraCalibrator calibrator)
        {
            mCalibrator = calibrator;
        }

        public override Mat Render(CameraBridgeViewBase.ICvCameraViewFrame inputFrame)
        {
            Mat renderedFrame = new Mat(inputFrame.Rgba().Size(), inputFrame.Rgba().Type());
            Imgproc.Undistort(inputFrame.Rgba(), renderedFrame,
                    mCalibrator.getCameraMatrix(), mCalibrator.getDistortionCoefficients());

            return renderedFrame;
        }
    }

    class ComparisonFrameRender : FrameRender
    {
        private int mWidth;
        private int mHeight;
        private Resources mResources;
        public ComparisonFrameRender(CameraCalibrator calibrator, int width, int height, Resources resources)
        {
            mCalibrator = calibrator;
            mWidth = width;
            mHeight = height;
            mResources = resources;
        }

        public override Mat Render(CameraBridgeViewBase.ICvCameraViewFrame inputFrame)
        {
            Mat undistortedFrame = new Mat(inputFrame.Rgba().Size(), inputFrame.Rgba().Type());
            Imgproc.Undistort(inputFrame.Rgba(), undistortedFrame,
                    mCalibrator.getCameraMatrix(), mCalibrator.getDistortionCoefficients());

            Mat comparisonFrame = inputFrame.Rgba();
            undistortedFrame.ColRange(new Range(0, mWidth / 2)).CopyTo(comparisonFrame.ColRange(new Range(mWidth / 2, mWidth)));
            List<MatOfPoint> border = new List<MatOfPoint>();
            int shift = (int)(mWidth * 0.005);
            border.Add(new MatOfPoint(new Point(mWidth / 2 - shift, 0), new Point(mWidth / 2 + shift, 0),
                    new Point(mWidth / 2 + shift, mHeight), new Point(mWidth / 2 - shift, mHeight)));
            Imgproc.FillPoly(comparisonFrame, border, new Scalar(255, 255, 255));

            Imgproc.PutText(comparisonFrame, mResources.GetString(Resource.String.original), new Point(mWidth * 0.1, mHeight * 0.1),
                    Core.Core.FontHersheySimplex, 1.0, new Scalar(255, 255, 0));
            Imgproc.PutText(comparisonFrame, mResources.GetString(Resource.String.undistorted), new Point(mWidth * 0.6, mHeight * 0.1),
                    Core.Core.FontHersheySimplex, 1.0, new Scalar(255, 255, 0));

            return comparisonFrame;
        }
    }

    class OnCameraFrameRender
    {
        private FrameRender mFrameRender;
        public OnCameraFrameRender(FrameRender frameRender)
        {
            mFrameRender = frameRender;
        }
        public Mat Render(CameraBridgeViewBase.ICvCameraViewFrame inputFrame)
        {
            return mFrameRender.Render(inputFrame);
        }
    }
}
