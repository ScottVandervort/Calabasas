using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calabasas
{
    public interface IFaceCamera
    {
        void Start();
        void Stop();
        event EventHandler<System.Drawing.PointF[]> OnFaceChanged; 
        event EventHandler OnTrackingFace;
    }
}
