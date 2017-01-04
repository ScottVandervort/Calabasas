using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using System;
using System.Linq;

namespace Calabasas
{
    public class FaceCamera : IFaceCamera<System.Drawing.PointF>
    {
        private KinectSensor _sensor = null;
        private BodyFrameSource _bodySource = null;
        private BodyFrameReader _bodyReader = null;

        private HighDefinitionFaceFrameSource _faceSourceHighDef = null;
        private HighDefinitionFaceFrameReader _faceReaderHighDef = null;

        private FaceFrameSource _faceSource = null;
        private FaceFrameReader _faceReader = null;

        private FaceAlignment _faceAlignment = null;
        private FaceModel _faceModel = null;

        private FaceState _faceState = new FaceState();

        public event EventHandler<FaceState> OnFaceChanged;
        public event EventHandler<bool> OnTrackingFace;
        
        void IFaceCamera<System.Drawing.PointF>.Start()
        {
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {

                _sensor.IsAvailableChanged += OnKinectSensorChanged;

                _bodySource = _sensor.BodyFrameSource;
                _bodyReader = _bodySource.OpenReader();

                _bodyReader.FrameArrived += OnBodyReaderFrameArrived;

                _faceSourceHighDef = new HighDefinitionFaceFrameSource(_sensor);
                _faceReaderHighDef = _faceSourceHighDef.OpenReader();
                _faceReaderHighDef.FrameArrived += OnFaceReaderHighDefFrameArrived;

                _faceSource = new FaceFrameSource(_sensor, 0, FaceFrameFeatures.Glasses |
                                                              FaceFrameFeatures.Happy |
                                                              FaceFrameFeatures.LeftEyeClosed |
                                                              FaceFrameFeatures.MouthOpen |
                                                              FaceFrameFeatures.MouthMoved |
                                                              FaceFrameFeatures.RightEyeClosed);

                _faceSource.TrackingIdLost += _faceSource_TrackingIdLost;
                _faceSourceHighDef.TrackingIdLost += _faceSource_TrackingIdLost;
                _faceReader = _faceSource.OpenReader();
                _faceReader.FrameArrived += OnFaceReaderFrameArrived;


                _faceModel = new FaceModel();
                _faceAlignment = new FaceAlignment();

                _sensor.Open();
            }
        }

        private void _faceSource_TrackingIdLost(object sender, TrackingIdLostEventArgs e)
        {
            Console.WriteLine("Losty tracking " + e.TrackingId);
        }

        private void OnFaceReaderFrameArrived(object sender, FaceFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    FaceFrameResult result = frame.FaceFrameResult;

                    if (result != null)
                    {
                        _faceState.IsHappy = result.FaceProperties[FaceProperty.Happy] == DetectionResult.Yes;
                        _faceState.IsLeftEyeClosed = result.FaceProperties[FaceProperty.LeftEyeClosed] == DetectionResult.Yes;
                        _faceState.IsRightEyeClosed = result.FaceProperties[FaceProperty.RightEyeClosed] == DetectionResult.Yes;
                        _faceState.IsMouthMoved = result.FaceProperties[FaceProperty.MouthMoved] == DetectionResult.Yes;
                        _faceState.IsMouthOpen = result.FaceProperties[FaceProperty.MouthOpen] == DetectionResult.Yes;
                        _faceState.IsWearingGlasses = result.FaceProperties[FaceProperty.WearingGlasses] == DetectionResult.Yes;

                        if (this.OnFaceChanged != null)
                            this.OnFaceChanged(sender, _faceState);
                    }
                }              
            }
        }

        private void OnKinectSensorChanged(object sender, IsAvailableChangedEventArgs e)
        {
            if (this.OnTrackingFace != null)           
                this.OnTrackingFace(sender, e.IsAvailable);            
        }

        private void OnFaceReaderHighDefFrameArrived(object sender, HighDefinitionFaceFrameArrivedEventArgs e)
        {
            using (HighDefinitionFaceFrame frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null && frame.IsFaceTracked)
                {
                    frame.GetAndRefreshFaceAlignmentResult(_faceAlignment);                                      

                    if (_faceModel != null && _sensor != null)
                    {
                        CameraSpacePoint[] cameraSpacePoints = _faceModel.CalculateVerticesForAlignment(_faceAlignment).ToArray();
                        DepthSpacePoint[] depthSpacePoints = new DepthSpacePoint[cameraSpacePoints.Length];

                        if (cameraSpacePoints.Length > 0)
                            _sensor.CoordinateMapper.MapCameraPointsToDepthSpace(cameraSpacePoints, depthSpacePoints);

                        _faceState.Points = depthSpacePoints.ConvertToPointF();

                        if (this.OnFaceChanged != null)
                            this.OnFaceChanged(sender, _faceState);
                    }
                }
            }
        }

        private void OnBodyReaderFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {              

                if (frame != null)
                {
                    Body[] bodies = new Body[frame.BodyCount];
                    frame.GetAndRefreshBodyData(bodies);

                    Body body = bodies.Where(b => b.IsTracked).FirstOrDefault();                  

                    if (!_faceSourceHighDef.IsTrackingIdValid)
                    {
                        if (body != null)
                        {
                            _faceSourceHighDef.TrackingId = body.TrackingId;
                        }
                    }

                    if (!_faceSource.IsTrackingIdValid)
                    {
                        if (body != null)
                        {
                            _faceSource.TrackingId = body.TrackingId;
                        }
                    }
                }
            }
        }

        void IFaceCamera<System.Drawing.PointF>.Stop()
        {
            if(_bodyReader != null)
            {
                _bodyReader.Dispose();
                _bodyReader = null;
            }

            if(_faceReader != null)
            {
                _faceReader.Dispose();
                _faceReader = null;
            }

            if (_faceReaderHighDef != null)
            {
                _faceReaderHighDef.Dispose();
                _faceReaderHighDef = null;
            }            

            if(_faceSource != null)
            {
                _faceSource.Dispose();
                _faceSource = null;
            }

            if (_sensor != null)
            {
                _sensor.Close();
                _sensor = null;
            }

            if (_faceModel != null)
            {
                _faceModel.Dispose();
                _faceModel = null;
            }
        }
    }
}
