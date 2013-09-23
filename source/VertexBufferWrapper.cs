using Sungiant.Cor;



namespace Sungiant.Cor.Xna4Runtime
{
	public class VertexBufferWrapper
		: IVertexBuffer
	{
		Microsoft.Xna.Framework.Graphics.VertexBuffer _xnaVertBuf;
		VertexDeclaration _vertexDeclaration;

		public VertexDeclaration VertexDeclaration 
		{ 
			get
			{
				return _vertexDeclaration;
			}
		}

		internal Microsoft.Xna.Framework.Graphics.VertexBuffer XNAVertexBuffer
		{
			get
			{
				return _xnaVertBuf;
			}
		}

		public VertexBufferWrapper(Microsoft.Xna.Framework.Graphics.GraphicsDevice gfx, VertexDeclaration vertexDeclaration, int vertexCount)
		{
			Microsoft.Xna.Framework.Graphics.VertexDeclaration xnaVertDecl = vertexDeclaration.ToXNA();

			_vertexDeclaration = vertexDeclaration;
			_xnaVertBuf = new Microsoft.Xna.Framework.Graphics.VertexBuffer(
				gfx,
				xnaVertDecl, 
				vertexCount, 
				Microsoft.Xna.Framework.Graphics.BufferUsage.None
				);
		}

		public void GetData<T>(T[] data) where T : struct, IVertexType
		{
			_xnaVertBuf.GetData<T>(data);
		}

		public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct, IVertexType
		{
			_xnaVertBuf.GetData<T>(data, startIndex, elementCount);
		}

		public void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct, IVertexType
		{
			_xnaVertBuf.GetData<T>(offsetInBytes, data, startIndex, elementCount, vertexStride);
		}

		public void SetData<T>(T[] data) where T : struct, IVertexType
		{
			_xnaVertBuf.SetData<T>(data);
		}

		public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct, IVertexType
		{
			_xnaVertBuf.SetData<T>(data, startIndex, elementCount);
		}

		public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct, IVertexType
		{
			_xnaVertBuf.SetData<T>(offsetInBytes, data, startIndex, elementCount, vertexStride);
		}


		public int VertexCount
		{
			get
			{
				return _xnaVertBuf.VertexCount;
			}
		}
	}
}
