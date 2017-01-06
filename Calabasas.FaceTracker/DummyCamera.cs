using System;
using System.Drawing;

namespace Calabasas
{
    public class DummyCamera : IFaceCamera<PointF>
    {
        private const int IndexTopOfHeadPoint = 29;

        private FaceState _faceState;

        int IFaceCamera<PointF>.IndexTopOfHeadPoint
        {
            get
            {
                return IndexTopOfHeadPoint;
            }
        }

        public event EventHandler<FaceState> OnFaceChanged;
        public event EventHandler<bool> OnTrackingFace;

        private DummyCamera() { }

        public DummyCamera(string filePath) {
            if (!FaceState.LoadFromFile(filePath, out _faceState))
                throw new System.IO.FileNotFoundException("File not found!", filePath);
        }

        public void Start()
        {
            if (this.OnTrackingFace!=null)
            {
                this.OnTrackingFace(this, true);
            }

            if (this.OnFaceChanged!=null)
            {
                this.OnFaceChanged(this, _faceState);
            }

        }

        public void Stop()
        {
            if (this.OnTrackingFace != null)
            {
                this.OnTrackingFace(this, false);
            }
        }
    }
}
