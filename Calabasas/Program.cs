using SharpDX.Mathematics.Interop;

namespace Calabasas
{ 
    class Program
    {
        static void Main(string[] args)
        {
            IFaceCamera camera = new FaceCamera();

            //IPumpkinFaceRenderer<System.Drawing.PointF> pumpkinFaceRenderer = new PumpkinFaceRenderer2D(camera);
            //pumpkinFaceRenderer.Draw(new System.Drawing.PointF[] { new System.Drawing.PointF(0, 0), new System.Drawing.PointF(100, 0), new System.Drawing.PointF(100, 100), new System.Drawing.PointF(0, 100) });

            IPumpkinFaceRenderer<RawVector3> pumpkinFaceRenderer = new PumpkinFaceRenderer3D(camera);

            pumpkinFaceRenderer.Start();
        }
    }
}
