using System;
using System.Drawing;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using StbImageSharp;
using Utils;

namespace Transformations
{
    class Program
    {
        private static readonly float[] vertices = new float[]
        {
            // POSITION             COLOR                   TEXEL
           -0.5f, -0.5f, 0.0f,      1.0f, 0.0f, 0.0f,       0.0f, 0.0f,  // Bottom Left
            0.5f, -0.5f, 0.0f,      0.0f, 1.0f, 0.0f,       1.0f, 0.0f,  // Bottom Right
            0.5f,  0.5f, 0.0f,      0.0f, 0.0f, 1.0f,       1.0f, 1.0f,  // Top Right
           -0.5f,  0.5f, 0.0f,      1.0f, 0.0f, 1.0f,       0.0f, 1.0f,  // Top Left
        };

        private static readonly string vertexShaderSource = @"
            #version 330 core
            layout(location = 0) in vec3 aPosition;
            layout(location = 1) in vec3 aColor;
            layout(location = 2) in vec2 aTexel;

            out vec3 color;
            out vec2 texel;

            uniform mat4 transform;

            void main()
            {
                gl_Position = transform * vec4(aPosition, 1.0);
                color = aColor;
                texel = aTexel;
            }
        ";
        private static readonly string fragmentShaderSource = @"
            #version 330 core
            in vec3 color;
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
            options.Title = "Transformations";
            options.Size = new Size(800, 800);
            var window = Window.Create(options);

            GL gl = null;
            uint tex = 0;
            uint vbo = 0;
            uint vao = 0;
            Shader shader = null;
            Matrix4x4 transform = Matrix4x4.CreateTranslation(0.5f, 0.0f, 0.0f);

            window.Load += () =>
            {
                // register the key handler
                window.CreateInput().Keyboards.ForEach(kbd => kbd.KeyDown += (kbd, key, code) =>
                {
                    if (key == Key.Escape)
                    {
                        window.Close();
                    }
                });

                // retrieve the OpenGL instance
                gl = GL.GetApi();

                // set-up the texture
                tex = gl.GenTexture();
                gl.ActiveTexture(GLEnum.Texture0);
                gl.BindTexture(GLEnum.Texture2D, tex);

                int repeat = (int)GLEnum.Repeat;
                int linear = (int)GLEnum.Linear;

                gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, ref repeat);
                gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, ref repeat);
                gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, ref linear);
                gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, ref linear);

                ImageResult image = ImageUtils.LoadEmbeddedImage("wall.jpg");
                unsafe
                {
                    fixed (void* ptr = image.Data)
                    {
                        var format = image.SourceComp.ToEnum();
                        gl.TexImage2D(GLEnum.Texture2D, 0, (int)format, (uint)image.Width,
                                     (uint)image.Height, 0, format, GLEnum.UnsignedByte, ptr);
                    }
                }

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
                    gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 8 * sizeof(float), (void*)(0 * sizeof(float)));
                    gl.VertexAttribPointer(1, 3, GLEnum.Float, false, 8 * sizeof(float), (void*)(3 * sizeof(float)));
                    gl.VertexAttribPointer(2, 2, GLEnum.Float, false, 8 * sizeof(float), (void*)(6 * sizeof(float)));
                }
                gl.EnableVertexAttribArray(0);
                gl.EnableVertexAttribArray(1);
                gl.EnableVertexAttribArray(2);

                // set-up the shader
                shader = new Shader(vertexShaderSource, fragmentShaderSource, gl);
                shader.Set(nameof(tex), (int)GLEnum.Texture0);
                shader.Set(nameof(transform), ref transform);

                // set color
                gl.ClearColor(Color.LightGray);
            };
            window.Update += delta =>
            {
                transform = Matrix4x4.CreateRotationZ((float)delta) * transform;
                shader.Set(nameof(transform), ref transform);
            };
            window.Render += _ =>
            {
                // clear the buffer 
                gl.Clear((uint)GLEnum.ColorBufferBit);

                // render
                gl.ActiveTexture(GLEnum.Texture0);
                gl.BindTexture(GLEnum.Texture2D, tex);
                shader.Use();
                gl.BindVertexArray(vao);

                gl.DrawArrays(GLEnum.TriangleFan, 0, 4);
            };
            window.Resize += size => gl.Viewport(size);

            window.Run();
        }
    }
}
