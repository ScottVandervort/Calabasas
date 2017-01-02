using System;


namespace Calabasas
{
    public interface IFaceCamera<VectorType>
    {
        void Start();
        void Stop();
        event EventHandler<FaceState> OnFaceChanged;
        event EventHandler<bool> OnTrackingFace;
    }
}
