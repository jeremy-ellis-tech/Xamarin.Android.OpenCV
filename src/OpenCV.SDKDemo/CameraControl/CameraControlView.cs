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
using OpenCV.Android;
using OpenCV.Core;
using Android.Hardware;
using Android.Util;
using Java.IO;
using Size = Android.Hardware.Camera.Size;

namespace OpenCV.SDKDemo.CameraControl
{
    public class CameraControlView : JavaCameraView, Camera.IPictureCallback
    {
        private const string TAG = "CameraControlView";
        private String mPictureFileName;

        public CameraControlView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        public IList<String> getEffectList()
        {
            return MCamera.GetParameters().SupportedColorEffects;
        }

        public bool isEffectSupported()
        {
            return (MCamera.GetParameters().ColorEffect != null);
        }

        public String getEffect()
        {
            return MCamera.GetParameters().ColorEffect;
        }

        public void setEffect(String effect)
        {
            Camera.Parameters param = MCamera.GetParameters();
            param.ColorEffect = effect;
            MCamera.SetParameters(param);
        }

        public List<Size> getResolutionList()
        {
            return MCamera.GetParameters().SupportedPreviewSizes.ToList();
        }

        public void setResolution(Size resolution)
        {
            DisconnectCamera();
            MMaxHeight = (int)resolution.Height;
            MMaxWidth = (int)resolution.Width;
            ConnectCamera(Width, Height);
        }

        public Size getResolution()
        {
            return MCamera.GetParameters().PreviewSize;
        }

        public void TakePicture(string fileName)
        {
            Log.Info(TAG, "Taking picture");
            this.mPictureFileName = fileName;
            // Postview and jpeg are sent in the same buffers if the queue is not empty when performing a capture.
            // Clear up buffers to avoid mCamera.takePicture to be stuck because of a memory issue
            MCamera.SetPreviewCallback(null);

            // PictureCallback is implemented by the current class
            MCamera.TakePicture(null, null, this);
        }

        public void OnPictureTaken(byte[] data, Camera camera)
        {
            Log.Info(TAG, "Saving a bitmap to file");
            // The camera preview was automatically stopped. Start it again.
            MCamera.StartPreview();
            MCamera.SetPreviewCallback(this);

            // Write the image in a file (in jpeg format)
            try
            {
                FileOutputStream fos = new FileOutputStream(mPictureFileName);

                fos.Write(data);
                fos.Close();

            }
            catch (Exception ex)
            {
                Log.Error("PictureDemo", "Exception in photoCallback", ex);
            }

        }
    }
}