using System;
using System.IO;
using System.Numerics;
using Silk.NET.OpenGL;

namespace Utils
{
    public class Shader : IDisposable
    {
        private readonly GL gl;
        private readonly uint id;

        /// <summary>
        /// Creates a new shader program from the given vertex shader and fragment shader sources.
        /// </summary>
        /// <param name="vertexShaderSource">The vertex shader source.</param>
        /// <param name="fragmentShaderSource">The fragment shader source.</param>
        /// <param name="gl">The <see cref="GL"/> instance with which this shader will work.</param>
        /// <exception cref="InvalidOperationException"/>.Thrown when the compilation/linking of the shader failed</exception>
        public Shader(string vertexShaderSource, string fragmentShaderSource, GL gl)
        {
            this.gl = gl;

            // compile the vertex shader
            uint vertShader = gl.CreateShader(GLEnum.VertexShader);
            gl.ShaderSource(vertShader, vertexShaderSource);
            gl.CompileShader(vertShader);
            CheckShader(vertShader, "Vertex");

            // compile the fragment shader
            uint fragShader = gl.CreateShader(GLEnum.FragmentShader);
            gl.ShaderSource(fragShader, fragmentShaderSource);
            gl.CompileShader(fragShader);
            CheckShader(fragShader, "Fragment");

            id = gl.CreateProgram();
            gl.AttachShader(id, vertShader);
            gl.AttachShader(id, fragShader);
            gl.LinkProgram(id);
            CheckProgram(id);

            /// <summary>
            /// Checks for compilation status for the given shader.
            /// If it failed, an <see cref="InvalidOperationException" is thrown with the log.
            /// </summary>
            /// <param name="shader">The id of the shader that will be checked.</param>
            /// <param name="name">The name of the shader. Used for debugging.</param>            
            /// <exception cref="InvalidOperationException"/>.Thrown when the compilation of the shader failed</exception>
            void CheckShader(uint shader, string name)
            {
                int success;
                gl.GetShader(shader, GLEnum.CompileStatus, out success);
                if (success == 0)
                {
                    string log;
                    gl.GetShaderInfoLog(shader, out log);
                    throw new InvalidOperationException($"Compiling shader '{name}' failed with the following log:\n{log}");
                }
            }

            /// <summary>
            /// Checks for linking status for the given shader program.
            /// If it failed, an <see cref="InvalidOperationException" is thrown with the log.
            /// </summary>
            /// <param name="program">The id of the shader program that will be checked.</param>
            /// <exception cref="InvalidOperationException"/>.Thrown when the linking of the shader program failed</exception>
            void CheckProgram(uint program)
            {
                int success;
                gl.GetProgram(program, GLEnum.LinkStatus, out success);
                if (success == 0)
                {
                    string log;
                    gl.GetProgramInfoLog(program, out log);
                    throw new InvalidOperationException($"Linking shader program failed with the following log:\n{log}");
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="Shader"/> from the given files.
        /// </summary>
        /// <param name="vertexShaderFile">The path to the vertex shader file.</param>
        /// <param name="fragmentshaderFile">The path to the fragment shader file.</param>
        /// <param name="gl">The <see cref="GL"/> instance with which this shader will work.</param>
        /// <returns>The newly created <see cref="Shader"/>.</returns>
        /// <exception cref="InvalidOperationException"/>.Thrown when the compilation/linking of the shader failed</exception>
        public static Shader FromFiles(string vertexShaderFile, string fragmentshaderFile, GL gl)
        {
            string vertexSource = File.ReadAllText(vertexShaderFile);
            string fragmetSource = File.ReadAllText(fragmentshaderFile);

            return new Shader(vertexSource, fragmetSource, gl);
        }

        /// <summary>
        /// Sets the <see cref="Shader"/> as the currently active one.
        /// </summary>
        public void Use()
        {
            gl.UseProgram(id);
        }

        /// <summary>
        /// Sets the value of the given uniform.
        /// </summary>
        /// <param name="name">The name of the uniform.</param>
        /// <param name="value">The value to set.</param>
        public void Set(string name, bool value) => UseAnd(() => gl.Uniform1(gl.GetUniformLocation(id, name), value ? 1 : 0));

        /// <summary>
        /// Sets the value of the given uniform.2
        /// </summary>
        /// <param name="name">The name of the uniform.</param>
        /// <param name="value">The value to set.</param>
        public void Set(string name, int value) => UseAnd(() => gl.Uniform1(gl.GetUniformLocation(id, name), value));

        /// <summary>
        /// Sets the value of the given uniform.
        /// </summary>
        /// <param name="name">The name of the uniform.</param>
        /// <param name="value">The value to set.</param>
        public void Set(string name, float value) => UseAnd(() => gl.Uniform1(gl.GetUniformLocation(id, name), value));

        /// <summary>
        /// Sets the value of the given uniform.
        /// </summary>
        /// <param name="name">The name of the uniform.</param>
        /// <param name="value">The value to set.</param>
        public void Set(string name, ref Matrix4x4 value)
        {
            Use();
            unsafe
            {
                fixed (Matrix4x4* mat = &value)
                {
                    float* ptr = (float*)mat;
                    gl.UniformMatrix4(gl.GetUniformLocation(id, name), 1, false, ptr);
                }
            }
        }

        /// <summary>
        /// Sets the current shader as active and then runs the action.
        /// </summary>
        /// <param name="action">The action to run after setting the current shader as active.</param>
        private void UseAnd(Action action)
        {
            Use();
            action();
        }

        /// <summary>
        /// Deletes the shader program.
        /// </summary>
        public void Dispose()
        {
            gl.DeleteProgram(id);
        }
    }
}
