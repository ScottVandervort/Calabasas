using System;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using SharpDX.DirectWrite;
using SharpDX.D3DCompiler;
using SharpDX.Mathematics.Interop;

namespace Calabasas
{
    public class PumpkinFaceRenderer3D : PumpkinFaceRenderer<RawVector3>
    {    
        private SharpDX.Direct3D11.Device d3dDevice;
        private SharpDX.Direct3D11.DeviceContext d3dDeviceContext;
        private SwapChain swapChain;
        private SharpDX.Direct3D11.RenderTargetView renderTargetView;
        private Viewport viewport;

        // Shaders
        private SharpDX.Direct3D11.VertexShader vertexShader;
        private SharpDX.Direct3D11.PixelShader pixelShader;
        private ShaderSignature inputSignature;
        private SharpDX.Direct3D11.InputLayout inputLayout;

        private SharpDX.Direct3D11.InputElement[] inputElements = new SharpDX.Direct3D11.InputElement[]
        {
            new SharpDX.Direct3D11.InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, SharpDX.Direct3D11.InputClassification.PerVertexData, 0),
            new SharpDX.Direct3D11.InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0, SharpDX.Direct3D11.InputClassification.PerVertexData, 0)
        };

        private RawVector3 [] points;
        private Boolean pointsChanged;

        private VertexPositionColor[] vertices;
        private SharpDX.Direct3D11.Buffer vertexBuffer;

        public PumpkinFaceRenderer3D(IFaceCamera<RawVector3> faceCamera) : base(faceCamera)
        {
            InitializeDeviceResources();
            InitializeShaders();
        }

        private void InitializeDeviceResources()
        {
            // Descriptor for the swap chain
            SwapChainDescription swapChainDesc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(Width, Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = renderForm.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            // Create device and swap chain
            SharpDX.Direct3D11.Device.CreateWithSwapChain(
                DriverType.Hardware, 
                SharpDX.Direct3D11.DeviceCreationFlags.None, 
                swapChainDesc, 
                out d3dDevice, 
                out swapChain);

            d3dDeviceContext = d3dDevice.ImmediateContext;

            viewport = new Viewport(0, 0, Width, Height);
            d3dDeviceContext.Rasterizer.SetViewport(viewport);

            // Create render target view for back buffer
            using (SharpDX.Direct3D11.Texture2D backBuffer = swapChain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>(0))
            {
                renderTargetView = new SharpDX.Direct3D11.RenderTargetView(d3dDevice, backBuffer);
            }
        }

        private void InitializeShaders()
        {
            // Compile the vertex shader code
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile("vertexShader.hlsl", "main", "vs_4_0", ShaderFlags.Debug))
            {
                // Read input signature from shader code
                inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

                vertexShader = new SharpDX.Direct3D11.VertexShader(d3dDevice, vertexShaderByteCode);
            }

            // Compile the pixel shader code
            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("pixelShader.hlsl", "main", "ps_4_0", ShaderFlags.Debug))
            {
                pixelShader = new SharpDX.Direct3D11.PixelShader(d3dDevice, pixelShaderByteCode);
            }

            // Set as current vertex and pixel shaders
            d3dDeviceContext.VertexShader.Set(vertexShader);
            d3dDeviceContext.PixelShader.Set(pixelShader);

            d3dDeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            // Create the input layout from the input signature and the input elements
            inputLayout = new SharpDX.Direct3D11.InputLayout(d3dDevice, inputSignature, inputElements);

            // Set input layout to use
            d3dDeviceContext.InputAssembler.InputLayout = inputLayout;
        }

        protected override void OnRenderCallback()
        {
            // Set render targets
            d3dDeviceContext.OutputMerger.SetRenderTargets(renderTargetView);

            // Clear the screen
            d3dDeviceContext.ClearRenderTargetView(renderTargetView, new SharpDX.Color(32, 103, 178));

            // Refresh vertex buffer.
            if (pointsChanged)
            {
                if (this.vertexBuffer != null)
                    this.vertexBuffer.Dispose();

                this.vertices = new VertexPositionColor[points.Length];
                for (int vertexIndex = 0; vertexIndex < points.Length; vertexIndex++)
                    this.vertices[vertexIndex] = new VertexPositionColor(points[vertexIndex], SharpDX.Color.OrangeRed);

                this.vertexBuffer = SharpDX.Direct3D11.Buffer.Create(d3dDevice, SharpDX.Direct3D11.BindFlags.VertexBuffer, vertices);
            }

            if (this.vertexBuffer != null && this.vertices != null)
            {
                // Set vertex buffer
                d3dDeviceContext.InputAssembler.SetVertexBuffers(0, new SharpDX.Direct3D11.VertexBufferBinding(this.vertexBuffer, Utilities.SizeOf<VertexPositionColor>(), 0));

                // Draw the triangle
                d3dDeviceContext.Draw(this.vertices.Length, 0);

                // Swap front and back buffer
                swapChain.Present(1, PresentFlags.None);
            }

            base.OnRenderCallback();
        }
     
        override public void Draw (RawVector3 [] points)
        {
            this.points = points;
            this.pointsChanged = true;
        }

        override public void Dispose()
        {
            base.Dispose();

            inputLayout.Dispose();
            inputSignature.Dispose();
            vertexBuffer.Dispose();
            vertexShader.Dispose();
            pixelShader.Dispose();
            renderTargetView.Dispose();
            swapChain.Dispose();
            d3dDevice.Dispose();
            d3dDeviceContext.Dispose();
            renderForm.Dispose();
        }                           
    }
}


