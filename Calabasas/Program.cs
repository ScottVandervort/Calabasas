namespace Calabasas
{ 
    class Program
    {
        static void Main(string[] args)
        {
            PumpkinFaceRenderer pumpkinFaceRenderer = new PumpkinFaceRenderer();

            Camera camera = new Camera(pumpkinFaceRenderer);

            pumpkinFaceRenderer.Run();

            camera.Run();        
        }
    }
}
