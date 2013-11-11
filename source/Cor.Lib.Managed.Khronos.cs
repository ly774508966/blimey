﻿// ┌────────────────────────────────────────────────────────────────────────┐ \\
// │ Cor.Lib.Managed.Khronos                                                │ \\
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
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

#if COR_PLATFORM_MANAGED_XIOS
using OpenTK.Graphics.ES20;
#elif COR_PLATFORM_MANAGED_MONOMAC
using MonoMac.OpenGL;
#else
    Platform not supported.
#endif

namespace Sungiant.Cor.Lib.Managed.Khronos
{
    public static class ErrorHandler
    {
        [Conditional("DEBUG")]
        public static void Check()
        {
            var ec = GL.GetError();

            if (ec != ErrorCode.NoError)
            {
                throw new Exception( ec.ToString());
            }
        }
    }

    /// <summary>
    /// Static class to help with horrible shader system.
    /// </summary>
    public static class ShaderUtils
    {
        public class ShaderUniform
        {
            public Int32 Index { get; set; }
            public String Name { get; set; }
            public ActiveUniformType Type { get; set; }
        }

        public class ShaderAttribute
        {
            public Int32 Index { get; set; }
            public String Name { get; set; }
            public ActiveAttribType Type { get; set; }
        }
        
        public static Int32 CreateShaderProgram()
        {
            // Create shader program.
            Int32 programHandle = GL.CreateProgram ();

            if( programHandle == 0 )
                throw new Exception("Failed to create shader program");

            ErrorHandler.Check();

            return programHandle;
        }

        public static Int32 CreateVertexShader(string path)
        {
            Int32 vertShaderHandle;

            if( Path.GetExtension(path) != ".vsh" )
            {
                throw new Exception("Vertex shader [" + path + "] should end with .vsh");
            }

            if( !File.Exists(path))
            {
                throw new Exception("Vertex shader at [" + path + "] does not exist.");
            }

            ShaderUtils.CompileShader (
                ShaderType.VertexShader, 
                path, 
                out vertShaderHandle );

            if( vertShaderHandle == 0 )
                throw new Exception("Failed to compile vertex shader program");

            return vertShaderHandle;
        }

        public static Int32 CreateFragmentShader(string path)
        {
            Int32 fragShaderHandle;

            if( Path.GetExtension(path) != ".fsh" )
            {
                throw new Exception("Fragement shader [" + path + "] should end with .fsh");
            }

            if( !File.Exists(path))
            {
                throw new Exception("Fragement shader at [" + path + "] does not exist.");
            }

            ShaderUtils.CompileShader (
                ShaderType.FragmentShader,
                path,
                out fragShaderHandle );

            if( fragShaderHandle == 0 )
                throw new Exception("Failed to compile fragment shader program");


            return fragShaderHandle;
        }

        public static void AttachShader(
            Int32 programHandle,
            Int32 shaderHandle)
        {
            if (shaderHandle != 0)
            {
                // Attach vertex shader to program.
                GL.AttachShader (programHandle, shaderHandle);
                ErrorHandler.Check();
            }
        }

        public static void DetachShader(
            Int32 programHandle,
            Int32 shaderHandle )
        {
            if (shaderHandle != 0)
            {
                GL.DetachShader (programHandle, shaderHandle);
                ErrorHandler.Check();
            }
        }

        public static void DeleteShader(
            Int32 programHandle,
            Int32 shaderHandle )
        {
            if (shaderHandle != 0)
            {
                GL.DeleteShader (shaderHandle);
                shaderHandle = 0;
                ErrorHandler.Check();
            }
        }
        
        public static void DestroyShaderProgram (Int32 programHandle)
        {
            if (programHandle != 0)
            {
#if COR_PLATFORM_MANAGED_XIOS
                GL.DeleteProgram (programHandle);
#elif COR_PLATFORM_MANAGED_MONOMAC
                GL.DeleteProgram (1, new int[]{ programHandle } );
#endif

                programHandle = 0;
                ErrorHandler.Check();
            }
        }

