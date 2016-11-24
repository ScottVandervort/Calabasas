using SharpDX;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace PumpkinFaceAnimation
{
    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct VertexPositionColor
    {
        public readonly RawVector3 Position;
        public readonly RawColor4 Color;

        public VertexPositionColor(RawVector3 position, RawColor4 color)
        {
            Position = position;
            Color = color;
        }
    }
}