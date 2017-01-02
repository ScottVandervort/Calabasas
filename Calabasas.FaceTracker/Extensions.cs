using Microsoft.Kinect;

namespace Calabasas
{
    public static class Extensions
    {
        public static System.Drawing.PointF[] ConvertToPointF(this DepthSpacePoint [] points)
        {
            System.Drawing.PointF[] result = new System.Drawing.PointF[points.Length];

            for (int index = 0; index < points.Length; index++)
            {
                result[index] = new System.Drawing.PointF(points[index].X, points[index].Y);
            }

            return result;
        }
    }
}
