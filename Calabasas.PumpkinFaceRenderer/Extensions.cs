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

        public static SharpDX.Mathematics.Interop.RawVector2 ConvertToRawVector2(this SharpDX.Vector2 vector)
        {
            return new SharpDX.Mathematics.Interop.RawVector2(vector.X,vector.Y);
        }
    }
}
