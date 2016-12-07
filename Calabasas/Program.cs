namespace Calabasas
{ 
    class Program
    {
        static void Main(string[] args)
        {
            PumpkinFaceRenderer pumpkinFaceRenderer = new PumpkinFaceRenderer();

            //Camera camera = new Camera(pumpkinFaceRenderer);

            //camera.Run();

            pumpkinFaceRenderer.Draw(new System.Drawing.PointF[] { new System.Drawing.PointF(100, 100), new System.Drawing.PointF(-50, -50), new System.Drawing.PointF(90, 90) });

            pumpkinFaceRenderer.Run();
     
        }
    }
}
