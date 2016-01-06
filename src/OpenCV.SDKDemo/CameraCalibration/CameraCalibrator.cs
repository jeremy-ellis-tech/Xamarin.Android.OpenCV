using Android.Util;
using OpenCV.Core;
using OpenCV.ImgProc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Size = OpenCV.Core.Size;

namespace OpenCV.SDKDemo.CameraCalibration
{
    class CameraCalibrator
    {
        private const string TAG = "CameraCalibrator";

        private readonly Size mPatternSize = new Size(4, 11);
        private readonly int mCornersSize;
        private bool mPatternWasFound = false;
        private MatOfPoint2f mCorners = new MatOfPoint2f();
        private List<Mat> mCornersBuffer = new List<Mat>();
        private bool mIsCalibrated = false;

        private Mat mCameraMatrix = new Mat();
        private Mat mDistortionCoefficients = new Mat();
        private int mFlags;
        private double mRms;
        private double mSquareSize = 0.0181;
        private Size mImageSize;

        public CameraCalibrator(int width, int height)
        {
            mImageSize = new Size(width, height);
            mFlags = Calib3d.Calib3d.CalibFixPrincipalPoint +
                     Calib3d.Calib3d.CalibZeroTangentDist +
                     Calib3d.Calib3d.CalibFixAspectRatio +
                     Calib3d.Calib3d.CalibFixK4 +
                     Calib3d.Calib3d.CalibFixK5;
            Mat.Eye(3, 3, CvType.Cv64fc1).CopyTo(mCameraMatrix);
            mCameraMatrix.Put(0, 0, 1.0);
            Mat.Zeros(5, 1, CvType.Cv64fc1).CopyTo(mDistortionCoefficients);
            Log.Info(TAG, "Instantiated new " + GetType().ToString());
            mCornersSize = (int)(mPatternSize.Width * mPatternSize.Height);
        }

        public void processFrame(Mat grayFrame, Mat rgbaFrame)
        {
            findPattern(grayFrame);
            renderFrame(rgbaFrame);
        }

        public void calibrate()
        {
            List<Mat> rvecs = new List<Mat>();
            List<Mat> tvecs = new List<Mat>();
            Mat reprojectionErrors = new Mat();
            List<Mat> objectPoints = new List<Mat>();
            objectPoints.Add(Mat.Zeros(mCornersSize, 1, CvType.Cv32fc3));
            calcBoardCornerPositions(objectPoints[0]);
            for (int i = 1; i < mCornersBuffer.Count; i++)
            {
                objectPoints.Add(objectPoints[0]);
            }

            Calib3d.Calib3d.CalibrateCamera(objectPoints, mCornersBuffer, mImageSize,
                    mCameraMatrix, mDistortionCoefficients, rvecs, tvecs, mFlags);

            mIsCalibrated = Core.Core.CheckRange(mCameraMatrix)
                    && Core.Core.CheckRange(mDistortionCoefficients);

            mRms = computeReprojectionErrors(objectPoints, rvecs, tvecs, reprojectionErrors);
            Log.Info(TAG, String.Format("Average re-projection error: %f", mRms));
            Log.Info(TAG, "Camera matrix: " + mCameraMatrix.Dump());
            Log.Info(TAG, "Distortion coefficients: " + mDistortionCoefficients.Dump());
        }

        public void clearCorners()
        {
            mCornersBuffer.Clear();
        }

        private void calcBoardCornerPositions(Mat corners)
        {
            const int cn = 3;
            float[] positions = new float[mCornersSize * cn];

            for (int i = 0; i < mPatternSize.Height; i++)
            {
                for (int j = 0; j < mPatternSize.Width * cn; j += cn)
                {
                    positions[(int)(i * mPatternSize.Width * cn + j + 0)] =
                            (2 * (j / cn) + i % 2) * (float)mSquareSize;
                    positions[(int)(i * mPatternSize.Width * cn + j + 1)] =
                            i * (float)mSquareSize;
                    positions[(int)(i * mPatternSize.Width * cn + j + 2)] = 0;
                }
            }
            corners.Create(mCornersSize, 1, CvType.Cv32fc3);
            corners.Put(0, 0, positions);
        }

        private double computeReprojectionErrors(List<Mat> objectPoints,
                List<Mat> rvecs, List<Mat> tvecs, Mat perViewErrors)
        {
            MatOfPoint2f cornersProjected = new MatOfPoint2f();
            double totalError = 0;
            double error;
            float[] viewErrors = new float[objectPoints.Count];

            MatOfDouble distortionCoefficients = new MatOfDouble(mDistortionCoefficients);
            int totalPoints = 0;
            for (int i = 0; i < objectPoints.Count; i++)
            {
                MatOfPoint3f points = new MatOfPoint3f(objectPoints[i]);
                Calib3d.Calib3d.ProjectPoints(points, rvecs[i], tvecs[i],
                        mCameraMatrix, distortionCoefficients, cornersProjected);
                error = Core.Core.Norm(mCornersBuffer[i], cornersProjected, Core.Core.NormL2);

                int n = objectPoints[i].Rows();
                viewErrors[i] = (float)Math.Sqrt(error * error / n);
                totalError += error * error;
                totalPoints += n;
            }
            perViewErrors.Create(objectPoints.Count, 1, CvType.Cv32fc1);
            perViewErrors.Put(0, 0, viewErrors);

            return Math.Sqrt(totalError / totalPoints);
        }

        private void findPattern(Mat grayFrame)
        {
            mPatternWasFound = Calib3d.Calib3d.FindCirclesGrid(grayFrame, mPatternSize,
                    mCorners, Calib3d.Calib3d.CalibCbAsymmetricGrid);
        }

        public void addCorners()
        {
            if (mPatternWasFound)
            {
                mCornersBuffer.Add(mCorners.Clone());
            }
        }

        private void drawPoints(Mat rgbaFrame)
        {
            Calib3d.Calib3d.DrawChessboardCorners(rgbaFrame, mPatternSize, mCorners, mPatternWasFound);
        }

        private void renderFrame(Mat rgbaFrame)
        {
            drawPoints(rgbaFrame);

            Imgproc.PutText(rgbaFrame, "Captured: " + mCornersBuffer.Count, new Point(rgbaFrame.Cols() / 3 * 2, rgbaFrame.Rows() * 0.1),
                    Core.Core.FontHersheySimplex, 1.0, new Scalar(255, 255, 0));
        }

        public Mat getCameraMatrix()
        {
            return mCameraMatrix;
        }

        public Mat getDistortionCoefficients()
        {
            return mDistortionCoefficients;
        }

        public int getCornersBufferSize()
        {
            return mCornersBuffer.Count;
        }

        public double getAvgReprojectionError()
        {
            return mRms;
        }

        public bool isCalibrated()
        {
            return mIsCalibrated;
        }

        public void setCalibrated()
        {
            mIsCalibrated = true;
        }
    }
}
