using System;
using Sungiant.Cor;

namespace Sungiant.Blimey
{
	// high level wrapper for blending stuff
	public struct BlendMode
		: IEquatable<BlendMode>
	{
		BlendFunction rgbBlendFunction;
		BlendFactor sourceRgb;
		BlendFactor destinationRgb;

		BlendFunction alphaBlendFunction;
		BlendFactor sourceAlpha;
		BlendFactor destinationAlpha;

		public override String ToString ()
		{
			return string.Format (
				"{{rgbBlendFunction:{0} sourceRgb:{1} destinationRgb:{2} alphaBlendFunction:{3} sourceAlpha:{4} destinationAlpha:{5}}}"
				, new Object[]
					{ 
						rgbBlendFunction.ToString (), sourceRgb.ToString (), destinationRgb.ToString (),
						alphaBlendFunction.ToString (), sourceAlpha.ToString (), destinationAlpha.ToString () 
					}
			);
		}
		
		public Boolean Equals (BlendMode other)
		{
			return this == other;
		}
		
		public override Boolean Equals (Object obj)
		{
			Boolean flag = false;
			if (obj is BlendMode) {
				flag = this.Equals ((BlendMode)obj);
			}
			return flag;
		}
		
		public override Int32 GetHashCode ()
		{
			int a = (int) rgbBlendFunction.GetHashCode();
			int b = (int) sourceRgb.GetHashCode();
			int c = (int) destinationRgb.GetHashCode();
			
			int d = (int) alphaBlendFunction.GetHashCode();
			int e = (int) sourceAlpha.GetHashCode();
			int f = (int) destinationAlpha.GetHashCode();


			return a + b + c + d + e + f;
		}

		public static Boolean operator != (BlendMode value1, BlendMode value2)
		{
			return !(value1 == value2); 
		}

		public static Boolean operator == (BlendMode value1, BlendMode value2)
		{
			if (value1.rgbBlendFunction != value2.rgbBlendFunction) return false;
			if (value1.sourceRgb != value2.sourceRgb) return false;
			if (value1.destinationRgb != value2.destinationRgb) return false;
			if (value1.alphaBlendFunction != value2.alphaBlendFunction) return false;
			if (value1.sourceAlpha != value2.sourceAlpha) return false;
			if (value1.destinationAlpha != value2.destinationAlpha) return false;

			return true;
		}

		public static BlendMode Default
		{
			get
			{
				var blendMode = new BlendMode();
				
				blendMode.rgbBlendFunction = 	BlendFunction.Add;
				blendMode.sourceRgb = 			BlendFactor.SourceAlpha;
				blendMode.destinationRgb = 		BlendFactor.InverseSourceAlpha;
				
				blendMode.alphaBlendFunction = 	BlendFunction.Add;
				blendMode.sourceAlpha = 		BlendFactor.One;
				blendMode.destinationAlpha = 	BlendFactor.InverseSourceAlpha;
				
				return blendMode;
			}
		}

		public static BlendMode Opaque
		{
			get
			{
				var blendMode = new BlendMode();
				
				blendMode.rgbBlendFunction = 	BlendFunction.Add;
				blendMode.sourceRgb = 			BlendFactor.One;
				blendMode.destinationRgb = 		BlendFactor.Zero;
				
				blendMode.alphaBlendFunction = 	BlendFunction.Add;
				blendMode.sourceAlpha = 		BlendFactor.One;
				blendMode.destinationAlpha = 	BlendFactor.Zero;
				
				return blendMode;
			}
		}
		
		public static BlendMode Subtract
		{
			get
			{
				var blendMode = new BlendMode();

				blendMode.rgbBlendFunction = 	BlendFunction.ReverseSubtract;
				blendMode.sourceRgb = 			BlendFactor.SourceAlpha;
				blendMode.destinationRgb = 		BlendFactor.One;

				blendMode.alphaBlendFunction = 	BlendFunction.ReverseSubtract;
				blendMode.sourceAlpha = 		BlendFactor.SourceAlpha;
				blendMode.destinationAlpha = 	BlendFactor.One;

				return blendMode;
			}
		}

		public static BlendMode Additive
		{
			get
			{
				var blendMode = new BlendMode();
				
				blendMode.rgbBlendFunction = 	BlendFunction.Add;
				blendMode.sourceRgb = 			BlendFactor.SourceAlpha;
				blendMode.destinationRgb = 		BlendFactor.One;
				
				blendMode.alphaBlendFunction = 	BlendFunction.Add;
				blendMode.sourceAlpha = 		BlendFactor.SourceAlpha;
				blendMode.destinationAlpha = 	BlendFactor.One;
				
				return blendMode;
			}
		}

		static BlendMode lastSet = BlendMode.Default;
		static Boolean neverSet = true;

		public static void Apply(BlendMode blendMode, IGraphicsManager graphics)
		{
			if (neverSet || lastSet != blendMode)
			{
				graphics.SetBlendEquation (
					blendMode.rgbBlendFunction, blendMode.sourceRgb, blendMode.destinationRgb,
					blendMode.alphaBlendFunction, blendMode.sourceAlpha, blendMode.destinationAlpha
					);

				lastSet = blendMode;
			}
		}
	}
}

