﻿using System;
using Sungiant.Abacus.Packed;
using Sungiant.Abacus.SinglePrecision;
using Sungiant.Cor;

namespace Sungiant.Cor.Platform.Managed.Xna4
{
	internal class DisplayStatus
		: IDisplayStatus
	{
		Microsoft.Xna.Framework.GraphicsDeviceManager gfxManager;

		internal DisplayStatus(Microsoft.Xna.Framework.GraphicsDeviceManager gfxManager)
		{
			this.gfxManager = gfxManager;
		}

		public Boolean Fullscreen { get { return this.gfxManager.IsFullScreen; } }

		// What is the size of the frame buffer?
		// On most devices this will be the same as the screen size.
		// However on a PC or Mac the app could be running in windowed mode
		// and not take up the whole screen.
		public Int32 CurrentWidth { get { return this.gfxManager.GraphicsDevice.PresentationParameters.BackBufferWidth; } }
		public Int32 CurrentHeight { get { return this.gfxManager.GraphicsDevice.PresentationParameters.BackBufferHeight; } }
	}

	public class GraphicsManager
		: IGraphicsManager
	{
		Microsoft.Xna.Framework.GraphicsDeviceManager _xnaGfxDeviceManager;
		GpuUtils _gpuUtils;
		DisplayStatus displayStatus;

		public GraphicsManager(ICor engine, Microsoft.Xna.Framework.GraphicsDeviceManager gfxManager)
		{
			_xnaGfxDeviceManager = gfxManager;

			_xnaGfxDeviceManager.GraphicsDevice.RasterizerState = Microsoft.Xna.Framework.Graphics.RasterizerState.CullNone;
			_xnaGfxDeviceManager.GraphicsDevice.BlendState = Microsoft.Xna.Framework.Graphics.BlendState.Opaque;
			_xnaGfxDeviceManager.GraphicsDevice.DepthStencilState = Microsoft.Xna.Framework.Graphics.DepthStencilState.Default;

			_gpuUtils = new GpuUtils();

			displayStatus = new DisplayStatus(_xnaGfxDeviceManager);
		}



        #region IGraphicsManager

        public IDisplayStatus DisplayStatus
        {
            get
            {
                return displayStatus;
            }
        }

        public IGpuUtils GpuUtils
        {
            get
            {
                return _gpuUtils;
            }
        }

        public void Reset()
        {

        }

        public void ClearColourBuffer(Rgba32 col = new Rgba32())
        {
            var xnaCol = col.ToXNA();

            _xnaGfxDeviceManager.GraphicsDevice.Clear(xnaCol);
        }

        public void ClearDepthBuffer(Single z = 1f)
        {

            _xnaGfxDeviceManager.GraphicsDevice.Clear(
                Microsoft.Xna.Framework.Graphics.ClearOptions.DepthBuffer,
                Microsoft.Xna.Framework.Vector4.Zero,
                z,
                0);
        }


        public void SetCullMode(CullMode cullMode)
        {

        }

        public IGeometryBuffer CreateGeometryBuffer(
            VertexDeclaration vertexDeclaration,
            Int32 vertexCount,
            Int32 indexCount)
        {
            return new GeometryBuffer(_xnaGfxDeviceManager.GraphicsDevice, vertexDeclaration, vertexCount, indexCount);
        }

        public void SetActiveGeometryBuffer(IGeometryBuffer buffer)
        {
            var vbuf = buffer.VertexBuffer as VertexBufferWrapper;

            _xnaGfxDeviceManager.GraphicsDevice.SetVertexBuffer(vbuf.XNAVertexBuffer);

            var ibuf = buffer.IndexBuffer as IndexBufferWrapper;

            _xnaGfxDeviceManager.GraphicsDevice.Indices = ibuf.XNAIndexBuffer;
        }

        public void SetActiveTexture(Int32 slot, Texture2D tex)
        {

        }


        public void SetBlendEquation(
            BlendFunction rgbBlendFunction, BlendFactor sourceRgb, BlendFactor destinationRgb,
            BlendFunction alphaBlendFunction, BlendFactor sourceAlpha, BlendFactor destinationAlpha
            )
        {

        }


        public void DrawPrimitives(
            PrimitiveType primitiveType,            // Describes the type of primitive to render.
            Int32 startVertex,                      // Index of the first vertex to load. Beginning at startVertex, the correct number of vertices is read out of the vertex buffer.
            Int32 primitiveCount)                  // Number of primitives to render. The primitiveCount is the number of primitives as determined by the primitive type. If it is a line list, each primitive has two vertices. If it is a triangle list, each primitive has three vertices.
        {
            throw new NotImplementedException();
        }

        public void DrawIndexedPrimitives(
            PrimitiveType primitiveType,            // Describes the type of primitive to render. PrimitiveType.PointList is not supported with this method.
            Int32 baseVertex,                       // . Offset to add to each vertex index in the index buffer.
            Int32 minVertexIndex,                   // . Minimum vertex index for vertices used during the call. The minVertexIndex parameter and all of the indices in the index stream are relative to the baseVertex parameter.
            Int32 numVertices,                      // Number of vertices used during the call. The first vertex is located at index: baseVertex + minVertexIndex.
            Int32 startIndex,                       // . Location in the index array at which to start reading vertices.
            Int32 primitiveCount                    // Number of primitives to render. The number of vertices used is a function of primitiveCount and primitiveType.
            )
        {
            var xnaPrimType = EnumConverter.ToXNA(primitiveType);
            _xnaGfxDeviceManager.GraphicsDevice.DrawIndexedPrimitives(xnaPrimType, 0, 0, numVertices, 0, primitiveCount);
        }

        public void DrawUserPrimitives<T>(
            PrimitiveType primitiveType,            // Describes the type of primitive to render.
            T[] vertexData,                         // The vertex data.
            Int32 vertexOffset,                     // Offset (in vertices) from the beginning of the buffer to start reading data.
            Int32 primitiveCount,                   // Number of primitives to render.
            VertexDeclaration vertexDeclaration)   // The vertex declaration, which defines per-vertex data.
            where T : struct, IVertexType
        {
            var xnaPrimType = EnumConverter.ToXNA(primitiveType);
            var xnaVertDecl = vertexDeclaration.ToXNA();

            _xnaGfxDeviceManager.GraphicsDevice.DrawUserPrimitives(
                xnaPrimType, vertexData, vertexOffset, primitiveCount, xnaVertDecl);
        }

        public void DrawUserIndexedPrimitives<T>(
            PrimitiveType primitiveType,            // Describes the type of primitive to render.
            T[] vertexData,                         // The vertex data.
            Int32 vertexOffset,                     // Offset (in vertices) from the beginning of the vertex buffer to the first vertex to draw.
            Int32 numVertices,                      // Number of vertices to draw.
            Int32[] indexData,                      // The index data.
            Int32 indexOffset,                      // Offset (in indices) from the beginning of the index buffer to the first index to use.
            Int32 primitiveCount,                   // Number of primitives to render.
            VertexDeclaration vertexDeclaration)
            where T : struct, IVertexType
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}