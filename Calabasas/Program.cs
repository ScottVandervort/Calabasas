using System;

namespace Calabasas
{
    class Program
    {        
        static void Main(string[] args)
        {
            IFaceCamera<System.Drawing.PointF> camera = null;

            if (args.Length > 0)
            {
                string filePath = args[0];

                if (System.IO.File.Exists(filePath))
                    camera = new DummyCamera(filePath);
                else
                {
                    Console.WriteLine(String.Format("File does not exist. File: {0}", filePath));
                    Console.WriteLine("Press any key to exit");
                    Console.ReadKey();
                }
            }
            else            
                camera = new KinectCamera();

            if (camera != null)
            {
                PumpkinFaceRenderer pumpkinFaceRenderer = new PumpkinFaceRenderer(camera);
                pumpkinFaceRenderer.Start();
            }
        }
    }
}
