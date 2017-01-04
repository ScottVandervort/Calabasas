using System;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using SharpDX.DirectWrite;
using Calabasas.Common;

namespace Calabasas
{
    public class PumpkinFaceRenderer : IDisposable
    {
        private const int Width = 1280;
        private const int Height = 720;
        private const int ExpectedFacePoints = 121;
        private const int IndexTopOfHeadPoint = 29;

        IFaceCamera<System.Drawing.PointF> faceCamera;

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

        private Vector2[] facePoints = { };
        private Vector2 faceCenter = new Vector2(0, 0);
        private RectangleF faceBoundingBox = new RectangleF();

        private bool isLeftEyeClosed;
        private bool isRightEyeClosed;
        private bool isHappy;
        private bool isMouthOpen;
        private bool isMouthMoved;
        private bool isWearingGlasses;

        private Vector2 selectedFacePoint;
        private int selectedFacePointIndex;
        FramesPerSecond framesPerSecond;

        private Matrix3x2 transformation = Matrix3x2.Identity;

        //private Vector2[] leftEyebrow = { };
        //private Vector2[] rightEyebrow = { };
        //private Vector2[] mouth = { };
        //private Vector2[] nose = { };
        //private Vector2[] leftEye = { };
        //private Vector2[] leftPupil = { };
        //private Vector2[] rightEye = { };
        //private Vector2[] rightPupil = { };

        public TextFormat TextFormat { get; private set; }
        public SolidColorBrush SceneColorBrush { get; private set; }

        public PumpkinFaceRenderer(IFaceCamera<System.Drawing.PointF> faceCamera)
        {
            renderForm = new RenderForm("Calabasas");
            renderForm.AllowUserResizing = true;

            this.faceCamera = faceCamera;

            renderForm.KeyPress += OnRenderFormKeyPress;

            renderForm.MouseClick += OnRenderFormMouseClick;

            if (this.faceCamera != null)
            {
                this.faceCamera.OnFaceChanged += OnFaceChanged;
                this.faceCamera.OnTrackingFace += OnTrackingFace;
            }

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
            TextFormat = new TextFormat(dwFactory, "Calibri", 18);

            d2dRenderTarget.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Cleartype;

            // Initialize a Brush.
            SceneColorBrush = new SolidColorBrush(d2dRenderTarget, Color.White);

            drawingStateBlock = new DrawingStateBlock(d2dFactory);

            framesPerSecond = new FramesPerSecond();
        }

        public void Start()
        {
            if (this.faceCamera != null)
                faceCamera.Start();

            // Start the render loop
            RenderLoop.Run(renderForm, OnRenderCallback);
        }

        public void Draw(FaceState faceState)
        {
            this.isLeftEyeClosed = faceState.IsLeftEyeClosed;
            this.isRightEyeClosed = faceState.IsRightEyeClosed;
            this.isHappy = faceState.IsHappy;
            this.isMouthOpen = faceState.IsMouthOpen;
            this.isMouthMoved = faceState.IsMouthMoved;
            this.isWearingGlasses = faceState.IsWearingGlasses;

            this.facePoints = faceState.Points.ConvertToVector2();

            this.faceBoundingBox = faceState.BoundingBox.ConvertToRectangleF();

            this.faceCenter = new Vector2(
                        this.facePoints[IndexTopOfHeadPoint].X,
                        this.facePoints[IndexTopOfHeadPoint].Y + (this.faceBoundingBox.Height / 2.0f));

            this.transformation =
                Matrix3x2.Translation(-faceCenter.X, -faceCenter.Y) *
                Matrix3x2.Scaling(3, 3) *
                Matrix3x2.Translation(Width / 2.0f, Height / 2.0f);            
        }

        //public void Draw(System.Drawing.PointF [] points)
        //{
        //    Vector2[] newPoints = new Vector2[points.Length];
        //    float totalX = 0,
        //            totalY = 0;

