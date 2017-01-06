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
        private const float PointSize = 0.2f;
        private const float ClickSize = 2f;
        private const int ClickTimeoutSeconds = 3;
        private const float ZoomDelta = 0.2f;
        private const string StateFilePath = "state.dat";

        private float zoom = 3.0f;
        private bool showDebugInfo = true;
        /// <summary>
        /// True, if the data being displayed is from the Kinect.
        /// </summary>
        private bool isKinectFeed = true;

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
        private int expectedFacePoints = ExpectedFacePoints;
        private int indexTopOfHeadPoint = IndexTopOfHeadPoint;

        private bool isLeftEyeClosed;
        private bool isRightEyeClosed;
        private bool isHappy;
        private bool isMouthOpen;
        private bool isMouthMoved;
        private bool isWearingGlasses;
        
        private int? selectedFacePointIndex;
        private TimeSpan selectedFacePointTimeout;
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

        private Geometry facePointGeometry;
        private SolidColorBrush facePointBrush;
        private Color facePointPenColor = Color.Orange;

        private FaceState CurrentFaceState
        {
            get
            {
                return new FaceState()
                {
                    Points = this.facePoints.ConvertToVector2(),
                    IsHappy = this.isHappy,
                    IsLeftEyeClosed = this.isLeftEyeClosed,
                    IsRightEyeClosed = this.isRightEyeClosed,
                    IsMouthMoved = this.isMouthMoved,
                    IsMouthOpen = this.isMouthOpen,
                    IsWearingGlasses = this.isWearingGlasses
                };
            }
        }

        public PumpkinFaceRenderer(IFaceCamera<System.Drawing.PointF> faceCamera, int expectedFacePoints = ExpectedFacePoints, int indexTopOfHeadPoint = IndexTopOfHeadPoint)
        {
            renderForm = new RenderForm("Calabasas");
            renderForm.AllowUserResizing = true;

            this.faceCamera = faceCamera;
            this.expectedFacePoints = expectedFacePoints;
            this.indexTopOfHeadPoint = indexTopOfHeadPoint;

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

            // Initialize geometery/drawable primitives.
            facePointGeometry = new RectangleGeometry(d2dFactory, new SharpDX.Mathematics.Interop.RawRectangleF(-PointSize/2.0F,-PointSize/2.0F,PointSize/2.0F,PointSize/2.0F));
            facePointBrush = new SolidColorBrush(d2dRenderTarget, new SharpDX.Color(facePointPenColor.R, facePointPenColor.G, facePointPenColor.B));

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

        public void Draw(FaceState faceState, bool isKinectFeed = false)
        {
            this.isKinectFeed = isKinectFeed;

            this.isLeftEyeClosed = faceState.IsLeftEyeClosed;
            this.isRightEyeClosed = faceState.IsRightEyeClosed;
            this.isHappy = faceState.IsHappy;
            this.isMouthOpen = faceState.IsMouthOpen;
            this.isMouthMoved = faceState.IsMouthMoved;
            this.isWearingGlasses = faceState.IsWearingGlasses;

            this.facePoints = faceState.Points.ConvertToVector2();

            this.faceBoundingBox = faceState.BoundingBox.ConvertToRectangleF();

            this.faceCenter = new Vector2(
                        this.facePoints[this.indexTopOfHeadPoint].X,
                        this.facePoints[this.indexTopOfHeadPoint].Y + (this.faceBoundingBox.Height / 2.0f));

            this.transformation = CalculateTransformation(this.faceCenter, this.zoom, Width, Height);
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
            facePointGeometry.Dispose();
            facePointBrush.Dispose();
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

            renderPoints(facePoints);

            d2dRenderTarget.RestoreDrawingState(drawingStateBlock);

            if (this.showDebugInfo)
                renderDebugInfo();

            d2dRenderTarget.EndDraw();

            swapChain.Present(0, PresentFlags.None);

            framesPerSecond.Frame();
        }

        private void renderDebugInfo ()
        {
            TimeSpan runTime = framesPerSecond.RunTime;

            // Column 1
            renderText(new Vector2(0, 0), String.Format("FPS: {0}", framesPerSecond.GetFPS().ToString()));
            renderText(new Vector2(0, 20), String.Format("Runtime: {0}", runTime.ToString(@"hh\:mm\:ss\:ff")));
            renderText(new Vector2(0, 40), String.Format("Total Face Points: {0}", ((this.facePoints != null) ? this.facePoints.Length : 0)));
            renderText(new Vector2(0, 60), String.Format("Zoom: {0}x", this.zoom));

            // Column 2
            if (this.isLeftEyeClosed)
                renderText(new Vector2(200, 0), "Left Eye Closed");
            if (this.isRightEyeClosed)
                renderText(new Vector2(200, 20), "Right Eye Closed");
            if (this.isHappy)
                renderText(new Vector2(200, 40), "Is Happy");
            if (this.isMouthOpen)
                renderText(new Vector2(200, 60), "Mouth Open");
            if (this.isMouthMoved)
                renderText(new Vector2(200, 80), "Mouth Moved");
            if (this.isWearingGlasses)
                renderText(new Vector2(200, 100), "Is Wearing Glasses");

            // Column 3
            if (this.facePoints != null && this.facePoints.Length > 0 && this.selectedFacePointIndex.HasValue && this.selectedFacePointTimeout > runTime)
                renderText(new Vector2(400, 0), String.Format("Clicked Point Index: {0} ({1},{2})", this.selectedFacePointIndex, this.facePoints[this.selectedFacePointIndex.Value].X, this.facePoints[this.selectedFacePointIndex.Value].Y));
        }

        private void renderPoints (SharpDX.Vector2 [] points)
        {
            using (DrawingStateBlock block = new DrawingStateBlock(d2dFactory))
            {
                d2dRenderTarget.SaveDrawingState(block);

                foreach (Vector2 point in points)
                {
                    d2dRenderTarget.Transform = Matrix3x2.Translation(point) * this.transformation;
                    d2dRenderTarget.DrawGeometry(this.facePointGeometry, facePointBrush);
                }

                d2dRenderTarget.RestoreDrawingState(block);
            }
        }

        private void renderText(SharpDX.Vector2 point, string text)
        {
            using (TextLayout textLayout = new TextLayout(dwFactory, text, TextFormat, 400, 50))
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

        static private SharpDX.Matrix3x2 CalculateTransformation (Vector2 faceCenter, float zoom, int deviceWidth, int deviceHeight)
        {
            Matrix3x2 result = Matrix3x2.Identity;

            // 1) Translate the target so that the points will be centered.
            // 2) Scale the points to fit the target.
            // 3) Translate the target to the center of the device target.
            result =
                Matrix3x2.Translation(-faceCenter.X, -faceCenter.Y) *
                Matrix3x2.Scaling(zoom, zoom) *
                Matrix3x2.Translation(deviceWidth / 2.0f, deviceHeight / 2.0f);

            return result;
        }

        private void OnTrackingFace(object sender, bool isTracking)
        {
            // TODO: Handle messages from camera.
        }

        private void OnFaceChanged(object sender, FaceState faceState)
        {
            if (this.isKinectFeed)
                this.Draw(faceState, true);
        }

        private void OnRenderFormKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            switch (Char.ToUpperInvariant(e.KeyChar))
            {
                case '+':
                    this.zoom += ZoomDelta;
                    this.transformation = CalculateTransformation(this.faceCenter, this.zoom, Width, Height);
                    e.Handled = true;
                    break;
                case '-':
                    this.zoom -= ZoomDelta;
                    this.zoom = Math.Max(ZoomDelta, this.zoom);
                    this.transformation = CalculateTransformation(this.faceCenter, this.zoom, Width, Height);
                    e.Handled = true;
                    break;
                case 'D':
                    this.showDebugInfo = !this.showDebugInfo;
                    e.Handled = true;
                    break;
                case 'S':
                    if (FaceState.SaveToFile(this.CurrentFaceState, StateFilePath))
                    {
                        // Display state in debug info.
                    }
                    e.Handled = true;
                    break;
                case 'L':
                    FaceState savedFaceState;

                    if (this.isKinectFeed)
                    {
                        if (FaceState.LoadFromFile(StateFilePath, out savedFaceState))
                        {
                            this.isKinectFeed = false;
                            this.Draw(savedFaceState);
                        }
                    }
                    else
                    {
                        this.isKinectFeed = true;
                    }

                    e.Handled = true;
                    break;
            }           
        }

        private void OnRenderFormMouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Mouse clicks are in "screen space". DirectX might be rendering the client/form in a different resolution (i.e., "world space") 
            // so we need to transform the click coordinates from "screen space" to "world space".
            // It gets trickier. We want to know what facial point is clicked on. The facial point coordinates exist in "local/object space". They 
            // were transformed to "world space" prior to being rendered. So, we need to transform the mouse click coordinates from "world space" to 
            // the facial point "local/object space".            

            Matrix3x2 inverseOfRenderTargetTransform = new Matrix3x2();
            Matrix3x2.Invert(ref this.transformation, out inverseOfRenderTargetTransform);

            Size2F renderTargetSize = this.d2dRenderTarget.Size;
            System.Drawing.Rectangle clientSize = this.renderForm.ClientRectangle;

            float scalingFromClientToRenderTargetX = renderTargetSize.Width / clientSize.Right;
            float scalingFromClientToRenderTargetY = renderTargetSize.Height / clientSize.Bottom;

            Vector2 renderTargetClickCoord = new Vector2(e.X * scalingFromClientToRenderTargetX, e.Y * scalingFromClientToRenderTargetY);
            Vector2 transformedRenderTargetClickCoord = Matrix3x2.TransformPoint(inverseOfRenderTargetTransform, renderTargetClickCoord);

            RectangleF transformedRenderTargetClickArea = new RectangleF(transformedRenderTargetClickCoord.X, transformedRenderTargetClickCoord.Y, ClickSize, ClickSize);

            this.selectedFacePointIndex = null;

            for (int pointIndex = 0; pointIndex < this.facePoints.Length; pointIndex++)
                if (transformedRenderTargetClickArea.Contains(this.facePoints[pointIndex]))
                {                    
                    this.selectedFacePointIndex = pointIndex;
                    break;
                }

            this.selectedFacePointTimeout = this.framesPerSecond.RunTime.Add(new TimeSpan(0, 0, ClickTimeoutSeconds));
        }
    }
}
 