using System;


namespace Calabasas
{
    public interface IFaceCamera<VectorType>
    {
        void Start();
        void Stop();
        event EventHandler<VectorType[]> OnFaceChanged; 
        event EventHandler OnTrackingFace;
    }
}
