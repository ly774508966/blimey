// ┌────────────────────────────────────────────────────────────────────────┐ \\
// │ Cor! - Low Level 3D App Engine                                         │ \\
// ├────────────────────────────────────────────────────────────────────────┤ \\
// │ Brought to you by:                                                     │ \\
// │          _________                    .__               __             │ \\
// │         /   _____/__ __  ____    ____ |__|____    _____/  |_           │ \\
// │         \_____  \|  |  \/    \  / ___\|  \__  \  /    \   __\          │ \\
// │         /        \  |  /   |  \/ /_/  >  |/ __ \|   |  \  |            │ \\
// │        /_______  /____/|___|  /\___  /|__(____  /___|  /__|            │ \\
// │                \/           \//_____/         \/     \/                │ \\
// │                                                                        │ \\
// ├────────────────────────────────────────────────────────────────────────┤ \\
// │ Copyright © 2013 A.J.Pook (http://sungiant.github.com)                 │ \\
// ├────────────────────────────────────────────────────────────────────────┤ \\
// │ Permission is hereby granted, free of charge, to any person obtaining  │ \\
// │ a copy of this software and associated documentation files (the        │ \\
// │ "Software"), to deal in the Software without restriction, including    │ \\
// │ without limitation the rights to use, copy, modify, merge, publish,    │ \\
// │ distribute, sublicense, and/or sellcopies of the Software, and to      │ \\
// │ permit persons to whom the Software is furnished to do so, subject to  │ \\
// │ the following conditions:                                              │ \\
// │                                                                        │ \\
// │ The above copyright notice and this permission notice shall be         │ \\
// │ included in all copies or substantial portions of the Software.        │ \\
// │                                                                        │ \\
// │ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,        │ \\
// │ EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF     │ \\
// │ MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. │ \\
// │ IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY   │ \\
// │ CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,   │ \\
// │ TORT OR OTHERWISE, ARISING FROM,OUT OF OR IN CONNECTION WITH THE       │ \\
// │ SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                 │ \\
// └────────────────────────────────────────────────────────────────────────┘ \\

using System;
using Sungiant.Abacus.SinglePrecision;

namespace Sungiant.Cor.MonoTouchRuntime
{
	public static class Vector2Converter
	{
		// VECTOR 2
		public static OpenTK.Vector2 ToOpenTK (this Vector2 vec)
		{
			return new OpenTK.Vector2 (vec.X, vec.Y);
		}

		public static Vector2 ToAbacus (this OpenTK.Vector2 vec)
		{
			return new Vector2 (vec.X, vec.Y);
		}

		
		public static System.Drawing.PointF ToSystemDrawing(this Vector2 vec)
		{
			return new System.Drawing.PointF (vec.X, vec.Y);
		}

		public static Vector2 ToAbacus (this System.Drawing.PointF vec)
		{
			return new Vector2 (vec.X, vec.Y);
		}
	}

	
	public static class Vector3Converter
	{
		// VECTOR 3
		public static OpenTK.Vector3 ToOpenTK (this Vector3 vec)
		{
			return new OpenTK.Vector3 (vec.X, vec.Y, vec.Z);
		}

		public static Vector3 ToAbacus (this OpenTK.Vector3 vec)
		{
			return new Vector3 (vec.X, vec.Y, vec.Z);
		}
	}
	
	public static class Vector4Converter
	{
		// VECTOR 3
		public static OpenTK.Vector4 ToOpenTK (this Vector4 vec)
		{
			return new OpenTK.Vector4 (vec.X, vec.Y, vec.Z, vec.W);
		}

		public static Vector4 ToAbacus (this OpenTK.Vector4 vec)
		{
			return new Vector4 (vec.X, vec.Y, vec.Z, vec.W);
		}
	}

	public static class MatrixConverter
	{
		static bool flip = false;

