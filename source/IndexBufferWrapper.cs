using System;
using Sungiant.Cor;

namespace Sungiant.Cor.Xna4Runtime
{
	/// <summary>
	/// This is a game component that implements IUpdateable.
	/// </summary>
	public class IndexBufferWrapper
		: IIndexBuffer
	{
		Microsoft.Xna.Framework.Graphics.IndexBuffer _xnaIndexBuf;

		internal Microsoft.Xna.Framework.Graphics.IndexBuffer XNAIndexBuffer
		{
			get
			{
				return _xnaIndexBuf;
			}
		}

		public IndexBufferWrapper(Microsoft.Xna.Framework.Graphics.GraphicsDevice gfx, Int32 indexCount)
		{
			_xnaIndexBuf = new Microsoft.Xna.Framework.Graphics.IndexBuffer(
				gfx, 
				typeof(UInt16), 
				indexCount, 
				Microsoft.Xna.Framework.Graphics.BufferUsage.None
				);

		}

		public void GetData(Int32[] data)
		{
			throw new System.NotImplementedException();
			//_xnaIndexBuf.GetData<UInt16>(data);
		}

		/*
		public void GetData(UInt16[] data, int startIndex, int elementCount)
		{
			_xnaIndexBuf.GetData<UInt16>(data, startIndex, elementCount);
		}

		public void GetData(int offsetInBytes, UInt16[] data, int startIndex, int elementCount)
		{
			_xnaIndexBuf.GetData<UInt16>(offsetInBytes, data, startIndex, elementCount);
		}
		 */

		public void SetData(Int32[] data)
		{
			UInt16[] udata = new UInt16[data.Length];

			for (Int32 i = 0; i < data.Length; ++i)
			{
				udata[i] = (UInt16)data[i];
			}

			_xnaIndexBuf.SetData<UInt16>(udata);
		}

		/*
		public void SetData(UInt16[] data, int startIndex, int elementCount)
		{
			_xnaIndexBuf.SetData<UInt16>(data, startIndex, elementCount);
		}

		public void SetData(int offsetInBytes, UInt16[] data, int startIndex, int elementCount)
		{
			_xnaIndexBuf.SetData(offsetInBytes, data, startIndex, elementCount);
		}
		*/

		public int IndexCount 
		{ 
			get
			{
				return _xnaIndexBuf.IndexCount;
			}
		}
	}
}