        //    // Convert PointF to Vector2 and determine center.
        //    for (int pointIndex = 0; pointIndex < points.Length; pointIndex++)
        //    {
        //        newPoints[pointIndex] = new Vector2(points[pointIndex].X, points[pointIndex].Y);
        //        totalX += points[pointIndex].X;
        //        totalY += points[pointIndex].Y;
        //    }

        //    this.points = newPoints;
        //    this.center = new Vector2(totalX / points.Length, totalY / points.Length);

        //    if (this.IsDrawingFace())
        //        this.GenerateFace();
        //}

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

        private bool IsDrawingFace()
        {
            return (this.facePoints != null && this.facePoints.Length == ExpectedFacePoints);
        }

        //private void GenerateFace()
        //{
        //    this.leftEyebrow = new Vector2[] {
        //        points[(int)FacePoints.LeftEyebrow0],
        //        points[(int)FacePoints.LeftEyebrow1],
        //        points[(int)FacePoints.LeftEyebrow2],
        //        points[(int)FacePoints.LeftEyebrow3]
        //    };

        //    this.rightEyebrow = new Vector2[] {
        //        points[(int)FacePoints.RightEyebrow0],
        //        points[(int)FacePoints.RightEyebrow1],
        //        points[(int)FacePoints.RightEyebrow2],
        //        points[(int)FacePoints.RightEyebrow3]
        //    };

        //    this.nose = new Vector2[] {
        //        points[(int)FacePoints.Nose0],
        //        points[(int)FacePoints.Nose1],
        //        points[(int)FacePoints.Nose2],
        //        points[(int)FacePoints.Nose3],
        //        points[(int)FacePoints.Nose4],
        //        points[(int)FacePoints.Nose5],
        //        points[(int)FacePoints.Nose6],
        //        points[(int)FacePoints.Nose7],
        //        points[(int)FacePoints.Nose8],
        //        points[(int)FacePoints.Nose9],
        //        points[(int)FacePoints.Nose10]
        //    };

        //    this.mouth = new Vector2[] {
        //        points[(int)FacePoints.Mouth0],
        //        points[(int)FacePoints.Mouth1],
        //        points[(int)FacePoints.Mouth2],
        //        points[(int)FacePoints.Mouth3],
        //        points[(int)FacePoints.Mouth4],
        //        points[(int)FacePoints.Mouth5],
        //        points[(int)FacePoints.Mouth6]
        //    };

        //    this.leftEye = new Vector2[] {
        //        points[(int)FacePoints.LeftEye0],
        //        points[(int)FacePoints.LeftEye1],
        //        points[(int)FacePoints.LeftEye2],
        //        points[(int)FacePoints.LeftEye3],
        //        points[(int)FacePoints.LeftEye4],
        //        points[(int)FacePoints.LeftEye5],
        //        points[(int)FacePoints.LeftEye6],
        //        points[(int)FacePoints.LeftEye7]

        //    };

        //    this.rightEye = new Vector2[] {
        //        points[(int)FacePoints.RightEye0],
        //        points[(int)FacePoints.RightEye1],
        //        points[(int)FacePoints.RightEye2]
        //    };

        //    this.rightPupil = new Vector2[] {
        //        points[(int)FacePoints.RightPupil0],
        //        points[(int)FacePoints.RightPupil1],
        //        points[(int)FacePoints.RightPupil2],
        //        points[(int)FacePoints.RightPupil3]
        //    };

        //    this.leftPupil = new Vector2[] {
        //        points[(int)FacePoints.LeftPupil0],
        //        points[(int)FacePoints.LeftPupil1],
        //        points[(int)FacePoints.LeftPupil2],
        //        points[(int)FacePoints.LeftPupil3]
        //    };
        //}


