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
        
        internal int? indexTopOfHeadPoint;
        internal System.Drawing.RectangleF boundingBox;

        public int? IndexTopOfHeadPoint
        {
            get
            {                
                if (this.Points != null && this.Points.Length > 0 && (!this.indexTopOfHeadPoint.HasValue))
                {
                    int resultIndexTopOfHeadPoint = 0;

                    DetermineFaceDimensions(this.Points, out this.boundingBox, out resultIndexTopOfHeadPoint);

                    this.indexTopOfHeadPoint = resultIndexTopOfHeadPoint;
                }

                return this.indexTopOfHeadPoint;
            }
        }

        public System.Drawing.RectangleF BoundingBox
        {
            get
            {
                if (this.Points != null && this.Points.Length > 0 && (this.boundingBox == System.Drawing.RectangleF.Empty))
                {
                    int resultIndexTopOfHeadPoint = 0;

                    DetermineFaceDimensions(this.Points, out this.boundingBox, out resultIndexTopOfHeadPoint);

                    this.indexTopOfHeadPoint = resultIndexTopOfHeadPoint;
                }

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

        static public void DetermineFaceDimensions ( System.Drawing.PointF [] points, out System.Drawing.RectangleF boundingBox, out int indexTopOfHeadPoint )
        {
            indexTopOfHeadPoint = -1;
            boundingBox = System.Drawing.RectangleF.Empty;

            if (points != null && points.Length > 0)
            {
                boundingBox = DetermineBoundingBox(points);
            
                if (boundingBox != System.Drawing.Rectangle.Empty)
                {
                    System.Drawing.PointF desiredPoint = new System.Drawing.PointF((boundingBox.Right - boundingBox.Left) / 2.0f, boundingBox.Top);
                    double minDist = Math.Round(Math.Sqrt(Math.Pow((desiredPoint.X - points[0].X), 2) + Math.Pow((desiredPoint.Y - points[0].Y), 2)), 2);
                    indexTopOfHeadPoint = 0;

                    for (int pointIndex = 0; pointIndex < points.Length; pointIndex++)
                    {
                        double dist = Math.Round(Math.Sqrt(Math.Pow((desiredPoint.X - points[pointIndex].X), 2) + Math.Pow((desiredPoint.Y - points[pointIndex].Y), 2)), 2);

                        if (dist < minDist)
                        {
                            indexTopOfHeadPoint = pointIndex;
                            minDist = dist;                           
                        }
                    }
                }
            }
        }
    }
}
