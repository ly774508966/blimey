using System;
using Sungiant.Blimey;
using System.Collections.Generic;
using Sungiant.Abacus;

namespace Sungiant.Blimey.PsmRuntime
{
	public class Unlit_PositionTexture_ShaderEffectPass
		: IEffectPass
	{
		Sce.Pss.Core.Graphics.ShaderProgram _program;
		Sce.Pss.Core.Graphics.GraphicsContext _gfxContext;
		
		public Unlit_PositionTexture_ShaderEffectPass(Sce.Pss.Core.Graphics.GraphicsContext gfxContext, Sce.Pss.Core.Graphics.ShaderProgram program)
		{
			_gfxContext = gfxContext;
			_program = program;
		}
		
		public void Apply()
		{
			_gfxContext.SetShaderProgram( _program );
		}
	}
	
	public class Unlit_PositionTexture
		: IShader
	{
		Sce.Pss.Core.Graphics.ShaderProgram _program;
		Sce.Pss.Core.Graphics.GraphicsContext _gfxContext;
			
		Unlit_PositionTexture_ShaderEffectPass[] _pass;
		

		public Unlit_PositionTexture(
			Sce.Pss.Core.Graphics.GraphicsContext gfxContext, 
			Sce.Pss.Core.Graphics.ShaderProgram program)
		{
			_gfxContext = gfxContext;
			_program = program;
        	_program.SetUniformBinding(0, "WorldViewProj");
			_program.SetAttributeBinding( 0, "a_Position" ) ;
        	_program.SetAttributeBinding( 1, "a_TexCoord" ) ;
			
			_pass = new Unlit_PositionTexture_ShaderEffectPass[] { new Unlit_PositionTexture_ShaderEffectPass(_gfxContext, _program) }; 
		}

		public IEffectPass[] Passes
		{
			get
			{
				return _pass;
			}
		}

		public void Calibrate(Dictionary<string, ShaderSettingsData> settings)
		{
			Matrix world = Matrix.Identity;
			Matrix view = Matrix.Identity;
			Matrix proj = Matrix.Identity;
			Colour col = Colour.White;
			
			foreach (var param in settings.Keys)
			{
				if (param == "_world")
				{

					world = ((Matrix) settings[param].Value);
					continue;
				}

				if (param == "_view")
				{
					view = ((Matrix)settings[param].Value);
					continue;
				}

				if (param == "_proj")
				{
					proj = ((Matrix)settings[param].Value);
					continue;
				}

				if (param == "_colour")
				{
					col = ((Colour)settings[param].Value);
					continue;
				}
			}
			
			
			var worldViewProj = (world * view * proj).ToPSS();
			
			_program.SetUniformValue( 0, ref worldViewProj );

			                                               
	        Sce.Pss.Core.Vector4 materialColour	= col.ToVector4().ToPSS();
	        _program.SetUniformValue( _program.FindUniform( "MaterialColor" ), ref materialColour );
		}
	}
}
