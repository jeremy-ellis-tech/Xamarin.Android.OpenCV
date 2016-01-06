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
using Android.Util;

namespace OpenCV.SDKDemo.CameraCalibration
{
    public abstract class CalibrationResult
    {
        private const string TAG = "OCVSample::CalibrationResult";

        private static readonly int CAMERA_MATRIX_ROWS = 3;
        private static readonly int CAMERA_MATRIX_COLS = 3;
        private static readonly int DISTORTION_COEFFICIENTS_SIZE = 5;

        public static void save(Activity activity, Mat cameraMatrix, Mat distortionCoefficients)
        {
            ISharedPreferences sharedPref = activity.GetPreferences(FileCreationMode.Private);
            ISharedPreferencesEditor editor = sharedPref.Edit();

            double[] cameraMatrixArray = new double[CAMERA_MATRIX_ROWS * CAMERA_MATRIX_COLS];
            cameraMatrix.Get(0, 0, cameraMatrixArray);
            for (int i = 0; i < CAMERA_MATRIX_ROWS; i++)
            {
                for (int j = 0; j < CAMERA_MATRIX_COLS; j++)
                {
                    int id = i * CAMERA_MATRIX_ROWS + j;
                    editor.PutFloat(id.ToString(), (float)cameraMatrixArray[id]);
                }
            }

            double[] distortionCoefficientsArray = new double[DISTORTION_COEFFICIENTS_SIZE];
            distortionCoefficients.Get(0, 0, distortionCoefficientsArray);
            int shift = CAMERA_MATRIX_ROWS * CAMERA_MATRIX_COLS;
            for (int i = shift; i < DISTORTION_COEFFICIENTS_SIZE + shift; i++)
            {
                editor.PutFloat(i.ToString(), (float)distortionCoefficientsArray[i - shift]);
            }

            editor.Commit();
            Log.Info(TAG, "Saved camera matrix: " + cameraMatrix.Dump());
            Log.Info(TAG, "Saved distortion coefficients: " + distortionCoefficients.Dump());
        }

        public static bool tryLoad(Activity activity, Mat cameraMatrix, Mat distortionCoefficients)
        {
            ISharedPreferences sharedPref = activity.GetPreferences(FileCreationMode.Private);
            if (sharedPref.GetFloat("0", -1) == -1)
            {
                Log.Info(TAG, "No previous calibration results found");
                return false;
            }

            double[] cameraMatrixArray = new double[CAMERA_MATRIX_ROWS * CAMERA_MATRIX_COLS];
            for (int i = 0; i < CAMERA_MATRIX_ROWS; i++)
            {
                for (int j = 0; j < CAMERA_MATRIX_COLS; j++)
                {
                    int id = i * CAMERA_MATRIX_ROWS + j;
                    cameraMatrixArray[id] = sharedPref.GetFloat(id.ToString(), -1);
                }
            }

            cameraMatrix.Put(0, 0, cameraMatrixArray);

            Log.Info(TAG, "Loaded camera matrix: " + cameraMatrix.Dump());

            double[] distortionCoefficientsArray = new double[DISTORTION_COEFFICIENTS_SIZE];

            int shift = CAMERA_MATRIX_ROWS * CAMERA_MATRIX_COLS;

            for (int i = shift; i < DISTORTION_COEFFICIENTS_SIZE + shift; i++)
            {
                distortionCoefficientsArray[i - shift] = sharedPref.GetFloat(i.ToString(), -1);
            }

            distortionCoefficients.Put(0, 0, distortionCoefficientsArray);
            Log.Info(TAG, "Loaded distortion coefficients: " + distortionCoefficients.Dump());

            return true;
        }
    }
}