		// MATRIX
		public static OpenTK.Matrix4 ToOpenTK (this Matrix44 mat)
		{
			if( flip )
			{
				return new OpenTK.Matrix4(
					mat.M11, mat.M21, mat.M31, mat.M41,
					mat.M12, mat.M22, mat.M32, mat.M42,
					mat.M13, mat.M23, mat.M33, mat.M43,
					mat.M14, mat.M24, mat.M34, mat.M44
					);
			}
			else
			{
				return new OpenTK.Matrix4(
					mat.M11, mat.M12, mat.M13, mat.M14,
					mat.M21, mat.M22, mat.M23, mat.M24,
					mat.M31, mat.M32, mat.M33, mat.M34,
					mat.M41, mat.M42, mat.M43, mat.M44
					);
			}
		}

		public static Matrix44 ToAbacus (this OpenTK.Matrix4 mat)
		{

			if( flip )
			{
				return new Matrix44(
					mat.M11, mat.M21, mat.M31, mat.M41,
					mat.M12, mat.M22, mat.M32, mat.M42,
					mat.M13, mat.M23, mat.M33, mat.M43,
					mat.M14, mat.M24, mat.M34, mat.M44
					);
			}
			else
			{
				return new Matrix44(
					mat.M11, mat.M12, mat.M13, mat.M14,
					mat.M21, mat.M22, mat.M23, mat.M24,
					mat.M31, mat.M32, mat.M33, mat.M34,
					mat.M41, mat.M42, mat.M43, mat.M44
					);
			}
		}

	}


	public static class EnumConverter
	{

		public static DeviceOrientation ToCor(MonoTouch.UIKit.UIDeviceOrientation monoTouch)
		{
			switch(monoTouch)
			{
				case MonoTouch.UIKit.UIDeviceOrientation.FaceDown: return DeviceOrientation.Default;
				case MonoTouch.UIKit.UIDeviceOrientation.FaceUp: return DeviceOrientation.Default;
				case MonoTouch.UIKit.UIDeviceOrientation.LandscapeLeft: return DeviceOrientation.Leftside;
				case MonoTouch.UIKit.UIDeviceOrientation.LandscapeRight: return DeviceOrientation.Rightside;
				case MonoTouch.UIKit.UIDeviceOrientation.Portrait: return DeviceOrientation.Default;
				case MonoTouch.UIKit.UIDeviceOrientation.PortraitUpsideDown: return DeviceOrientation.Upsidedown;
			}

			return Sungiant.Cor.DeviceOrientation.Default;
		}

		public static TouchPhase ToCorPrimitiveType(MonoTouch.UIKit.UITouchPhase phase)
		{
			switch(phase)
			{
				case MonoTouch.UIKit.UITouchPhase.Began: return TouchPhase.JustPressed;
				case MonoTouch.UIKit.UITouchPhase.Cancelled: return TouchPhase.JustReleased;
				case MonoTouch.UIKit.UITouchPhase.Ended: return TouchPhase.JustReleased;
				case MonoTouch.UIKit.UITouchPhase.Moved: return TouchPhase.Active;
				case MonoTouch.UIKit.UITouchPhase.Stationary: return TouchPhase.Active;
			}

			return TouchPhase.Invalid;
		}

