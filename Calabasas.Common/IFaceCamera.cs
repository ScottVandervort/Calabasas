using System;


namespace Calabasas
{
    public interface IFaceCamera<VectorType>
    {
        int IndexTopOfHeadPoint { get; }

        void Start();
        void Stop();
        event EventHandler<FaceState> OnFaceChanged;
        event EventHandler<bool> OnTrackingFace;
    }
}
