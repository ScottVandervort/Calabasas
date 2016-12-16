namespace Calabasas
{ 
    class Program
    {
        static void Main(string[] args)
        {
            IFaceCamera camera = new FaceCamera();
            PumpkinFaceRenderer pumpkinFaceRenderer = new PumpkinFaceRenderer(camera);

            //pumpkinFaceRenderer.Draw(new System.Drawing.PointF[] { new System.Drawing.PointF(0, 0), new System.Drawing.PointF(100, 0), new System.Drawing.PointF(100, 100), new System.Drawing.PointF(0, 100) });

            pumpkinFaceRenderer.Start();
        }
    }
}
