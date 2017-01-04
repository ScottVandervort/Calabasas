using System;

namespace Calabasas
{
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
