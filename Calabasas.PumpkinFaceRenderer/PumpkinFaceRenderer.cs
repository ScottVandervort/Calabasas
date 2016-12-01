﻿using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3D11 = SharpDX.Direct3D11;

namespace Calabasas
{
    public class PumpkinFaceRenderer : IDisposable
    {
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
    }
}