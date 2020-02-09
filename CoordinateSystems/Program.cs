using System;
using System.Drawing;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using Utils;

namespace CoordinateSystems
{
    internal static class Program
    {
        private static readonly float[] vertices =
        {
            // Position             // Color
           -0.5f, -0.5f, 0.0f,      1.0f, 0.0f, 0.0f,
            0.5f, -0.5f, 0.0f,      0.0f, 1.0f, 0.0f,
            0.5f,  0.5f, 0.0f,      0.0f, 0.0f, 1.0f,
           -0.5f,  0.5f, 0.0f,      1.0f, 1.0f, 1.0f,
        };
        private static readonly string vertexShaderSource = @"
            #version 330 core
            layout(location = 0) in vec3 aPosition;
            layout(location = 1) in vec3 aColor;

            out vec3 color;

            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;

            void main()
            {
                gl_Position = projection * view * model * vec4(aPosition, 1.0);
                color = aColor;
            }
        ";
        private static readonly string fragmentShaderSource = @"
            #version 330 core
            in vec3 color;
            out vec4 fColor;

            void main()
            {
                fColor = vec4(color, 1.0);
            }
        ";

        private static void Main(string[] args)
        {
            var options = WindowOptions.Default;
            options.Title = "Coordinate Systems";
            options.Size = new Size(800, 800);
            var window = Window.Create(options);

            GL gl = null;
            uint vbo = 0;
            uint vao = 0;
            Shader shader = null;
            Matrix4x4 model = Matrix4x4.Identity;
            Matrix4x4 view = Matrix4x4.Identity;
            Matrix4x4 projection = Matrix4x4.Identity;

            window.Load += () =>
            {
                // register key-down handler
                window.CreateInput().Keyboards.ForEach(kbd => kbd.KeyDown += (kbd, key, code) =>
                {
                    if (key == Key.Escape)
                    {
                        window.Close();
                    }
                });

                // retrieve the gl instance
                gl = GL.GetApi();

                // set-up the vbo
                vbo = gl.GenBuffer();
                gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
                unsafe
                {
                    fixed (void* ptr = vertices)
                    {
                        gl.BufferData(GLEnum.ArrayBuffer, (uint)vertices.Length * sizeof(float), ptr, GLEnum.StaticDraw);
                    }
                }

                // set-up the vao
                vao = gl.GenVertexArray();
                gl.BindVertexArray(vao);
                unsafe
                {
                    const uint stride = 6 * sizeof(float);
                    gl.VertexAttribPointer(0, 3, GLEnum.Float, false, stride, (void*)0);
                    gl.VertexAttribPointer(1, 3, GLEnum.Float, false, stride, (void*)(3 * sizeof(float)));
                }
                gl.EnableVertexAttribArray(0);
                gl.EnableVertexAttribArray(1);

                // set-up the shader
                shader = new Shader(vertexShaderSource, fragmentShaderSource, gl);
                model = Matrix4x4.CreateRotationX(-55.0f.ToRadians());
                view = Matrix4x4.CreateTranslation(0, 0, -3.0f);
                projection = Matrix4x4.CreatePerspectiveFieldOfView(45.0f.ToRadians(), 1.0f, 0.1f, 100.0f);

                // set the clear color
                gl.ClearColor(Color.DarkGoldenrod);
            };
            window.Update += _ =>
            {
                shader.Set(nameof(model), ref model);
                shader.Set(nameof(view), ref view);
                shader.Set(nameof(projection), ref projection);
            };
            window.Render += _ =>
            {
                // clear the buffer
                gl.Clear((uint)GLEnum.ColorBufferBit);

                // draw
                shader.Use();
                gl.BindVertexArray(vao);
                gl.DrawArrays(GLEnum.TriangleFan, 0, 4);
            };
            window.Resize += size =>
            {
                gl.Viewport(size);
                projection = Matrix4x4.CreatePerspectiveFieldOfView(45.0f.ToRadians(), (float)size.Width / (float)size.Height, 0.1f, 100.0f);
            };

            window.Run();
        }
    }
}
