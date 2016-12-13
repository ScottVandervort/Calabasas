namespace Calabasas
{ 
    class Program
    {
        static void Main(string[] args)
        {
            IFaceCamera camera = new FaceCamera();
            PumpkinFaceRenderer pumpkinFaceRenderer = new PumpkinFaceRenderer(camera);

            pumpkinFaceRenderer.Start();
        }
    }
}
