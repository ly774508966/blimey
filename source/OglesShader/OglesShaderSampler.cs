using System;

namespace Sungiant.Cor.MonoTouchRuntime
{
	public class OglesShaderSampler
	{
		int ProgramHandle { get; set; }
		internal int UniformLocation { get; private set; }

		public String NiceName { get; set; }
		public String Name { get; set; }

		public OglesShaderSampler(
			int programHandle, ShaderUtils.ShaderUniform uniform )
		{
			this.ProgramHandle = programHandle;

			int uniformLocation = OpenTK.Graphics.ES20.GL.GetUniformLocation(programHandle, uniform.Name);

			OpenTKHelper.CheckError();


			this.UniformLocation = uniformLocation;
			this.Name = uniform.Name;
		}

		internal void RegisterExtraInfo(ShaderSamplerDefinition definition)
		{
			NiceName = definition.NiceName;
		}

		public void SetSlot(Int32 slot)
		{
			// set the sampler texture unit to 0
			OpenTK.Graphics.ES20.GL.Uniform1( this.UniformLocation, slot );
			OpenTKHelper.CheckError();
		}

	}
}

