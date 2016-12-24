using SharpDX;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;
using System.Collections.Generic;

namespace Calabasas
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

        static public VertexPositionColor[] Convert(System.Drawing.PointF[] points, RawColor4 color)
        {
            VertexPositionColor[] result = new VertexPositionColor[points.Length];
            for (int vertexIndex = 0; vertexIndex < points.Length; vertexIndex++)
                result[vertexIndex] = new VertexPositionColor(new SharpDX.Mathematics.Interop.RawVector3(points[vertexIndex].X, points[vertexIndex].Y, 0), color);

            return result;
        }
    }
}