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

        private Vector2 [] points = { };
        private Vector2 center = new Vector2(0, 0);

        public TextFormat TextFormat { get; private set; }
        public SolidColorBrush SceneColorBrush { get; private set; }

        public PumpkinFaceRenderer ( IFaceCamera faceCamera )
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
        }

        public void Start()
        {
            faceCamera.Start();

            // Start the render loop
            RenderLoop.Run(renderForm, OnRenderCallback);
        }

        public void Draw(System.Drawing.PointF [] points)
        {
            Vector2 [] newPoints = new Vector2[points.Length];
            float   totalX = 0,
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
        }

        private void OnRenderCallback()
        {
            d2dRenderTarget.BeginDraw();
            d2dRenderTarget.Clear(Color.Black);

            for (int pointIndex = 0; pointIndex < points.Length; pointIndex++)
            {
                using (TextLayout textLayout = new TextLayout(dwFactory, pointIndex.ToString(), TextFormat, 100, 100))
                {
                    d2dRenderTarget.DrawTextLayout(points[pointIndex], textLayout, SceneColorBrush, DrawTextOptions.None);
                }
            }           

            d2dRenderTarget.Transform = Matrix3x2.Translation(-center.X, -center.Y) *  Matrix3x2.Scaling(3, 3) * Matrix3x2.Translation(Width/2,Height/2);

            d2dRenderTarget.EndDraw();

            swapChain.Present(0, PresentFlags.None);
        }

        private void OnFaceChanged(object sender, System.Drawing.PointF[] points)
        {
            this.Draw(points);
        }

        private void OnRenderFormKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            // TODO: Handle keystrokes.
        }

        /*

                private RenderForm renderForm;

                private const int Width = 1280;
                private const int Height = 720;

                private D3D11.Device d3dDevice;
                private D3D11.DeviceContext d3dDeviceContext;
                private SwapChain swapChain;
                private D3D11.RenderTargetView renderTargetView;
                private Viewport viewport;

                // Shaders
                private D3D11.VertexShader vertexShader;
                private D3D11.PixelShader pixelShader;
                private ShaderSignature inputSignature;
                private D3D11.InputLayout inputLayout;

                private D3D11.InputElement[] inputElements = new D3D11.InputElement[]
                {
                    new D3D11.InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, D3D11.InputClassification.PerVertexData, 0),
                    new D3D11.InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0, D3D11.InputClassification.PerVertexData, 0)
                };

                SharpDX.

                private VertexPositionColor[] rightEyebrowVertices;
                private VertexPositionColor[] rightEyeVertices;
                private VertexPositionColor[] leftEyebrowVertices;
                private VertexPositionColor[] leftEyeVertices;
                private VertexPositionColor[] noseVertices;
                private VertexPositionColor[] mouthVertices;

                private D3D11.Buffer rightEyebrowVertexBuffer;
                private D3D11.Buffer rightEyeVertexBuffer;
                private D3D11.Buffer leftEyebrowVertexBuffer;
                private D3D11.Buffer leftEyeVertexBuffer;
                private D3D11.Buffer noseVertexBuffer;
                private D3D11.Buffer mouthVertexBuffer;

                /// <summary>
                /// Create and initialize a new game.
                /// </summary>
                public PumpkinFaceRenderer()
                {
                    // Set window properties
                    renderForm = new RenderForm("My first SharpDX game");
                    renderForm.ClientSize = new Size(Width, Height);
                    renderForm.AllowUserResizing = false;

                    InitializeDeviceResources();
                    InitializeShaders();
                }

                /// <summary>
                /// Start the game.
                /// </summary>
                public void Run()
                {
                    // Start the render loop
                    RenderLoop.Run(renderForm, RenderCallback);
                }

                private void RenderCallback()
                {
                    Draw();
                }

                private void InitializeDeviceResources()
                {
                    ModeDescription backBufferDesc = new ModeDescription(Width, Height, new Rational(60, 1), Format.R8G8B8A8_UNorm);

                    // Descriptor for the swap chain
                    SwapChainDescription swapChainDesc = new SwapChainDescription()
                    {
                        ModeDescription = backBufferDesc,
                        SampleDescription = new SampleDescription(1, 0),
                        Usage = Usage.RenderTargetOutput,
                        BufferCount = 1,
                        OutputHandle = renderForm.Handle,
                        IsWindowed = true
                    };

                    // Create device and swap chain
                    D3D11.Device.CreateWithSwapChain(DriverType.Hardware, D3D11.DeviceCreationFlags.None, swapChainDesc, out d3dDevice, out swapChain);
                    d3dDeviceContext = d3dDevice.ImmediateContext;

                    viewport = new Viewport(0, 0, Width, Height);
                    d3dDeviceContext.Rasterizer.SetViewport(viewport);

                    // Create render target view for back buffer
                    using (D3D11.Texture2D backBuffer = swapChain.GetBackBuffer<D3D11.Texture2D>(0))
                    {
                        renderTargetView = new D3D11.RenderTargetView(d3dDevice, backBuffer);

                        var renderTarget = new SharpDX.Direct2D1.RenderTarget()
                    }
                }

                private void InitializeShaders()
                {
                    // Compile the vertex shader code
                    using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile("vertexShader.hlsl", "main", "vs_4_0", ShaderFlags.Debug))
                    {
                        // Read input signature from shader code
                        inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

                        vertexShader = new D3D11.VertexShader(d3dDevice, vertexShaderByteCode);
                    }

                    // Compile the pixel shader code
                    using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("pixelShader.hlsl", "main", "ps_4_0", ShaderFlags.Debug))
                    {
                        pixelShader = new D3D11.PixelShader(d3dDevice, pixelShaderByteCode);
                    }

                    // Set as current vertex and pixel shaders
                    d3dDeviceContext.VertexShader.Set(vertexShader);
                    d3dDeviceContext.PixelShader.Set(pixelShader);

                    d3dDeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

                    // Create the input layout from the input signature and the input elements
                    inputLayout = new D3D11.InputLayout(d3dDevice, inputSignature, inputElements);

                    // Set input layout to use
                    d3dDeviceContext.InputAssembler.InputLayout = inputLayout;
                }

                /// <summary>
                /// Draw the game.
                /// </summary>
                private void Draw()
                {
                    // Set render targets
                    d3dDeviceContext.OutputMerger.SetRenderTargets(renderTargetView);

                    // Clear the screen
                    d3dDeviceContext.ClearRenderTargetView(renderTargetView, new SharpDX.Color(32, 103, 178));


                    SharpDX.DirectWrite.Factory directWriteFactory = new SharpDX.DirectWrite.Factory();

                    TextFormat textFormat = new SharpDX.DirectWrite.TextFormat(directWriteFactory, "Calibri", 12) {
                        TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading,
                        ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Near };


                    SharpDX.Direct2D1.RenderTarget direct2DRenderTarget = new SharpDX.Direct2D1.RenderTarget(directWriteFactory, d2dSurface, new SharpDX.Direct2D1.RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied)));


                    SharpDX.Direct2D1.SolidColorBrush fontColor = new SharpDX.Direct2D1.SolidColorBrush(direct2DRenderTarget, SharpDX.Color.White);

                    Draw(rightEyebrowVertexBuffer, rightEyebrowVertices);
                    Draw(rightEyeVertexBuffer, rightEyeVertices);
                    Draw(leftEyebrowVertexBuffer, leftEyebrowVertices);
                    Draw(leftEyeVertexBuffer, leftEyeVertices);
                    Draw(mouthVertexBuffer, mouthVertices);
                    Draw(noseVertexBuffer, noseVertices);

                    // Swap front and back buffer
                    swapChain.Present(1, PresentFlags.None);
                }

                private void Draw (D3D11.Buffer buffer, VertexPositionColor [] vertices)
                {
                    // Set vertex buffer
                    d3dDeviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(buffer, Utilities.SizeOf<VertexPositionColor>(), 0));

                    // Draw the triangle
                    d3dDeviceContext.Draw(vertices.Count(), 0);
                }

                public void Draw (System.Drawing.PointF [] leftEyebrow, System.Drawing.PointF [] leftEye, System.Drawing.PointF [] rightEyebrow, System.Drawing.PointF [] rightEye, System.Drawing.PointF [] mouth, System.Drawing.PointF [] nose)
                {
                    rightEyebrowVertices = VertexPositionColor.Convert(rightEyebrow, SharpDX.Color.OrangeRed);
                    rightEyebrowVertexBuffer = D3D11.Buffer.Create(d3dDevice, D3D11.BindFlags.VertexBuffer, rightEyebrowVertices);

                    rightEyeVertices = VertexPositionColor.Convert(rightEye, SharpDX.Color.OrangeRed);
                    rightEyeVertexBuffer = D3D11.Buffer.Create(d3dDevice, D3D11.BindFlags.VertexBuffer, rightEyeVertices);

                    leftEyebrowVertices = VertexPositionColor.Convert(leftEyebrow, SharpDX.Color.OrangeRed);
                    leftEyebrowVertexBuffer = D3D11.Buffer.Create(d3dDevice, D3D11.BindFlags.VertexBuffer, leftEyebrowVertices);

                    leftEyeVertices = VertexPositionColor.Convert(leftEye, SharpDX.Color.OrangeRed);
                    leftEyeVertexBuffer = D3D11.Buffer.Create(d3dDevice, D3D11.BindFlags.VertexBuffer, leftEyeVertices);

                    noseVertices = VertexPositionColor.Convert(nose, SharpDX.Color.OrangeRed);
                    noseVertexBuffer = D3D11.Buffer.Create(d3dDevice, D3D11.BindFlags.VertexBuffer, noseVertices);

                    mouthVertices = VertexPositionColor.Convert(mouth, SharpDX.Color.OrangeRed);
                    mouthVertexBuffer = D3D11.Buffer.Create(d3dDevice, D3D11.BindFlags.VertexBuffer, mouthVertices);
                }

                public void Dispose()
                {
                    inputLayout.Dispose();
                    inputSignature.Dispose();
                    mouthVertexBuffer.Dispose();
                    leftEyebrowVertexBuffer.Dispose();
                    leftEyeVertexBuffer.Dispose();
                    rightEyebrowVertexBuffer.Dispose();
                    rightEyeVertexBuffer.Dispose();
                    noseVertexBuffer.Dispose();
                    vertexShader.Dispose();
                    pixelShader.Dispose();
                    renderTargetView.Dispose();
                    swapChain.Dispose();
                    d3dDevice.Dispose();
                    d3dDeviceContext.Dispose();
                    renderForm.Dispose();
                }
                */
    }



}