        private void OnRenderCallback()
        {
            d2dRenderTarget.BeginDraw();

            d2dRenderTarget.Clear(Color.Black);

            d2dRenderTarget.SaveDrawingState(drawingStateBlock);

            d2dRenderTarget.Transform = this.transformation;

            //if (this.IsDrawingFace())
            //{
            //    renderPolygon(this.leftEyebrow);
            //    renderPolygon(this.leftEye);
            //    renderPolygon(this.leftPupil);
            //    renderPolygon(this.rightEyebrow);
            //    renderPolygon(this.rightEye);
            //    renderPolygon(this.rightPupil);
            //    renderPolygon(this.mouth);
            //    renderPolygon(this.nose);
            //}
            //else
            //{
            //    //renderPolygon(this.points);
            //}

           for (int pointIndex = 0; pointIndex < facePoints.Length; pointIndex++)
                renderPoint(facePoints[pointIndex]);

            d2dRenderTarget.RestoreDrawingState(drawingStateBlock);

            renderText(new Vector2(0, 0), String.Format("FPS: {0}", framesPerSecond.GetFPS().ToString()));
            renderText(new Vector2(0, 20), String.Format("Runtime: {0}", framesPerSecond.RunTime.ToString(@"hh\:mm\:ss\:ff")));
            renderText(new Vector2(0, 40), String.Format("Total Face Points: {0}", ((this.facePoints != null) ? this.facePoints.Length : 0)));
            renderText(new Vector2(0, 60), String.Format("Is Left Eye Closed: {0}", this.isLeftEyeClosed));
            renderText(new Vector2(0, 80), String.Format("Is Right Eye Closed: {0}", this.isRightEyeClosed));
            renderText(new Vector2(0, 100), String.Format("Is Happy: {0}", this.isHappy));
            renderText(new Vector2(0, 120), String.Format("Is Mouth Open: {0}", this.isMouthOpen));
            renderText(new Vector2(0, 140), String.Format("Is Mouth Moved: {0}", this.isMouthMoved));
            renderText(new Vector2(0, 160), String.Format("Is Wearing Glasses: {0}", this.isWearingGlasses));
            renderText(new Vector2(0, 180), String.Format("Is Wearing Glasses: {0}", this.isWearingGlasses));

            d2dRenderTarget.EndDraw();

            swapChain.Present(0, PresentFlags.None);

            framesPerSecond.Frame();
        }

        private void renderPoint(SharpDX.Vector2 point)
        {
            using (EllipseGeometry ellipseGeometry = new EllipseGeometry(d2dFactory, new Ellipse(point.ConvertToRawVector2(), 0.2f, 0.2f)))
            {
                Color penColor = Color.DarkOrange;
                SolidColorBrush penBrush = new SolidColorBrush(d2dRenderTarget, new SharpDX.Color(penColor.R, penColor.G, penColor.B));

                d2dRenderTarget.DrawGeometry(ellipseGeometry, penBrush);
                d2dRenderTarget.FillGeometry(ellipseGeometry, penBrush, null);

            }
        }

        private void renderText(SharpDX.Vector2 point, string text)
        {
            using (TextLayout textLayout = new TextLayout(dwFactory, text, TextFormat, 200, 50))
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

                        d2dRenderTarget.DrawGeometry(pathGeometery, penBrush);
                    }
                }
            }
        }

        private void OnTrackingFace(object sender, bool isTracking)
        {
            // TODO: Handle messages from camera.
        }

        private void OnFaceChanged(object sender, FaceState faceState)
        {
            this.Draw(faceState);
        }

        private void OnRenderFormKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            // TODO: Handle keystrokes.
        }

        private void OnRenderFormMouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Matrix3x2 inverse = new Matrix3x2();
            Matrix3x2.Invert(ref this.transformation, out inverse);

            Size2F size = this.d2dRenderTarget.Size;
            System.Drawing.Rectangle clientRec = this.renderForm.ClientRectangle;

            float x = size.Width / clientRec.Right;
            float y = size.Height / clientRec.Bottom;

            Vector2 click = new Vector2(e.X * x, e.Y * y);
            Vector2 transformedClicked = Matrix3x2.TransformPoint(inverse, click);






            RectangleF transformedClickArea = new RectangleF(transformedClicked.X, transformedClicked.Y, 3, 3);

            for (int pointIndex = 0; pointIndex < this.facePoints.Length; pointIndex++)
            {
                if (transformedClickArea.Contains(this.facePoints[pointIndex]))
                {
                    Console.WriteLine("Hit!" + pointIndex);
                }
            }
        }
    }




}
 