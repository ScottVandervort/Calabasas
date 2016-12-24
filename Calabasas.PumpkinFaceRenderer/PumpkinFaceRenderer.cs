using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calabasas
{
    abstract public class PumpkinFaceRenderer<VectorType> : IPumpkinFaceRenderer<VectorType>
    {
        protected const int Width = 1280;
        protected const int Height = 720;

        protected IFaceCamera<VectorType> faceCamera;

        protected FramesPerSecond framesPerSecond;

        protected RenderForm renderForm;

        public PumpkinFaceRenderer(IFaceCamera<VectorType> faceCamera)
        {
            this.renderForm = new RenderForm("Calabasas");

            this.renderForm.ClientSize = new Size(Width, Height);
            this.renderForm.AllowUserResizing = false;

            this.faceCamera = faceCamera;

            this.framesPerSecond = new FramesPerSecond();

            this.renderForm.KeyPress += OnRenderFormKeyPress;

            if (this.faceCamera != null)
                this.faceCamera.OnFaceChanged += OnFaceChanged;
        }

        virtual public void Start()
        {
            if (this.faceCamera != null)
                this.faceCamera.Start();

            // Start the render loop
            RenderLoop.Run(renderForm, OnRenderCallback);
        }

        virtual protected void OnRenderCallback()
        {
            framesPerSecond.Frame();
        }

        virtual protected void OnFaceChanged(object sender, VectorType[] points)
        {
            this.Draw(points);
        }

        virtual protected void OnRenderFormKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            // TODO: Handle keystrokes.
        }

        virtual public void Dispose()
        {
            if (this.faceCamera != null)
            {
                this.faceCamera.Stop();
                this.faceCamera = null;
            }
        }

        public abstract void Draw(VectorType[] points);
    }
}
