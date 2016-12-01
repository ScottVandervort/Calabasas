using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using System;
using System.Collections.Generic;

namespace Calabasas
{
    public class Camera
    {
        private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();
        private readonly Dictionary<int, PumpkinFaceTracker> trackedSkeletons = new Dictionary<int, PumpkinFaceTracker>();
        private DepthImageFormat depthImageFormat = DepthImageFormat.Undefined;
        private short[] depthImage;
        private Skeleton[] skeletonData;
        private ColorImageFormat colorImageFormat = ColorImageFormat.Undefined;
        private byte[] colorImage;
        private const uint MaxMissedFrames = 100;
        private PumpkinFaceRenderer pumpkinFaceRenderer;

        public Camera (PumpkinFaceRenderer pumpkinFaceRenderer)
        {
            this.pumpkinFaceRenderer = pumpkinFaceRenderer;
        }

        public void Run ()
        {
            sensorChooser.KinectChanged += SensorChooser_KinectChanged;

            sensorChooser.Start();
        }

        private void SensorChooser_KinectChanged(object sender, KinectChangedEventArgs e)
        {
            KinectSensor oldSensor = e.OldSensor;
            KinectSensor newSensor = e.NewSensor;

            if (oldSensor != null)
            {
                oldSensor.AllFramesReady -= KinectSensorOnAllFramesReady;
                oldSensor.ColorStream.Disable();
                oldSensor.DepthStream.Disable();
                oldSensor.DepthStream.Range = DepthRange.Default;
                oldSensor.SkeletonStream.Disable();
                oldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                oldSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;

                ResetFaceTracking();
            }

            if (newSensor != null)
            {
                try
                {
                    newSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    newSensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                    try
                    {
                        // This will throw on non Kinect For Windows devices.
                        newSensor.DepthStream.Range = DepthRange.Near;
                        newSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        newSensor.DepthStream.Range = DepthRange.Default;
                        newSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }

                    newSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    newSensor.SkeletonStream.Enable();
                    newSensor.AllFramesReady += KinectSensorOnAllFramesReady;
                }
                catch (InvalidOperationException)
                {
                    // This exception can be thrown when we are trying to
                    // enable streams on a device that has gone away.  This
                    // can occur, say, in app shutdown scenarios when the sensor
                    // goes away between the time it changed status and the
                    // time we get the sensor changed notification.
                    //
                    // Behavior here is to just eat the exception and assume
                    // another notification will come along if a sensor
                    // comes back.
                }
            }
        }

        private void KinectSensorOnAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            ColorImageFrame colorImageFrame = null;
            DepthImageFrame depthImageFrame = null;
            SkeletonFrame skeletonFrame = null;

            try
            {
                colorImageFrame = e.OpenColorImageFrame();
                depthImageFrame = e.OpenDepthImageFrame();
                skeletonFrame = e.OpenSkeletonFrame();

                if (colorImageFrame == null || depthImageFrame == null || skeletonFrame == null)
                {
                    return;
                }

                // Check for image format changes.  The FaceTracker doesn't
                // deal with that so we need to reset.
                if (depthImageFormat != depthImageFrame.Format)
                {
                    ResetFaceTracking();
                    depthImage = null;
                    depthImageFormat = depthImageFrame.Format;
                }

                if (colorImageFormat != colorImageFrame.Format)
                {
                    ResetFaceTracking();
                    colorImage = null;
                    colorImageFormat = colorImageFrame.Format;
                }

                // Create any buffers to store copies of the data we work with
                if (depthImage == null)
                {
                    depthImage = new short[depthImageFrame.PixelDataLength];
                }

                if (colorImage == null)
                {
                    colorImage = new byte[colorImageFrame.PixelDataLength];
                }

                // Get the skeleton information
                if (skeletonData == null || skeletonData.Length != skeletonFrame.SkeletonArrayLength)
                {
                    skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                }

                colorImageFrame.CopyPixelDataTo(colorImage);
                depthImageFrame.CopyPixelDataTo(depthImage);
                skeletonFrame.CopySkeletonDataTo(skeletonData);

                // Update the list of trackers and the trackers with the current frame information
                foreach (Skeleton skeleton in skeletonData)
                {
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked
                        || skeleton.TrackingState == SkeletonTrackingState.PositionOnly)
                    {
                        // We want keep a record of any skeleton, tracked or untracked.
                        if (!trackedSkeletons.ContainsKey(skeleton.TrackingId))
                        {
                            trackedSkeletons.Add(skeleton.TrackingId, new PumpkinFaceTracker(pumpkinFaceRenderer));
                        }

                        // Give each tracker the upated frame.
                        PumpkinFaceTracker skeletonFaceTracker;
                        if (trackedSkeletons.TryGetValue(skeleton.TrackingId, out skeletonFaceTracker))
                        {
                            skeletonFaceTracker.OnFrameReady(sensorChooser.Kinect, colorImageFormat, colorImage, depthImageFormat, depthImage, skeleton);
                            skeletonFaceTracker.LastTrackedFrame = skeletonFrame.FrameNumber;
                        }
                    }
                }

                RemoveOldTrackers(skeletonFrame.FrameNumber);

                //InvalidateVisual();
            }
            finally
            {
                if (colorImageFrame != null)
                {
                    colorImageFrame.Dispose();
                }

                if (depthImageFrame != null)
                {
                    depthImageFrame.Dispose();
                }

                if (skeletonFrame != null)
                {
                    skeletonFrame.Dispose();
                }
            }
        }

        private void RemoveOldTrackers(int currentFrameNumber)
        {
            var trackersToRemove = new List<int>();

            foreach (var tracker in trackedSkeletons)
            {
                uint missedFrames = (uint)currentFrameNumber - (uint)tracker.Value.LastTrackedFrame;
                if (missedFrames > MaxMissedFrames)
                {
                    // There have been too many frames since we last saw this skeleton
                    trackersToRemove.Add(tracker.Key);
                }
            }

            foreach (int trackingId in trackersToRemove)
            {
                RemoveTracker(trackingId);
            }
        }

        private void RemoveTracker(int trackingId)
        {
            trackedSkeletons[trackingId].Dispose();
            trackedSkeletons.Remove(trackingId);
        }

        private void ResetFaceTracking()
        {
            foreach (int trackingId in new List<int>(trackedSkeletons.Keys))
            {
                RemoveTracker(trackingId);
            }
        }
    }
}
