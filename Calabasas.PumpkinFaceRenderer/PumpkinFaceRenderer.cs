using System;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using SharpDX.DirectWrite;

namespace Calabasas
{
    public class PumpkinFaceRenderer : IDisposable
    {
        private const int Width = 1280;
        private const int Height = 720;

        IFaceCamera faceCamera;

        FramesPerSecond framesPerSecond;

        private RenderForm renderForm;
        private SwapChainDescription swapChainDesc;
        private SharpDX.Direct3D11.Device device;
        private SwapChain swapChain;
        private SharpDX.Direct2D1.Factory d2dFactory;
        private SharpDX.DirectWrite.Factory dwFactory;
        private RenderTargetView renderTargetView;
        private RenderTarget d2dRenderTarget;
        private Texture2D backBuffer;
        private Surface surface;
        private DrawingStateBlock drawingStateBlock;

        private Vector2[] points = { };
        private Vector2 center = new Vector2(0, 0);

        public TextFormat TextFormat { get; private set; }
        public SolidColorBrush SceneColorBrush { get; private set; }

        public PumpkinFaceRenderer(IFaceCamera faceCamera)
        {
            renderForm = new RenderForm("Calabasas");

            this.faceCamera = faceCamera;

            renderForm.KeyPress += OnRenderFormKeyPress;

            this.faceCamera.OnFaceChanged += OnFaceChanged;

            // SwapChain description
            swapChainDesc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(Width, Height,
                                                       new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = renderForm.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            // Create Device and SwapChain
            SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, new SharpDX.Direct3D.FeatureLevel[] { SharpDX.Direct3D.FeatureLevel.Level_10_0 }, swapChainDesc, out device, out swapChain);

            d2dFactory = new SharpDX.Direct2D1.Factory();
            dwFactory = new SharpDX.DirectWrite.Factory();

            // Ignore all windows events
            SharpDX.DXGI.Factory factory = swapChain.GetParent<SharpDX.DXGI.Factory>();
            factory.MakeWindowAssociation(renderForm.Handle, WindowAssociationFlags.IgnoreAll);

            // New RenderTargetView from the backbuffer
            backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            renderTargetView = new RenderTargetView(device, backBuffer);

            surface = backBuffer.QueryInterface<Surface>();

            d2dRenderTarget = new RenderTarget(
                d2dFactory,
                surface,
                new RenderTargetProperties(new PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied)));

            // Initialize a TextFormat
            TextFormat = new TextFormat(dwFactory, "Calibri", 6);

            d2dRenderTarget.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Cleartype;

            // Initialize a Brush.
            SceneColorBrush = new SolidColorBrush(d2dRenderTarget, Color.White);

            drawingStateBlock = new DrawingStateBlock(d2dFactory);

            framesPerSecond = new FramesPerSecond();
        }

        public void Start()
        {
            faceCamera.Start();

            // Start the render loop
            RenderLoop.Run(renderForm, OnRenderCallback);
        }

        public void Draw(System.Drawing.PointF[] points)
        {
            Vector2[] newPoints = new Vector2[points.Length];
            float totalX = 0,
                    totalY = 0;

            // Convert PointF to Vector2 and determine center.
            for (int pointIndex = 0; pointIndex < points.Length; pointIndex++)
            {
                newPoints[pointIndex] = new Vector2(points[pointIndex].X, points[pointIndex].Y);
                totalX += points[pointIndex].X;
                totalY += points[pointIndex].Y;
            }

            this.points = newPoints;
            this.center = new Vector2(totalX / points.Length, totalY / points.Length);
        }

        public void Dispose()
        {
            this.faceCamera.Stop();

            // Release all resources
            drawingStateBlock.Dispose();
            renderTargetView.Dispose();
            d2dRenderTarget.Dispose();
            backBuffer.Dispose();
            device.ImmediateContext.ClearState();
            device.ImmediateContext.Flush();
            device.Dispose();
            device.Dispose();
            swapChain.Dispose();
            d2dFactory.Dispose();
            dwFactory.Dispose();
            SceneColorBrush.Dispose();
            TextFormat.Dispose();
            drawingStateBlock.Dispose();
        }

        private void OnRenderCallback()
        {
            d2dRenderTarget.BeginDraw();

            d2dRenderTarget.Clear(Color.Black);

            d2dRenderTarget.SaveDrawingState(drawingStateBlock);

            d2dRenderTarget.Transform = Matrix3x2.Translation(-center.X, -center.Y) * Matrix3x2.Scaling(3, 3) * Matrix3x2.Translation(Width / 2, Height / 2);

            renderPolygon(points);

            for (int pointIndex = 0; pointIndex < points.Length; pointIndex++)
            {
                renderPoint(points[pointIndex]);
                renderText(points[pointIndex], pointIndex.ToString());
            }

            d2dRenderTarget.RestoreDrawingState(drawingStateBlock);

            renderText(new Vector2(0, 0), framesPerSecond.GetFPS().ToString());
            renderText(new Vector2(0, 20), framesPerSecond.RunTime.ToString(@"hh\:mm\:ss\:ff"));

            d2dRenderTarget.EndDraw();

            swapChain.Present(0, PresentFlags.None);

            framesPerSecond.Frame();
        }

        private void renderPoint(SharpDX.Vector2 point)
        {
            using (EllipseGeometry ellipseGeometry = new EllipseGeometry(d2dFactory, new Ellipse(point.ConvertToRawVector2(), 1, 1)))
            {
                Color penColor = Color.DarkOrange;
                SolidColorBrush penBrush = new SolidColorBrush(d2dRenderTarget, new SharpDX.Color(penColor.R, penColor.G, penColor.B));

                d2dRenderTarget.DrawGeometry(ellipseGeometry, penBrush);
                d2dRenderTarget.FillGeometry(ellipseGeometry, penBrush, null);

            }
        }

        private void renderText(SharpDX.Vector2 point, string text)
        {
            using (TextLayout textLayout = new TextLayout(dwFactory, text, TextFormat, 100, 100))
            {
                d2dRenderTarget.DrawTextLayout(point, textLayout, SceneColorBrush, DrawTextOptions.None);
            }
        }

        private void renderPolygon(SharpDX.Vector2[] points)
        {
            if (points != null && points.Length > 0)
            {
                SharpDX.Mathematics.Interop.RawVector2[] vertices = points.ConvertToRawVector2();

                using (PathGeometry pathGeometery = new PathGeometry(d2dFactory))
                {
                    using (GeometrySink geometrySink = pathGeometery.Open())
                    {
                        geometrySink.BeginFigure(points[0], new FigureBegin());
                        geometrySink.AddLines(vertices);
                        geometrySink.AddLine(vertices[0]);
                        geometrySink.EndFigure(new FigureEnd());
                        geometrySink.Close();

                        Color penColor = Color.DarkOrange;
                        SolidColorBrush penBrush = new SolidColorBrush(d2dRenderTarget, new SharpDX.Color(penColor.R, penColor.G, penColor.B));


                        //RadialGradientBrush radialGradientBrush = new RadialGradientBrush(d2dRenderTarget, new RadialGradientBrushProperties()


                        d2dRenderTarget.DrawGeometry(pathGeometery, penBrush);
                    }
                }
            }
        }

        private void OnFaceChanged(object sender, System.Drawing.PointF[] points)
        {
            this.Draw(points);
        }

        private void OnRenderFormKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            // TODO: Handle keystrokes.
        }
    }

}