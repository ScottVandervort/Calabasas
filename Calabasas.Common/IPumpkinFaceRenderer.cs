using System;

namespace Calabasas
{
    public interface IPumpkinFaceRenderer<VectorType> : IDisposable
    {
        void Start();

        void Draw(VectorType[] points);
    }
}
