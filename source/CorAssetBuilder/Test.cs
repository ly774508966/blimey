﻿using System;
using Cor;
using System.Collections.Generic;
using Abacus.SinglePrecision;
using Abacus.Packed;
using ServiceStack.Text;

namespace CorAssetBuilder
{
	public class Test
	{
		public static void Run ()
		{
			JsConfig.IncludeTypeInfo = true;
			var parameter = new ShaderDefinition()
			{
				Name = "VertexLit",
				PassNames = new List<string>() { "Main" },
				InputDefinitions = new List<ShaderInputDefinition>()
				{
					new ShaderInputDefinition()
					{
						Name = "a_vertPos",
						Type = typeof(Vector3),
						Usage = VertexElementUsage.Position,
						DefaultValue = Vector3.Zero,
						Optional = false,
					},
					new ShaderInputDefinition()
					{
						Name = "a_vertNormal",
						Type = typeof(Vector3),
						Usage = VertexElementUsage.Normal,
						DefaultValue = Vector3.Zero,
						Optional = false,
					},
					new ShaderInputDefinition()
					{
						Name = "a_vertTexcoord",
						Type = typeof(Vector2),
						Usage = VertexElementUsage.TextureCoordinate,
						DefaultValue = Vector2.Zero,
						Optional = true,
					},
					new ShaderInputDefinition()
					{
						Name = "a_vertColour",
						Type = typeof(Rgba32),
						Usage = VertexElementUsage.Colour,
						DefaultValue = Rgba32.White,
						Optional = true,
					},
				},
				SamplerDefinitions = new List<ShaderSamplerDefinition>()
				{
					new ShaderSamplerDefinition()
					{
						NiceName = "TextureSampler",
						Name = "s_tex0",
						Optional = true,
					}
				},
				VariableDefinitions = new List<ShaderVariableDefinition>()
				{
					new ShaderVariableDefinition()
					{
						NiceName = "World",
						Name = "u_world",
						Type = typeof(Matrix44),
						DefaultValue = Matrix44.Identity,
					},
					new ShaderVariableDefinition()
					{
						NiceName = "View",
						Name = "u_view",
						Type = typeof(Matrix44),
						DefaultValue = Matrix44.Identity,
					},
					new ShaderVariableDefinition()
					{
						NiceName = "Projection",
						Name = "u_proj",
						Type = typeof(Matrix44),
						DefaultValue = Matrix44.Identity,
					},
					new ShaderVariableDefinition()
					{
						NiceName = "MaterialColour",
						Name = "u_colour",
						Type = typeof(Rgba32),
						DefaultValue = Rgba32.White,
					},
					new ShaderVariableDefinition()
					{
						NiceName = "AmbientLightColour",
						Name = "u_liAmbient",
						Type = typeof(Rgba32),
						DefaultValue = Rgba32.Grey,
					},
					new ShaderVariableDefinition()
					{
						NiceName = "EmissiveColour",
						Name = "u_emissiveColour",
						Type = typeof(Rgba32),
						DefaultValue = Rgba32.Black,
					},
					new ShaderVariableDefinition()
					{
						NiceName = "SpecularColour",
						Name = "u_specularColour",
						Type = typeof(Rgba32),
						DefaultValue = Rgba32.White,
					},
					new ShaderVariableDefinition()
					{
						NiceName = "SpecularPower",
						Name = "u_specularPower",
						Type = typeof(Single),
						DefaultValue = 0.7f,
					},
					new ShaderVariableDefinition()
					{
						NiceName = "EyePosition",
						Name = "u_eyePosition",
						Type = typeof(Vector3),
						DefaultValue = Vector3.Zero,
					},
					new ShaderVariableDefinition()
					{
						NiceName = "FogEnabled",
						Name = "u_fogEnabled",
						Type = typeof(Single),
						DefaultValue = 1f,
					},
					new ShaderVariableDefinition()
					{
						NiceName = "FogStart",
						Name = "u_fogStart",
						Type = typeof(Single),
						DefaultValue = 100f,
					},
					new ShaderVariableDefinition()
					{
						NiceName = "FogEnd",
						Name = "u_fogEnd",
						Type = typeof(Single),
						DefaultValue = 1000f,
					},
					new ShaderVariableDefinition()
					{
						NiceName = "FogColour",
						Name = "u_fogColour",
						Type = typeof(Rgba32),
						DefaultValue = Rgba32.Blue,
					},
					new ShaderVariableDefinition()
					{
						NiceName = "DirectionalLight0Direction",
						Name = "u_li0Dir",
						Type = typeof(Vector3),
						DefaultValue = Vector3.Down,
					},
					new ShaderVariableDefinition()
					{
						NiceName = "DirectionalLight0DiffuseColour",
						Name = "u_li0Diffuse",
						Type = typeof(Rgba32),
						DefaultValue = Rgba32.Red,
					},
					new ShaderVariableDefinition()
					{
						NiceName = "DirectionalLight0SpecularColour",
						Name = "u_li0Spec",
						Type = typeof(Rgba32),
						DefaultValue = Rgba32.Salmon,
					},
					new ShaderVariableDefinition()
					{
						NiceName = "DirectionalLight1Direction",
						Name = "u_li1Dir",
						Type = typeof(Vector3),
						DefaultValue = Vector3.Down,
					},
					new ShaderVariableDefinition()
					{
						NiceName = "DirectionalLight1DiffuseColour",
						Name = "u_li1Diffuse",
						Type = typeof(Rgba32),
						DefaultValue = Rgba32.Red,
					},
					new ShaderVariableDefinition()
					{
						NiceName = "DirectionalLight1SpecularColour",
						Name = "u_li1Spec",
						Type = typeof(Rgba32),
						DefaultValue = Rgba32.Salmon,
					},
					new ShaderVariableDefinition()
					{
						NiceName = "DirectionalLight2Direction",
						Name = "u_li2Dir",
						Type = typeof(Vector3),
						DefaultValue = Vector3.Down,
					},
					new ShaderVariableDefinition()
					{
						NiceName = "DirectionalLight2DiffuseColour",
						Name = "u_li2Diffuse",
						Type = typeof(Rgba32),
						DefaultValue = Rgba32.Red,
					},
					new ShaderVariableDefinition()
					{
						NiceName = "DirectionalLight2SpecularColour",
						Name = "u_li2Spec",
						Type = typeof(Rgba32),
						DefaultValue = Rgba32.Salmon,
					},
				},
			};


			string json = parameter.ToJson ();
			Console.WriteLine (json);
		}
	}
}

