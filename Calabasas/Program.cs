using SharpDX.Mathematics.Interop;

namespace Calabasas
{ 
    class Program
    {
        static void Main(string[] args)
        {
            IFaceCamera<System.Drawing.PointF> camera = new FaceCamera();

            PumpkinFaceRenderer pumpkinFaceRenderer = new PumpkinFaceRenderer(camera);
            //pumpkinFaceRenderer.Draw(new System.Drawing.PointF[] { new System.Drawing.PointF(0, 0), new System.Drawing.PointF(100, 0), new System.Drawing.PointF(100, 100), new System.Drawing.PointF(0, 100) });

            //PumpkinFaceRenderer pumpkinFaceRenderer = new PumpkinFaceRenderer(null);

            //FaceState faceState = new FaceState()
            //{
            //    BoundingBox = new System.Drawing.Rectangle(0, 0, 100, 100),
            //    Points = new System.Drawing.PointF[] {
            //        new System.Drawing.PointF(0, 0),
            //        new System.Drawing.PointF(100, 0),
            //        new System.Drawing.PointF(100, 100),
            //        new System.Drawing.PointF(0, 100) }
            //};

            //pumpkinFaceRenderer.Draw(faceState);

            pumpkinFaceRenderer.Start();
        }
    }
}