        public static OpenTK.Graphics.ES20.All ToOpenTKTextureSlot(Int32 slot)
        {
            switch(slot)
            {
                case 0: return OpenTK.Graphics.ES20.All.Texture0;
                case 1: return OpenTK.Graphics.ES20.All.Texture1;
                case 2: return OpenTK.Graphics.ES20.All.Texture2;
                case 3: return  OpenTK.Graphics.ES20.All.Texture3;
                case 4: return OpenTK.Graphics.ES20.All.Texture4;
                case 5: return OpenTK.Graphics.ES20.All.Texture5;
                case 6: return OpenTK.Graphics.ES20.All.Texture6;
                case 7: return OpenTK.Graphics.ES20.All.Texture7;
                case 8: return OpenTK.Graphics.ES20.All.Texture8;
                case 9: return OpenTK.Graphics.ES20.All.Texture9;
                case 10: return OpenTK.Graphics.ES20.All.Texture10;
                case 11: return OpenTK.Graphics.ES20.All.Texture11;
                case 12: return OpenTK.Graphics.ES20.All.Texture12;
                case 13: return OpenTK.Graphics.ES20.All.Texture13;
                case 14: return OpenTK.Graphics.ES20.All.Texture14;
                case 15: return OpenTK.Graphics.ES20.All.Texture15;
                case 16: return OpenTK.Graphics.ES20.All.Texture16;
                case 17: return OpenTK.Graphics.ES20.All.Texture17;
                case 18: return OpenTK.Graphics.ES20.All.Texture18;
                case 19: return OpenTK.Graphics.ES20.All.Texture19;
                case 20: return OpenTK.Graphics.ES20.All.Texture20;
                case 21: return OpenTK.Graphics.ES20.All.Texture21;
                case 22: return OpenTK.Graphics.ES20.All.Texture22;
                case 23: return OpenTK.Graphics.ES20.All.Texture23;
                case 24: return OpenTK.Graphics.ES20.All.Texture24;
                case 25: return OpenTK.Graphics.ES20.All.Texture25;
                case 26: return OpenTK.Graphics.ES20.All.Texture26;
                case 27: return OpenTK.Graphics.ES20.All.Texture27;
                case 28: return OpenTK.Graphics.ES20.All.Texture28;
                case 29: return OpenTK.Graphics.ES20.All.Texture29;
                case 30: return OpenTK.Graphics.ES20.All.Texture30;
            }

            throw new NotSupportedException();
        }


		public static void ToOpenTK (
			VertexElementFormat blimey,
			out OpenTK.Graphics.ES20.DataType dataFormat,
			out bool normalized,
			out int size)
		{
			normalized = false;
			size = 0;
			dataFormat = OpenTK.Graphics.ES20.DataType.Float;

			switch(blimey)
			{
				case VertexElementFormat.Single: 
					dataFormat = OpenTK.Graphics.ES20.DataType.Float;
					size = 1;
					break;
				case VertexElementFormat.Vector2: 
					dataFormat = OpenTK.Graphics.ES20.DataType.Float; 
					size = 2;
					break;
				case VertexElementFormat.Vector3: 
					dataFormat = OpenTK.Graphics.ES20.DataType.Float; 
					size = 3;
					break;
				case VertexElementFormat.Vector4: 
					dataFormat = OpenTK.Graphics.ES20.DataType.Float; 
					size = 4;
					break;
				case VertexElementFormat.Colour: 
					dataFormat = OpenTK.Graphics.ES20.DataType.UnsignedByte; 
					normalized = true;
					size = 4;
					break;
				case VertexElementFormat.Byte4: throw new Exception("?");
				case VertexElementFormat.Short2: throw new Exception("?");
				case VertexElementFormat.Short4: throw new Exception("?");
				case VertexElementFormat.NormalizedShort2: throw new Exception("?");
				case VertexElementFormat.NormalizedShort4: throw new Exception("?");
				case VertexElementFormat.HalfVector2: throw new Exception("?");
				case VertexElementFormat.HalfVector4: throw new Exception("?");
			}
		}

        public static OpenTK.Graphics.ES20.All ToOpenTK(BlendFactor blimey)
        {
            switch(blimey)
            {
                case BlendFactor.Zero: return OpenTK.Graphics.ES20.All.Zero;
                case BlendFactor.One: return OpenTK.Graphics.ES20.All.One;
                case BlendFactor.SourceColour: return OpenTK.Graphics.ES20.All.SrcColor;
                case BlendFactor.InverseSourceColour: return OpenTK.Graphics.ES20.All.OneMinusSrcColor;
                case BlendFactor.SourceAlpha: return OpenTK.Graphics.ES20.All.SrcAlpha;
                case BlendFactor.InverseSourceAlpha: return OpenTK.Graphics.ES20.All.OneMinusSrcAlpha;
                case BlendFactor.DestinationAlpha: return OpenTK.Graphics.ES20.All.DstAlpha;
                case BlendFactor.InverseDestinationAlpha: return OpenTK.Graphics.ES20.All.OneMinusDstAlpha;
                case BlendFactor.DestinationColour: return OpenTK.Graphics.ES20.All.DstColor;
                case BlendFactor.InverseDestinationColour: return OpenTK.Graphics.ES20.All.OneMinusDstColor;
            }

            throw new Exception();
        }

