using System;
using System.Drawing;
using System.IO;
using Silk.NET.Input;
using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using StbImageSharp;
using Utils;

namespace Textures
{
    class Program
    {
        private static readonly float[] vertices =
        {
            // VERTEX            // COLOR            // TEXEL
           -0.5f, -0.5f, 0.0f,   1.0f, 0.0f, 0.0f,   0.0f, 0.0f,    // Bottom Left
            0.5f, -0.5f, 0.0f,   0.0f, 1.0f, 0.0f,   2.0f, 0.0f,    // Bottom Right
            0.5f,  0.5f, 0.0f,   0.0f, 0.0f, 1.0f,   2.0f, 2.0f,    // Top Right
           -0.5f,  0.5f, 0.0f,   1.0f, 1.0f, 0.0f,   0.0f, 2.0f,    // Top Left
        };

        private static readonly string vertexShaderSource = @"
            #version 330 core
            layout(location = 0) in vec3 aPosition;
            layout(location = 1) in vec3 aColor;
            layout(location = 2) in vec2 aTexel;

            out vec3 color;
            out vec2 texel;

            void main()
            {
                gl_Position = vec4(aPosition, 1.0);
                color = aColor;
                texel = aTexel;
            }
        ";

        private static readonly string fragmentShaderSource = @"
            #version 330 core
            in vec3 color;
            in vec2 texel;

            out vec4 oColor;

            uniform sampler2D texture1;
            uniform sampler2D texture2;
            uniform float ratio;

            void main()
            {
                oColor = mix(texture(texture1, texel / 2.0), texture(texture2, -texel), ratio) * vec4(color, 1.0);;
            }
        ";

        static void Main(string[] args)
        {
            var options = WindowOptions.Default;
            options.Title = "Textures";
            options.Size = new Size(800, 800);
            var window = Window.Create(options);

            GL gl = null;
            uint vbo = 0;
            uint vao = 0;
            uint[] textures = new uint[2];
            Shader shader = null;
            float ratio = 0.2f;

            window.Load += () =>
            {
                // register the key handler
                window.CreateInput().Keyboards.ForEach(kbd =>
                {
                    kbd.KeyDown += (keyboard, key, _) =>
                    {
                        if (key == Key.Escape)
                        {
                            window.Close();
                        }
                        else if (key == Key.Up || key == Key.Down)
                        {
                            ratio += key == Key.Up ? 0.1f : -0.1f;
                            ratio = ratio.LimitToRange(0.0f, 1.0f);
                            shader.Set(nameof(ratio), ratio);
                        }
                    };
                });

                // retrieve the OpenGL context
                gl = GL.GetApi();

                // set-up the textures
                string[] textureSources =
                {
                    "container.jpg",
                    "awesomeface.png",
                };
                var assembly = typeof(Program).Assembly;
                gl.GenTextures((uint)textures.Length, textures);
                for (int i = 0; i < textures.Length; i++)
                {
                    gl.ActiveTexture((GLEnum)((uint)GLEnum.Texture0 + i));
                    gl.BindTexture(GLEnum.Texture2D, textures[i]);

                    gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.Repeat);
                    gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.Repeat);
                    gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
                    gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);

                    ImageResult image;
                    var loader = new ImageStreamLoader();
                    using (var stream = assembly.GetManifestResourceStream($"{nameof(Textures)}.{textureSources[i]}"))
                    {
                        image = loader.Load(stream);
                    }
                    unsafe
                    {
                        fixed (void* ptr = image.Data)
                        {
                            gl.TexImage2D(GLEnum.Texture2D, 0, (int)image.SourceComp.ToEnum(), (uint)image.Width,
                                (uint)image.Height, 0, image.SourceComp.ToEnum(), GLEnum.UnsignedByte, ptr);
                        }
                    }
                    gl.GenerateMipmap(GLEnum.Texture2D);
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
                    gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 8 * sizeof(float), (void*)0);
                    gl.VertexAttribPointer(1, 3, GLEnum.Float, false, 8 * sizeof(float), (void*)(3 * sizeof(float)));
                    gl.VertexAttribPointer(2, 2, GLEnum.Float, false, 8 * sizeof(float), (void*)(6 * sizeof(float)));
                }
                gl.EnableVertexAttribArray(0);
                gl.EnableVertexAttribArray(1);
                gl.EnableVertexAttribArray(2);

                // set-up the shader
                shader = new Shader(vertexShaderSource, fragmentShaderSource, gl);
                shader.Set("texture1", 0);
                shader.Set("texture2", 1);
                shader.Set(nameof(ratio), ratio);

                // set the clear color
                gl.ClearColor(0.15f, 0.20f, 0.32f, 1.0f);
            };
            window.Render += _ =>
            {
                // clear the buffer
                gl.Clear((uint)GLEnum.ColorBufferBit);

                // render
                for (int i = 0; i < textures.Length; i++)
                {
                    gl.ActiveTexture((GLEnum)((uint)GLEnum.Texture0 + i));
                    gl.BindTexture(GLEnum.Texture2D, textures[i]);
                }
                shader.Use();
                gl.BindVertexArray(vao);
                gl.DrawArrays(GLEnum.TriangleFan, 0, 4);
            };
            window.Resize += size => gl.Viewport(size);

            window.Run();
        }
    }
}
