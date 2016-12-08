namespace Calabasas
{ 
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0].Trim().ToUpperInvariant())
                {
                    case "FILE":
                        if (args.Length > 1)
                        {
                            string filePath = args[1];
                            System.Drawing.PointF[] points = LoadFromFile(filePath);

                            DisplayPoints(points);
                        }
                        else
                            RunKinnect();
                        break;
                    default: // KINNECT
                        RunKinnect();
                        break;
                }
            }
            else
            {
                RunKinnect();
            }
        }

        public static System.Drawing.PointF[] LoadFromFile(string filePath)
        {
            // TODO:
            return null;
        }

        static void RunKinnect()
        {
            PumpkinFaceRenderer pumpkinFaceRenderer = new PumpkinFaceRenderer();

            Camera camera = new Camera(pumpkinFaceRenderer);

            camera.Run();

            pumpkinFaceRenderer.Draw(new System.Drawing.PointF[] { new System.Drawing.PointF(100, 100), new System.Drawing.PointF(-50, -50), new System.Drawing.PointF(90, 90) });

            pumpkinFaceRenderer.Run();
        }

        static void DisplayPoints( System.Drawing.PointF[] points )
        {
            PumpkinFaceRenderer pumpkinFaceRenderer = new PumpkinFaceRenderer();

            pumpkinFaceRenderer.Draw(points);

            pumpkinFaceRenderer.Run();
        }
    }
}
