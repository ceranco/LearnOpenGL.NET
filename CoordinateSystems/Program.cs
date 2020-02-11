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
            // Position             // Texel
           -0.5f, -0.5f, 0.0f,      0.0f, 0.0f,
            0.5f, -0.5f, 0.0f,      1.0f, 0.0f,
            0.5f,  0.5f, 0.0f,      1.0f, 1.0f,
           -0.5f,  0.5f, 0.0f,      0.0f, 1.0f,
        };
        private static readonly string vertexShaderSource = @"
            #version 330 core
            layout(location = 0) in vec3 aPosition;
            layout(location = 1) in vec2 aTexel;

            out vec2 texel;

            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;

            void main()
            {
                gl_Position = projection * view * model * vec4(aPosition, 1.0);
                texel = aTexel;
            }
        ";
        private static readonly string fragmentShaderSource = @"
            #version 330 core
            in vec2 texel;
            out vec4 fColor;

            uniform sampler2D tex;

            void main()
            {
                fColor = texture(tex, texel);
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
            uint tex = 0;
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

                // set-up the texture
                tex = gl.GenTexture();
                gl.ActiveTexture(GLEnum.Texture0);

                gl.BindTexture(GLEnum.Texture2D, tex);
                gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.Repeat);
                gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.Repeat);
                gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
                gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);

                var image = ImageUtils.LoadEmbeddedImage("container.jpg");
                unsafe
                {
                    fixed (void* ptr = image.Data)
                    {
                        gl.TexImage2D(GLEnum.Texture2D, 0, (int)image.SourceComp.ToEnum(), (uint)image.Width,
                        (uint)image.Height, 0, image.SourceComp.ToEnum(), GLEnum.UnsignedByte, ptr);
                    }
                }
                gl.GenerateMipmap(GLEnum.Texture2D);

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
                    const uint stride = 5 * sizeof(float);
                    gl.VertexAttribPointer(0, 3, GLEnum.Float, false, stride, (void*)0);
                    gl.VertexAttribPointer(1, 2, GLEnum.Float, false, stride, (void*)(3 * sizeof(float)));
                }
                gl.EnableVertexAttribArray(0);
                gl.EnableVertexAttribArray(1);

                // set-up the shader
                shader = new Shader(vertexShaderSource, fragmentShaderSource, gl);
                shader.Set(nameof(tex), 0);
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
                gl.ActiveTexture(GLEnum.Texture0);
                gl.BindTexture(GLEnum.Texture2D, tex);
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
