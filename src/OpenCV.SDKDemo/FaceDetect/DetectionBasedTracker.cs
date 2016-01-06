using Android.Runtime;
using OpenCV.Core;
using System;
using System.Runtime.InteropServices;

namespace OpenCV.SDKDemo.FaceDetect
{
    public class DetectionBasedTracker
    {
        private IntPtr mNativeObj = IntPtr.Zero;

        public DetectionBasedTracker(string cascadeName, int minFaceSize)
        {
            Java.Lang.String s = new Java.Lang.String(cascadeName);
            mNativeObj = nativeCreateObject(JNIEnv.Handle, JNIEnv.FindClass(typeof(Java.Lang.Object)), s.Handle, minFaceSize);
        }

        public void start()
        {
            nativeStart(JNIEnv.Handle, JNIEnv.FindClass(typeof(Java.Lang.Object)), mNativeObj);
        }

        public void stop()
        {
            nativeStop(JNIEnv.Handle, JNIEnv.FindClass(typeof(Java.Lang.Object)), mNativeObj);
        }

        public void setMinFaceSize(int size)
        {
            nativeSetFaceSize(JNIEnv.Handle, JNIEnv.FindClass(typeof(Java.Lang.Object)), mNativeObj, size);
        }

        public void detect(Mat imageGray, MatOfRect faces)
        {
            nativeDetect(JNIEnv.Handle, JNIEnv.FindClass(typeof(Java.Lang.Object)), mNativeObj, imageGray.NativeObjAddr, faces.NativeObjAddr);
        }

        public void release()
        {
            nativeDestroyObject(JNIEnv.Handle, JNIEnv.FindClass(typeof(Java.Lang.Object)), mNativeObj);
            mNativeObj = IntPtr.Zero;
        }

        [DllImport("libdetection_based_tracker", EntryPoint = "Java_org_opencv_samples_facedetect_DetectionBasedTracker_nativeCreateObject")]
        private extern static IntPtr nativeCreateObject(IntPtr env, IntPtr jniClass, IntPtr cascadeName, int minFaceSize);

        [DllImport("libdetection_based_tracker", EntryPoint = "Java_org_opencv_samples_facedetect_DetectionBasedTracker_nativeDestroyObject")]
        private extern static void nativeDestroyObject(IntPtr env, IntPtr jniClass, IntPtr thiz);

        [DllImport("libdetection_based_tracker", EntryPoint = "Java_org_opencv_samples_facedetect_DetectionBasedTracker_nativeStart")]
        private extern static void nativeStart(IntPtr env, IntPtr jniClass, IntPtr thiz);

        [DllImport("libdetection_based_tracker", EntryPoint = "Java_org_opencv_samples_facedetect_DetectionBasedTracker_nativeStop")]
        private extern static void nativeStop(IntPtr env, IntPtr jniClass, IntPtr thiz);

        [DllImport("libdetection_based_tracker", EntryPoint = "Java_org_opencv_samples_facedetect_DetectionBasedTracker_nativeSetFaceSize")]
        private extern static void nativeSetFaceSize(IntPtr env, IntPtr jniClass, IntPtr thiz, int size);

        [DllImport("libdetection_based_tracker", EntryPoint = "Java_org_opencv_samples_facedetect_DetectionBasedTracker_nativeDetect")]
        private extern static void nativeDetect(IntPtr env, IntPtr jniClass, IntPtr thiz, long inputImage, long faces);
    }
}