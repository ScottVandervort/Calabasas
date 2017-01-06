using Newtonsoft.Json;
using System;
using System.IO;

namespace Calabasas
{
    [Serializable]
    public struct FaceState
    {
        public System.Drawing.PointF [] Points;
        public bool IsLeftEyeClosed;
        public bool IsRightEyeClosed;
        public bool IsHappy;
        public bool IsMouthOpen;
        public bool IsMouthMoved;
        public bool IsWearingGlasses;

        internal System.Drawing.RectangleF boundingBox;

        public System.Drawing.RectangleF BoundingBox
        {
            get
            {
                if (this.Points != null && this.Points.Length > 0 && (this.boundingBox == System.Drawing.RectangleF.Empty))                
                    this.boundingBox = DetermineBoundingBox(this.Points);                

                return this.boundingBox;
            }
        }

        static public bool SaveToFile ( FaceState faceState, string path )
        {
            bool result = false;

            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, faceState);
                result = true;
            }

            return result;
        }

        static public bool LoadFromFile ( string path, out FaceState faceState )
        {
            bool result = false;

            faceState = new FaceState();

            try
            {
                using (StreamReader file = File.OpenText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    faceState = (FaceState)serializer.Deserialize(file, typeof(FaceState));
                    result = true;
                }

                //using (FileStream file = File.OpenRead(path))
                //{
                //    var reader = new BinaryFormatter();
                //    faceState = (FaceState)reader.Deserialize(file);
                //    result = true;
                //}
            }
            catch (FileNotFoundException)
            {
                result = false;
            }

            return result;
        }

        static public System.Drawing.RectangleF DetermineBoundingBox ( System.Drawing.PointF [] points)
        {
            System.Drawing.RectangleF result = System.Drawing.RectangleF.Empty;

            if (points != null && points.Length > 0)
            {
                float top = points[0].Y,
                      bottom = points[0].Y,
                      left = points[0].X,
                      right = points[0].X;

                foreach (System.Drawing.PointF point in points)
                {
                    if (point.X < left)
                        left = point.X;
                    if (point.X > right)
                        right = point.X;
                    if (point.Y < top)
                        top = point.Y;
                    if (point.Y > bottom)
                        bottom = point.Y;
                }

                result = new System.Drawing.RectangleF(left, top, right - left, bottom - top);
            }

            return result;
        }
    }
}
