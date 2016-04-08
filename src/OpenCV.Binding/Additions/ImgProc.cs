using Android.Runtime;
using System;

namespace OpenCV.ImgProc
{
    public partial class Imgproc : global::Java.Lang.Object
    {
        /// <summary>
        /// Thanks to @CeadeS on github for spotting this.
        /// The countours are supposed to be passed as a ref/out parameter,
        /// in the default binding implementation these aren't set
        /// before the method returns.
        /// </summary>

        static IntPtr id_findContours_Lorg_opencv_core_Mat_Ljava_util_List_Lorg_opencv_core_Mat_II;
        // Metadata.xml XPath method reference: path="/api/package[@name='org.opencv.imgproc']/class[@name='Imgproc']/method[@name='findContours' and count(parameter)=5 and parameter[1][@type='org.opencv.core.Mat'] and parameter[2][@type='java.util.List&lt;org.opencv.core.MatOfPoint&gt;'] and parameter[3][@type='org.opencv.core.Mat'] and parameter[4][@type='int'] and parameter[5][@type='int']]"
        [Register("findContours", "(Lorg/opencv/core/Mat;Ljava/util/List;Lorg/opencv/core/Mat;II)V", "")]
        public static unsafe void FindContours(global::OpenCV.Core.Mat image, global::System.Collections.Generic.IList<global::OpenCV.Core.MatOfPoint> contours, global::OpenCV.Core.Mat hierarchy, int mode, int method)
        {
            if (id_findContours_Lorg_opencv_core_Mat_Ljava_util_List_Lorg_opencv_core_Mat_II == IntPtr.Zero)
                id_findContours_Lorg_opencv_core_Mat_Ljava_util_List_Lorg_opencv_core_Mat_II = JNIEnv.GetStaticMethodID(class_ref, "findContours", "(Lorg/opencv/core/Mat;Ljava/util/List;Lorg/opencv/core/Mat;II)V");
            IntPtr native_p1 = global::Android.Runtime.JavaList<global::OpenCV.Core.MatOfPoint>.ToLocalJniHandle(contours);
            try
            {
                JValue* __args = stackalloc JValue[5];
                __args[0] = new JValue(image);
                __args[1] = new JValue(native_p1);
                __args[2] = new JValue(hierarchy);
                __args[3] = new JValue(mode);
                __args[4] = new JValue(method);
                JNIEnv.CallStaticVoidMethod(class_ref, id_findContours_Lorg_opencv_core_Mat_Ljava_util_List_Lorg_opencv_core_Mat_II, __args);
                contours = JavaList<Core.MatOfPoint>.FromJniHandle(native_p1, JniHandleOwnership.TransferLocalRef);
            }
            finally
            {
                JNIEnv.DeleteLocalRef(native_p1);
            }
        }

        static IntPtr id_findContours_Lorg_opencv_core_Mat_Ljava_util_List_Lorg_opencv_core_Mat_IILorg_opencv_core_Point_;
        // Metadata.xml XPath method reference: path="/api/package[@name='org.opencv.imgproc']/class[@name='Imgproc']/method[@name='findContours' and count(parameter)=6 and parameter[1][@type='org.opencv.core.Mat'] and parameter[2][@type='java.util.List&lt;org.opencv.core.MatOfPoint&gt;'] and parameter[3][@type='org.opencv.core.Mat'] and parameter[4][@type='int'] and parameter[5][@type='int'] and parameter[6][@type='org.opencv.core.Point']]"
        [Register("findContours", "(Lorg/opencv/core/Mat;Ljava/util/List;Lorg/opencv/core/Mat;IILorg/opencv/core/Point;)V", "")]
        public static unsafe void FindContours(global::OpenCV.Core.Mat image, global::System.Collections.Generic.IList<global::OpenCV.Core.MatOfPoint> contours, global::OpenCV.Core.Mat hierarchy, int mode, int method, global::OpenCV.Core.Point offset)
        {
            if (id_findContours_Lorg_opencv_core_Mat_Ljava_util_List_Lorg_opencv_core_Mat_IILorg_opencv_core_Point_ == IntPtr.Zero)
                id_findContours_Lorg_opencv_core_Mat_Ljava_util_List_Lorg_opencv_core_Mat_IILorg_opencv_core_Point_ = JNIEnv.GetStaticMethodID(class_ref, "findContours", "(Lorg/opencv/core/Mat;Ljava/util/List;Lorg/opencv/core/Mat;IILorg/opencv/core/Point;)V");
            IntPtr native_p1 = global::Android.Runtime.JavaList<global::OpenCV.Core.MatOfPoint>.ToLocalJniHandle(contours);
            try
            {
                JValue* __args = stackalloc JValue[6];
                __args[0] = new JValue(image);
                __args[1] = new JValue(native_p1);
                __args[2] = new JValue(hierarchy);
                __args[3] = new JValue(mode);
                __args[4] = new JValue(method);
                __args[5] = new JValue(offset);
                JNIEnv.CallStaticVoidMethod(class_ref, id_findContours_Lorg_opencv_core_Mat_Ljava_util_List_Lorg_opencv_core_Mat_IILorg_opencv_core_Point_, __args);
                contours = JavaList<Core.MatOfPoint>.FromJniHandle(native_p1, JniHandleOwnership.TransferLocalRef);
            }
            finally
            {
                JNIEnv.DeleteLocalRef(native_p1);
            }
        }
    }
}