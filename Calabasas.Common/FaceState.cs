namespace Calabasas
{
    public struct FaceState
    {
        public System.Drawing.PointF [] Points;
        public System.Drawing.Rectangle BoundingBox;
        public bool IsLeftEyeClosed;
        public bool IsRightEyeClosed;
        public bool IsHappy;
        public bool IsMouthOpen;
        public bool IsMouthMoved;
        public bool IsWearingGlasses;

        public System.Drawing.PointF Center 
        {
            get
            {
                System.Drawing.PointF result = System.Drawing.PointF.Empty;

                if (BoundingBox != null)
                    result = new System.Drawing.PointF((BoundingBox.Right - BoundingBox.Left) / 2.0f, (BoundingBox.Bottom - BoundingBox.Top) / 2.0f);

                return result;
            }

        }
    }
}
