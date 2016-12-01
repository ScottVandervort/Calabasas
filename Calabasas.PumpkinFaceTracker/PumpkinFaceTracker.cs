using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;
using System;
using System.Diagnostics;

namespace Calabasas
{
    public class PumpkinFaceTracker : IDisposable
    {
        private Microsoft.Kinect.Toolkit.FaceTracking.FaceTracker faceTracker;

        private FaceTrackFrame faceTrackFrame;

        private bool lastFaceTrackSucceeded;

        private SkeletonTrackingState skeletonTrackingState;

        public int LastTrackedFrame { get; set; }

        private PumpkinFaceRenderer pumpkinFaceRenderer;

        private System.Drawing.PointF[] leftEyebrowPoints = new System.Drawing.PointF[4];
        const int LeftEyebrowFeaturePointStartingIndex = 48;
        const int LeftEyeBrowTotalFeaturePoints = 4;

        private System.Drawing.PointF[] leftEyePoints = new System.Drawing.PointF[6];
        const int LeftEyeFeaturePointStartingIndex = 52;
        const int LeftEyeTotalFeaturePoints = 6;

        private System.Drawing.PointF[] rightEyebrowPoints = new System.Drawing.PointF[4];
        const int RightEyebrowFeaturePointStartingIndex = 15;
        const int RightEyeBrowTotalFeaturePoints = 4;

        private System.Drawing.PointF[] rightEyePoints = new System.Drawing.PointF[6];
        const int RightEyeFeaturePointStartingIndex = 19;
        const int RightEyeTotalFeaturePoints = 6;

        private System.Drawing.PointF[] nosePoints = new System.Drawing.PointF[6];
        const int NoseFeaturePointStartingIndex = 19;
        const int NoseTotalFeaturePoints = 6;

        public PumpkinFaceTracker(PumpkinFaceRenderer pumpkinFaceRenderer)
        {
            this.pumpkinFaceRenderer = pumpkinFaceRenderer;
        }

        public void Dispose()
        {
            if (this.faceTracker != null)
            {
                this.faceTracker.Dispose();
                this.faceTracker = null;
            }
        }
        
        /// <summary>
        /// Updates the face tracking information for this skeleton
        /// </summary>
        internal void OnFrameReady(KinectSensor kinectSensor, ColorImageFormat colorImageFormat, byte[] colorImage, DepthImageFormat depthImageFormat, short[] depthImage, Skeleton skeletonOfInterest)
        {
            this.skeletonTrackingState = skeletonOfInterest.TrackingState;

            if (this.skeletonTrackingState != SkeletonTrackingState.Tracked)
            {
                // nothing to do with an untracked skeleton.
                return;
            }

            if (this.faceTracker == null)
            {
                try
                {
                    this.faceTracker = new Microsoft.Kinect.Toolkit.FaceTracking.FaceTracker(kinectSensor);
                }
                catch (InvalidOperationException)
                {
                    // During some shutdown scenarios the FaceTracker
                    // is unable to be instantiated.  Catch that exception
                    // and don't track a face.
                    Debug.WriteLine("AllFramesReady - creating a new FaceTracker threw an InvalidOperationException");
                    this.faceTracker = null;
                }
            }

            if (this.faceTracker != null)
            {
                FaceTrackFrame frame = this.faceTracker.Track(
                    colorImageFormat, colorImage, depthImageFormat, depthImage, skeletonOfInterest);

                this.lastFaceTrackSucceeded = frame.TrackSuccessful;
                if (this.lastFaceTrackSucceeded)
                {
                    EnumIndexableCollection<FeaturePoint, PointF> kinectFacePoints = frame.GetProjected3DShape();

                    for (int index = 0; index < LeftEyeBrowTotalFeaturePoints; index++)                   
                        leftEyebrowPoints[index] = new System.Drawing.PointF(kinectFacePoints[LeftEyebrowFeaturePointStartingIndex + index].X, kinectFacePoints[LeftEyebrowFeaturePointStartingIndex + index].Y);

                    for (int index = 0; index < RightEyeBrowTotalFeaturePoints; index++)
                        rightEyebrowPoints[index] = new System.Drawing.PointF(kinectFacePoints[RightEyebrowFeaturePointStartingIndex + index].X, kinectFacePoints[RightEyebrowFeaturePointStartingIndex + index].Y);

                    for (int index = 0; index < RightEyeTotalFeaturePoints; index++)
                        rightEyePoints[index] = new System.Drawing.PointF(kinectFacePoints[RightEyeFeaturePointStartingIndex + index].X, kinectFacePoints[RightEyeFeaturePointStartingIndex + index].Y);

                    for (int index = 0; index < LeftEyeTotalFeaturePoints; index++)
                        leftEyePoints[index] = new System.Drawing.PointF(kinectFacePoints[LeftEyeFeaturePointStartingIndex + index].X, kinectFacePoints[LeftEyeFeaturePointStartingIndex + index].Y);


                    pumpkinFaceRenderer.Draw();
                }
            }
        }
    }
}
