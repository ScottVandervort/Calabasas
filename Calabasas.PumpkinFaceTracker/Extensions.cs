using Microsoft.Kinect.Toolkit.FaceTracking;

namespace Calabasas
{
    public static class Extensions
    {
        public static System.Drawing.PointF[] ConvertToPointF(this EnumIndexableCollection<FeaturePoint, PointF> collection)
        {
            System.Drawing.PointF[] result = new System.Drawing.PointF[collection.Count];

            for (int index = 0; index < collection.Count; index++)
            {
                result[index] = new System.Drawing.PointF(collection[index].X, collection[index].Y);
            }

            return result;
        }
    }
}