        public static void CompileShader (
            ShaderType type,
            String file,
            out Int32 shaderHandle )
        {
            String src = string.Empty;

            try
            {
                // Get the data from the text file
                src = System.IO.File.ReadAllText (file);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                shaderHandle = 0;
                return;
            }

            // Create an empty vertex shader object
            shaderHandle = GL.CreateShader (type);

            ErrorHandler.Check();

            // Replace the source code in the vertex shader object
#if COR_PLATFORM_MANAGED_XIOS
            GL.ShaderSource (
                shaderHandle,
                1,
                new String[] { src },
                (Int32[]) null );
#elif COR_PLATFORM_MANAGED_MONOMAC
            GL.ShaderSource (
                shaderHandle,
                src);
#endif

            ErrorHandler.Check();

            GL.CompileShader (shaderHandle);

            ErrorHandler.Check();
            
#if DEBUG
            Int32 logLength = 0;
            GL.GetShader (
                shaderHandle,
                ShaderParameter.InfoLogLength,
                out logLength);

            ErrorHandler.Check();
            var infoLog = new System.Text.StringBuilder(logLength);

            if (logLength > 0)
            {
                int temp = 0;
                GL.GetShaderInfoLog (
                    shaderHandle,
                    logLength,
                    out temp,
                    infoLog );

                string log = infoLog.ToString();

                Console.WriteLine(file);
                Console.WriteLine (log);
                Console.WriteLine(type);
            }
#endif
            Int32 status = 0;

            GL.GetShader (
                shaderHandle,
                ShaderParameter.CompileStatus,
                out status );

            ErrorHandler.Check();

            if (status == 0)
            {
                GL.DeleteShader (shaderHandle);
                throw new Exception ("Failed to compile " + type.ToString());
            }
        }
        
        public static List<ShaderUniform> GetUniforms (Int32 prog)
        {
            
            int numActiveUniforms = 0;
            
            var result = new List<ShaderUniform>();

            GL.GetProgram(prog, ProgramParameter.ActiveUniforms, out numActiveUniforms);
            ErrorHandler.Check();

            for(int i = 0; i < numActiveUniforms; ++i)
            {
                var sb = new System.Text.StringBuilder ();
                
                int buffSize = 0;
                int length = 0;
                int size = 0;
                ActiveUniformType type;

                GL.GetActiveUniform(
                    prog,
                    i,
                    64,
                    out length,
                    out size,
                    out type,
                    sb);
                ErrorHandler.Check();
                
                result.Add(
                    new ShaderUniform()
                    {
                    Index = i,
                    Name = sb.ToString(),
                    Type = type
                    }
                );
            }
            
            return result;
        }

        public static List<ShaderAttribute> GetAttributes (Int32 prog)
        {
            int numActiveAttributes = 0;
            
            var result = new List<ShaderAttribute>();
            
            // gets the number of active vertex attributes
            GL.GetProgram(prog, ProgramParameter.ActiveAttributes, out numActiveAttributes);
            ErrorHandler.Check();

            for(int i = 0; i < numActiveAttributes; ++i)
            {
                var sb = new System.Text.StringBuilder ();

                int buffSize = 0;
                int length = 0;
                int size = 0;
                ActiveAttribType type;
                GL.GetActiveAttrib(
                    prog,
                    i,
                    64,
                    out length,
                    out size,
                    out type,
                    sb);
                ErrorHandler.Check();
                    
                result.Add(
                    new ShaderAttribute()
                    {
                        Index = i,
                        Name = sb.ToString(),
                        Type = type
                    }
                );
            }
            
            return result;
        }
        
        
        public static bool LinkProgram (Int32 prog)
        {
            bool retVal = true;

            GL.LinkProgram (prog);

            ErrorHandler.Check();
            
#if DEBUG
            Int32 logLength = 0;

            GL.GetProgram (
                prog,
                ProgramParameter.InfoLogLength,
                out logLength );

            ErrorHandler.Check();

            if (logLength > 0)
            {
                retVal = false;

                /*
                var infoLog = new System.Text.StringBuilder ();

                GL.GetProgramInfoLog (
                    prog,
                    logLength,
                    out logLength,
                    infoLog );
                */
                var infoLog = string.Empty;
                GL.GetProgramInfoLog(prog, out infoLog);


                ErrorHandler.Check();

                Console.WriteLine (string.Format("[Cor.Resources] Program link log:\n{0}", infoLog));
            }
#endif
            Int32 status = 0;

            GL.GetProgram (
                prog,
                ProgramParameter.LinkStatus,
                out status );

            ErrorHandler.Check();

            if (status == 0)
            {
                throw new Exception(String.Format("Failed to link program: {0:x}", prog));
            }

            return retVal;

        }

        public static void ValidateProgram (Int32 programHandle)
        {
            GL.ValidateProgram (programHandle);

            ErrorHandler.Check();
            
            Int32 logLength = 0;

            GL.GetProgram (
                programHandle,
                ProgramParameter.InfoLogLength,
                out logLength );

            ErrorHandler.Check();

            if (logLength > 0)
            {
                var infoLog = new System.Text.StringBuilder ();

                GL.GetProgramInfoLog (
                    programHandle,
                    logLength,
                    out logLength, infoLog );

                ErrorHandler.Check();

                Console.WriteLine (string.Format("[Cor.Resources] Program validate log:\n{0}", infoLog));
            }
            
            Int32 status = 0;

            GL.GetProgram (
                programHandle, ProgramParameter.LinkStatus,
                out status );

            ErrorHandler.Check();

            if (status == 0)
            {
                throw new Exception (String.Format("Failed to validate program {0:x}", programHandle));
            }
        }
    }

}
