using System.Collections.Generic;
using Sungiant.Abacus.Packed;
using Sungiant.Abacus.SinglePrecision;
using Sungiant.Cor;
using System;
using System.Linq;

namespace Sungiant.Cor.Platform.Managed.Xna4
{
	public class BasicEffectShaderAdapter
		: IShader
	{
        public class ShaderPassWrapper
            : IShaderPass
        {
            public delegate void AdjustBasicEffectForVertDeclDelegate(VertexDeclaration vertexDeclaration);

            readonly AdjustBasicEffectForVertDeclDelegate ajustBasicEffectForVertDecl;
            readonly Action applySettings;

            public string Name { get { return this.xnaEffectPass.Name; } }

            readonly Microsoft.Xna.Framework.Graphics.EffectPass xnaEffectPass;

            internal ShaderPassWrapper(
                Microsoft.Xna.Framework.Graphics.EffectPass xnaEffectPass,
                AdjustBasicEffectForVertDeclDelegate ajustBasicEffectForVertDecl,
                Action applySettings)
            {
                this.xnaEffectPass = xnaEffectPass;
                this.ajustBasicEffectForVertDecl = ajustBasicEffectForVertDecl;
                this.applySettings = applySettings;
            }

            public void Activate(VertexDeclaration vertexDeclaration)
            {
                this.ajustBasicEffectForVertDecl(vertexDeclaration);
                this.applySettings();
                this.xnaEffectPass.Apply();
            }
        }

        public enum BasicEffectVertFormat
        {
            VertPos,
            VertPosTex,
            VertPosCol,
            VertPosTexCol,
            VertPosNorm,
            VertPosNormTex,
            VertPosNormCol,
            VertPosNormTexCol
        }

        readonly IShaderPass[] passArray;
        readonly ShaderType corShaderType;
        readonly String name;
        readonly Microsoft.Xna.Framework.Graphics.BasicEffect xnaBasicEffect;
        readonly VertexElementUsage[] requiredVertexElements;
        readonly VertexElementUsage[] optionalVertexElements;
        readonly Dictionary<VertexDeclaration, BasicEffectVertFormat> fastBasicEffectVertFormatLookup = new Dictionary<VertexDeclaration, BasicEffectVertFormat>();

        public BasicEffectShaderAdapter(
            Microsoft.Xna.Framework.Graphics.GraphicsDevice gfxDevice,
            ShaderType corShaderType
            )
        {
            this.corShaderType = corShaderType;
            this.xnaBasicEffect = new Microsoft.Xna.Framework.Graphics.BasicEffect(gfxDevice);
            this.xnaBasicEffect.DirectionalLight0.Enabled = true;
            this.xnaBasicEffect.DirectionalLight1.Enabled = true;
            this.xnaBasicEffect.DirectionalLight2.Enabled = true;

            switch (corShaderType)
            {
                case ShaderType.Unlit:
                    this.name = "Unlit";
                    this.requiredVertexElements = new VertexElementUsage[] { VertexElementUsage.Position };
                    this.optionalVertexElements = new VertexElementUsage[] { VertexElementUsage.TextureCoordinate, VertexElementUsage.Colour };
                    this.xnaBasicEffect.LightingEnabled = false;
                    this.xnaBasicEffect.PreferPerPixelLighting = false;
                    this.xnaBasicEffect.FogEnabled = false;
                    break;
                case ShaderType.VertexLit:
                    this.name = "VertexLit";
                    this.requiredVertexElements = new VertexElementUsage[] { VertexElementUsage.Position, VertexElementUsage.Normal };
                    this.optionalVertexElements = new VertexElementUsage[] { VertexElementUsage.TextureCoordinate, VertexElementUsage.Colour };
                    this.xnaBasicEffect.LightingEnabled = true;
                    this.xnaBasicEffect.PreferPerPixelLighting = false;
                    this.xnaBasicEffect.FogEnabled = true;
                    break;
                case ShaderType.PixelLit:
                    this.name = "PixelLit";
                    this.requiredVertexElements = new VertexElementUsage[] { VertexElementUsage.Position, VertexElementUsage.Normal };
                    this.optionalVertexElements = new VertexElementUsage[] { VertexElementUsage.TextureCoordinate, VertexElementUsage.Colour };
                    this.xnaBasicEffect.LightingEnabled = true;
                    this.xnaBasicEffect.PreferPerPixelLighting = true;
                    this.xnaBasicEffect.FogEnabled = true;
                    break;
                default: throw new Exception("Shader Type: " + corShaderType.ToString() + " not supported by BasicEffectShaderAdapter");
            }

            int numPasses = this.xnaBasicEffect.CurrentTechnique.Passes.Count;

            this.passArray = new IShaderPass[numPasses];

            for (int i = 0; i < numPasses; ++i)
            {
                this.passArray[i] = new ShaderPassWrapper(this.xnaBasicEffect.CurrentTechnique.Passes[i], this.AdjustBasicEffectForVertDecl, this.ApplySettings);
            }
        }

        void AdjustBasicEffectForVertDecl(VertexDeclaration vertexDeclaration)
        {
            if (!fastBasicEffectVertFormatLookup.ContainsKey(vertexDeclaration))
            {
                var elems = vertexDeclaration
               .GetVertexElements()
               .Select(x => x.VertexElementUsage)
               .ToList();

                bool col = elems.Contains(VertexElementUsage.Colour);
                bool tex = elems.Contains(VertexElementUsage.TextureCoordinate);
                bool norm = elems.Contains(VertexElementUsage.Normal);

                var mode = BasicEffectVertFormat.VertPos;

                if (norm)
                {
                    if (!col && !tex) mode = BasicEffectVertFormat.VertPosNorm;
                    else if (col && !tex) mode = BasicEffectVertFormat.VertPosNormCol;
                    else if (!col && tex) mode = BasicEffectVertFormat.VertPosNormTex;
                    else mode = BasicEffectVertFormat.VertPosNormTexCol;
                }
                else
                {
                    if (!col && !tex) mode = BasicEffectVertFormat.VertPos;
                    else if (col && !tex) mode = BasicEffectVertFormat.VertPosCol;
                    else if (!col && tex) mode = BasicEffectVertFormat.VertPosTex;
                    else mode = BasicEffectVertFormat.VertPosTexCol;
                }
                fastBasicEffectVertFormatLookup[vertexDeclaration] = mode;
            }

            var beMode = fastBasicEffectVertFormatLookup[vertexDeclaration];


            switch (beMode)
            {
                case BasicEffectVertFormat.VertPos:
                    this.xnaBasicEffect.TextureEnabled = false;
                    this.xnaBasicEffect.VertexColorEnabled = false;
                    this.xnaBasicEffect.LightingEnabled = false;
                    break;
                case BasicEffectVertFormat.VertPosTex:
                    this.xnaBasicEffect.TextureEnabled = true;
                    this.xnaBasicEffect.VertexColorEnabled = false;
                    this.xnaBasicEffect.LightingEnabled = false;
                    break;
                case BasicEffectVertFormat.VertPosCol:
                    this.xnaBasicEffect.TextureEnabled = false;
                    this.xnaBasicEffect.VertexColorEnabled = true;
                    this.xnaBasicEffect.LightingEnabled = false;
                    break;
                case BasicEffectVertFormat.VertPosTexCol:
                    this.xnaBasicEffect.TextureEnabled = true;
                    this.xnaBasicEffect.VertexColorEnabled = true;
                    this.xnaBasicEffect.LightingEnabled = false;
                    break;

                case BasicEffectVertFormat.VertPosNorm:
                    this.xnaBasicEffect.TextureEnabled = false;
                    this.xnaBasicEffect.VertexColorEnabled = false;
                    this.xnaBasicEffect.LightingEnabled = true;
                    break;
                case BasicEffectVertFormat.VertPosNormTex:
                    this.xnaBasicEffect.TextureEnabled = true;
                    this.xnaBasicEffect.VertexColorEnabled = false;
                    this.xnaBasicEffect.LightingEnabled = true;
                    break;
                case BasicEffectVertFormat.VertPosNormCol:
                    this.xnaBasicEffect.TextureEnabled = false;
                    this.xnaBasicEffect.VertexColorEnabled = true;
                    this.xnaBasicEffect.LightingEnabled = true;
                    break;
                case BasicEffectVertFormat.VertPosNormTexCol:
                    this.xnaBasicEffect.TextureEnabled = true;
                    this.xnaBasicEffect.VertexColorEnabled = true;
                    this.xnaBasicEffect.LightingEnabled = true;
                    break;
            }
        }

        HashSet<String> warningsLogged = new HashSet<string>();

        void ApplySettings()
        {
            foreach (var name in variableCache.Keys)
            {
                object obj = variableCache[name];

                if (name == "World")
                {
                    xnaBasicEffect.World = ((Matrix44)obj).ToXNA();
                }
                else if (name == "View")
                {
                    xnaBasicEffect.View = ((Matrix44)obj).ToXNA();
                }
                else if (name == "Projection")
                {
                    xnaBasicEffect.Projection = ((Matrix44)obj).ToXNA();
                }
                else if (name == "MaterialColour")
                {
                    var matCol = ((Rgba32)obj).ToXNA();
                    xnaBasicEffect.DiffuseColor = matCol.ToVector3();
                    xnaBasicEffect.Alpha = matCol.ToVector4().W;
                }
                else if (name == "AmbientLightColour")
                {
                    xnaBasicEffect.AmbientLightColor = ((Rgba32)obj).ToXNA().ToVector3();
                }
                else if (name == "EmissiveColour")
                {
                    xnaBasicEffect.EmissiveColor = ((Rgba32)obj).ToXNA().ToVector3();
                }
                else if (name == "SpecularColour")
                {
                    xnaBasicEffect.SpecularColor = ((Rgba32)obj).ToXNA().ToVector3();
                }
                else if (name == "SpecularPower")
                {
                    xnaBasicEffect.SpecularPower = (Single)obj;
                }
                else if (name == "FogEnabled")
                {
                    xnaBasicEffect.FogEnabled = (((Single)obj) == 1f) ? true : false;
                }
                else if (name == "FogStart")
                {
                    xnaBasicEffect.FogStart = (Single)obj;
                }
                else if (name == "FogEnd")
                {
                    xnaBasicEffect.FogEnd = (Single)obj;
                }
                else if (name == "FogColour")
                {
                    xnaBasicEffect.FogColor = ((Rgba32)obj).ToXNA().ToVector3();
                }
                else if (name == "DirectionalLight0Direction")
                {
                    xnaBasicEffect.DirectionalLight0.Direction = ((Vector3)obj).ToXNA();
                }
                else if (name == "DirectionalLight0DiffuseColour")
                {
                    xnaBasicEffect.DirectionalLight0.DiffuseColor = ((Rgba32)obj).ToXNA().ToVector3();
                }
                else if (name == "DirectionalLight0SpecularColour")
                {
                    xnaBasicEffect.DirectionalLight0.SpecularColor = ((Rgba32)obj).ToXNA().ToVector3();
                }
                else if (name == "DirectionalLight1Direction")
                {
                    xnaBasicEffect.DirectionalLight1.Direction = ((Vector3)obj).ToXNA();
                }
                else if (name == "DirectionalLight1DiffuseColour")
                {
                    xnaBasicEffect.DirectionalLight1.DiffuseColor = ((Rgba32)obj).ToXNA().ToVector3();
                }
                else if (name == "DirectionalLight1SpecularColour")
                {
                    xnaBasicEffect.DirectionalLight1.SpecularColor = ((Rgba32)obj).ToXNA().ToVector3();
                }
            
                else if (name == "DirectionalLight2Direction")
                {
                    xnaBasicEffect.DirectionalLight2.Direction = ((Vector3)obj).ToXNA();
                }
                else if (name == "DirectionalLight2DiffuseColour")
                {
                    xnaBasicEffect.DirectionalLight2.DiffuseColor = ((Rgba32)obj).ToXNA().ToVector3();
                }
                else if (name == "DirectionalLight2SpecularColour")
                {
                    xnaBasicEffect.DirectionalLight2.SpecularColor = ((Rgba32)obj).ToXNA().ToVector3();
                }
                else if (name == "EyePosition")
                {
                }
                else
                {
                    if (!warningsLogged.Contains(name) )
                    {
                        Console.WriteLine(string.Format("Shader {0}: Failed to set {1} to {2}", Name, name, obj));
                        warningsLogged.Add(name);
                    }
                }
            }
        }

        #region IShader


        Dictionary<string, object> variableCache = new Dictionary<string, object>();

        public void ResetVariables()
        {
            variableCache.Clear();
        }

        public void ResetSamplerTargets()
        {
            //Console.WriteLine("Not implemented: BasicEffectShaderAdapter.ResetSamplerTargets()");
        }

        public void SetVariable<T>(string name, T value)
        {
            variableCache[name] = value;
        }

        public void SetSamplerTarget(string name, Int32 textureSlot )
        {
        }

        public IShaderPass[] Passes { get { return this.passArray; } }

        public VertexElementUsage[] RequiredVertexElements { get { return this.requiredVertexElements; } }

        public VertexElementUsage[] OptionalVertexElements { get { return this.optionalVertexElements; } }

        public string Name { get { return this.name; } }

        #endregion
	}
}