        public static BlendFactor ToCorDestinationBlendFactor (OpenTK.Graphics.ES20.All openTK)
        {
            switch(openTK)
            {
                case OpenTK.Graphics.ES20.All.Zero: return BlendFactor.Zero;
                case OpenTK.Graphics.ES20.All.One: return BlendFactor.One;
                case OpenTK.Graphics.ES20.All.SrcColor: return BlendFactor.SourceColour;
                case OpenTK.Graphics.ES20.All.OneMinusSrcColor: return BlendFactor.InverseSourceColour;
                case OpenTK.Graphics.ES20.All.SrcAlpha: return BlendFactor.SourceAlpha;
                case OpenTK.Graphics.ES20.All.OneMinusSrcAlpha: return BlendFactor.InverseSourceAlpha;
                case OpenTK.Graphics.ES20.All.DstAlpha: return BlendFactor.DestinationAlpha;
                case OpenTK.Graphics.ES20.All.OneMinusDstAlpha: return BlendFactor.InverseDestinationAlpha;
                case OpenTK.Graphics.ES20.All.DstColor: return BlendFactor.DestinationColour;
                case OpenTK.Graphics.ES20.All.OneMinusDstColor: return BlendFactor.InverseDestinationColour;
            }

            throw new Exception();
        }

        public static OpenTK.Graphics.ES20.All ToOpenTK(BlendFunction blimey)
        {
            switch(blimey)
            {
                case BlendFunction.Add: return OpenTK.Graphics.ES20.All.FuncAdd;
                case BlendFunction.Max: return OpenTK.Graphics.ES20.All.MaxExt;
                case BlendFunction.Min: return OpenTK.Graphics.ES20.All.MinExt;
                case BlendFunction.ReverseSubtract: return OpenTK.Graphics.ES20.All.FuncReverseSubtract;
                case BlendFunction.Subtract: return OpenTK.Graphics.ES20.All.FuncSubtract;
            }
            
            throw new Exception();
        }

        public static BlendFunction ToCorDestinationBlendFunction (OpenTK.Graphics.ES20.All openTK)
        {
            switch(openTK)
            {
                case OpenTK.Graphics.ES20.All.FuncAdd: return BlendFunction.Add;
                case OpenTK.Graphics.ES20.All.MaxExt: return BlendFunction.Max;
                case OpenTK.Graphics.ES20.All.MinExt: return BlendFunction.Min;
                case OpenTK.Graphics.ES20.All.FuncReverseSubtract: return BlendFunction.ReverseSubtract;
                case OpenTK.Graphics.ES20.All.FuncSubtract: return BlendFunction.Subtract;
            }
            
            throw new Exception();
        }

		// PRIMITIVE TYPE
		public static OpenTK.Graphics.ES20.BeginMode ToOpenTK (PrimitiveType blimey)
		{
			switch (blimey) {
			case PrimitiveType.LineList:
				return	OpenTK.Graphics.ES20.BeginMode.Lines;
			case PrimitiveType.LineStrip:
				return	OpenTK.Graphics.ES20.BeginMode.LineStrip;
			case PrimitiveType.TriangleList:
				return	OpenTK.Graphics.ES20.BeginMode.Triangles;
			case PrimitiveType.TriangleStrip:
				return	OpenTK.Graphics.ES20.BeginMode.TriangleStrip;
					
			default:
				throw new Exception ("problem");
			}
		}

