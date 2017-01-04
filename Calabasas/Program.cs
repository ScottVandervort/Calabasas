using SharpDX.Mathematics.Interop;

namespace Calabasas
{ 
    class Program
    {
        private const int IndexTopOfHeadPoint = 29;

        static void Main(string[] args)
        {
            //IFaceCamera<System.Drawing.PointF> camera = new FaceCamera();

            //PumpkinFaceRenderer pumpkinFaceRenderer = new PumpkinFaceRenderer(camera);
            //pumpkinFaceRenderer.Draw(new System.Drawing.PointF[] { new System.Drawing.PointF(0, 0), new System.Drawing.PointF(100, 0), new System.Drawing.PointF(100, 100), new System.Drawing.PointF(0, 100) });

            PumpkinFaceRenderer pumpkinFaceRenderer = new PumpkinFaceRenderer(null);

            System.Drawing.PointF[] points = new System.Drawing.PointF[1000];
            for (int index = 0; index < 1000; index++)
            {
                // Renderer uses this point for centering.
                if (index == IndexTopOfHeadPoint)                
                    points[index] = new System.Drawing.PointF(500, 0);                
                else
                    points[index] = new System.Drawing.PointF(index, index);
            }
            FaceState faceState = new FaceState()
            {
                Points = points
            };

            pumpkinFaceRenderer.Draw(faceState);

            pumpkinFaceRenderer.Start();
        }
    }
}
