namespace Calabasas
{
    public static class Extensions
    {
        public static SharpDX.Mathematics.Interop.RawVector2[] ConvertToRawVector2(this SharpDX.Vector2 [] array)
        {
            SharpDX.Mathematics.Interop.RawVector2[] result = new SharpDX.Mathematics.Interop.RawVector2[array.Length];

            for (int index = 0; index < array.Length; index++)
            {
                result[index] = new SharpDX.Mathematics.Interop.RawVector2(array[index].X, array[index].Y);
            }

            return result;
        }

        public static SharpDX.Vector2 ConvertToVector2(this System.Drawing.PointF point)
        {
            return new SharpDX.Vector2(point.X, point.Y);
        }

        public static SharpDX.Vector2[] ConvertToVector2(this System.Drawing.PointF [] points)
        {
            SharpDX.Vector2[] result = { };

            if (points != null && points.Length > 0)
            {
                result = new SharpDX.Vector2[points.Length];

                for (int index = 0; index < points.Length; index++)
                {
                    result[index] = new SharpDX.Vector2(points[index].X, points[index].Y);
                }
            }

            return result;
        }

        public static SharpDX.Mathematics.Interop.RawVector2 ConvertToRawVector2(this SharpDX.Vector2 vector)
        {
            return new SharpDX.Mathematics.Interop.RawVector2(vector.X,vector.Y);
        }
    }
}