		public static PrimitiveType ToCorPrimitiveType (OpenTK.Graphics.ES20.All openTK)
		{
			switch (openTK) {
			case OpenTK.Graphics.ES20.All.Lines:
				return	PrimitiveType.LineList;
			case OpenTK.Graphics.ES20.All.LineStrip:
				return	PrimitiveType.LineStrip;
			case OpenTK.Graphics.ES20.All.Points:
				throw new Exception ("Not supported by Cor");
			case OpenTK.Graphics.ES20.All.TriangleFan:
				throw new Exception ("Not supported by Cor");
			case OpenTK.Graphics.ES20.All.Triangles:
				return	PrimitiveType.TriangleList;
			case OpenTK.Graphics.ES20.All.TriangleStrip:
				return	PrimitiveType.TriangleStrip;
				
			default:
				throw new Exception ("problem");

			}
		}
	}
}

		/*

		// VERTEX ELEMENT FORMAT
		public static OpenTK.Graphics.ES20.All ToOpenTK (VertexElementFormat blimey)
		{
			switch (blimey) {
			case VertexElementFormat.Byte4: throw new NotImplementedException();
				//return OpenTK.Graphics.ES20.All.Byte;
			case VertexElementFormat.Color:
				return OpenTK.Graphics.ES20.All.FloatVec4;
				case VertexElementFormat.HalfVector2: throw new NotImplementedException();
				//return OpenTK.Graphics.ES20.All.Half2;
			case VertexElementFormat.HalfVector4: throw new NotImplementedException();
				//return OpenTK.Graphics.ES20.All.Half4;
			case VertexElementFormat.NormalizedShort2: throw new NotImplementedException();
				//return OpenTK.Graphics.ES20.All.Short2N;
			case VertexElementFormat.NormalizedShort4: throw new NotImplementedException();
				//return OpenTK.Graphics.ES20.All.Short4N;
			case VertexElementFormat.Short2: throw new NotImplementedException();
				//return OpenTK.Graphics.ES20.All.Short2;
			case VertexElementFormat.Short4: throw new NotImplementedException();
				//return OpenTK.Graphics.ES20.All.Short4;
			case VertexElementFormat.Single:
				return OpenTK.Graphics.ES20.All.Float;
			case VertexElementFormat.Vector2:
				return OpenTK.Graphics.ES20.All.FloatVec2;
			case VertexElementFormat.Vector3:
				return OpenTK.Graphics.ES20.All.FloatVec3;
			case VertexElementFormat.Vector4:
				return OpenTK.Graphics.ES20.All.FloatVec4;
				
			default:
				throw new Exception ("problem");
			}
		}

		public static VertexElementFormat ToCorVertexElementFormat (OpenTK.Graphics.ES20.All openTK)
		{
			switch (openTK) {
			case OpenTK.Graphics.ES20.All.None:
				throw new Exception ("Not supported by Cor");
			case OpenTK.Graphics.ES20.All.Float:
				return VertexElementFormat.Single;
			case OpenTK.Graphics.ES20.All.FloatVec2:
				return VertexElementFormat.Vector2;
			case OpenTK.Graphics.ES20.All.FloatVec3:
				return VertexElementFormat.Vector3;
			case OpenTK.Graphics.ES20.All.FloatVec4:
				return VertexElementFormat.Vector4;
			//case OpenTK.Graphics.ES20.All.Half:
			//	throw new Exception ("Not supported by Cor");
			//case OpenTK.Graphics.ES20.All.Half2:
			//	return VertexElementFormat.HalfVector2;
			//case OpenTK.Graphics.ES20.All.Half3:
			//	throw new Exception ("Not supported by Cor");
			//case OpenTK.Graphics.ES20.All.Half4:
			//	return VertexElementFormat.HalfVector4;
			//case OpenTK.Graphics.ES20.All.Short:
			//	throw new Exception ("Not supported by Cor");
			//case OpenTK.Graphics.ES20.All.Short2:
			//	return VertexElementFormat.Short2;
			//case OpenTK.Graphics.ES20.All.Short3:
			//	throw new Exception ("Not supported by Cor");
			//case OpenTK.Graphics.ES20.All.Short4:
			//	return VertexElementFormat.Short4;
			//case OpenTK.Graphics.ES20.All.UShort:
			//	throw new Exception ("Not supported by Cor");
			//case OpenTK.Graphics.ES20.All.UShort2:
			//	throw new Exception ("Not supported by Cor");
			//case OpenTK.Graphics.ES20.All.UShort3:
			//	throw new Exception ("Not supported by Cor");
			//case OpenTK.Graphics.ES20.All.UShort4:
			//	throw new Exception ("Not supported by Cor");
			case OpenTK.Graphics.ES20.All.Byte:
				throw new Exception ("Not supported by Cor");
			//case OpenTK.Graphics.ES20.All.Byte2:
			//	throw new Exception ("Not supported by Cor");
			//case OpenTK.Graphics.ES20.All.Byte3:
			//	throw new Exception ("Not supported by Cor");
			//case OpenTK.Graphics.ES20.All.Byte4:
			//	return VertexElementFormat.Byte4;
			//case OpenTK.Graphics.ES20.All.UByte:
			//	throw new Exception ("Not supported by Cor");
			//case OpenTK.Graphics.ES20.All.UByte2:
			//	throw new Exception ("Not supported by Cor");
			//case OpenTK.Graphics.ES20.All.UByte3:
			//	throw new Exception ("Not supported by Cor");
			//case OpenTK.Graphics.ES20.All.UByte4:
			//	throw new Exception ("Not supported by Cor");
			//case OpenTK.Graphics.ES20.All.ShortN:
			//	throw new Exception ("Not supported by Cor");
			//case OpenTK.Graphics.ES20.All.Short2N:
			//	return VertexElementFormat.NormalizedShort2;
			//case OpenTK.Graphics.ES20.All.Short3N:
			//	throw new Exception ("Not supported by Cor");
			//case OpenTK.Graphics.ES20.All.Short4N:
			//	return VertexElementFormat.NormalizedShort4;
			//case OpenTK.Graphics.ES20.All.UShortN:
			//	throw new Exception ("Not supported by Cor");
			//case OpenTK.Graphics.ES20.All.UShort2N:
			//	throw new Exception ("Not supported by Cor");	
			//case OpenTK.Graphics.ES20.All.UShort3N:
			//	throw new Exception ("Not supported by Cor");	
			//case OpenTK.Graphics.ES20.All.UShort4N:
			//	throw new Exception ("Not supported by Cor");	
			//case OpenTK.Graphics.ES20.All.ByteN:
			//	throw new Exception ("Not supported by Cor");
			//case OpenTK.Graphics.ES20.All.Byte2N:
			//	throw new Exception ("Not supported by Cor");
			//case OpenTK.Graphics.ES20.All.Byte3N:
			//	throw new Exception ("Not supported by Cor");	
			//case OpenTK.Graphics.ES20.All.Byte4N:
			//	throw new Exception ("Not supported by Cor");
			//case OpenTK.Graphics.ES20.All.UByteN:
			//	throw new Exception ("Not supported by Cor"); 	
			//case OpenTK.Graphics.ES20.All.UByte2N:
			//	throw new Exception ("Not supported by Cor");	
			//case OpenTK.Graphics.ES20.All.UByte3N:
			//	throw new Exception ("Not supported by Cor"); 	
			//case OpenTK.Graphics.ES20.All.UByte4N:
			//	throw new Exception ("Not supported by Cor");
				
			default:
				throw new Exception ("problem");
			}
		}

	}



	public static class VertexDeclarationConverter
	{

		public static Sce.Pss.Core.Graphics.VertexFormat[] ToMonoTouch (this VertexDeclaration blimey)
		{
			Int32 blimeyStride = blimey.VertexStride;

			VertexElement[] blimeyElements = blimey.GetVertexElements ();

			var pssElements = new Sce.Pss.Core.Graphics.VertexFormat[blimeyElements.Length];

			for (Int32 i = 0; i < blimeyElements.Length; ++i) {
				VertexElement elem = blimeyElements [i];
				pssElements [i] = elem.ToMonoTouch ();
			}

			return pssElements;
		}

	}

	public static class VertexElementConverter
	{
		public static Sce.Pss.Core.Graphics.VertexFormat ToMonoTouch (this VertexElement blimey)
		{
			Int32 bliOffset = blimey.Offset;
			var bliElementFormat = blimey.VertexElementFormat;
			var bliElementUsage = blimey.VertexElementUsage;
			Int32 bliUsageIndex = blimey.UsageIndex;
			
			return EnumConverter.ToMonoTouch (bliElementFormat);
		}
	}

*/


