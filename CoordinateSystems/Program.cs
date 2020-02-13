using System;
using System.Diagnostics;
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
           -0.5f, -0.5f, -0.5f,     0.0f, 0.0f,
            0.5f, -0.5f, -0.5f,     1.0f, 0.0f,
            0.5f,  0.5f, -0.5f,     1.0f, 1.0f,
            0.5f,  0.5f, -0.5f,     1.0f, 1.0f,
           -0.5f,  0.5f, -0.5f,     0.0f, 1.0f,
           -0.5f, -0.5f, -0.5f,     0.0f, 0.0f,

           -0.5f, -0.5f,  0.5f,     0.0f, 0.0f,
            0.5f, -0.5f,  0.5f,     1.0f, 0.0f,
            0.5f,  0.5f,  0.5f,     1.0f, 1.0f,
            0.5f,  0.5f,  0.5f,     1.0f, 1.0f,
           -0.5f,  0.5f,  0.5f,     0.0f, 1.0f,
           -0.5f, -0.5f,  0.5f,     0.0f, 0.0f,

           -0.5f,  0.5f,  0.5f,     1.0f, 0.0f,
           -0.5f,  0.5f, -0.5f,     1.0f, 1.0f,
           -0.5f, -0.5f, -0.5f,     0.0f, 1.0f,
           -0.5f, -0.5f, -0.5f,     0.0f, 1.0f,
           -0.5f, -0.5f,  0.5f,     0.0f, 0.0f,
           -0.5f,  0.5f,  0.5f,     1.0f, 0.0f,

            0.5f,  0.5f,  0.5f,     1.0f, 0.0f,
            0.5f,  0.5f, -0.5f,     1.0f, 1.0f,
            0.5f, -0.5f, -0.5f,     0.0f, 1.0f,
            0.5f, -0.5f, -0.5f,     0.0f, 1.0f,
            0.5f, -0.5f,  0.5f,     0.0f, 0.0f,
            0.5f,  0.5f,  0.5f,     1.0f, 0.0f,

           -0.5f, -0.5f, -0.5f,     0.0f, 1.0f,
            0.5f, -0.5f, -0.5f,     1.0f, 1.0f,
            0.5f, -0.5f,  0.5f,     1.0f, 0.0f,
            0.5f, -0.5f,  0.5f,     1.0f, 0.0f,
           -0.5f, -0.5f,  0.5f,     0.0f, 0.0f,
           -0.5f, -0.5f, -0.5f,     0.0f, 1.0f,

           -0.5f,  0.5f, -0.5f,     0.0f, 1.0f,
            0.5f,  0.5f, -0.5f,     1.0f, 1.0f,
            0.5f,  0.5f,  0.5f,     1.0f, 0.0f,
            0.5f,  0.5f,  0.5f,     1.0f, 0.0f,
           -0.5f,  0.5f,  0.5f,     0.0f, 0.0f,
           -0.5f,  0.5f, -0.5f,     0.0f, 1.0f
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

            uniform sampler2D texture1;
            uniform sampler2D texture2;

            void main()
            {
                fColor = mix(texture(texture1, texel), texture(texture2, vec2(texel.x, -texel.y)), 0.2);
            }
        ";
        private static readonly string[] texture_names =
        {
            "container.jpg",
            "awesomeface.png"
        };

        private static void Main(string[] args)
        {
            var options = WindowOptions.Default;
            options.Title = "Coordinate Systems";
            options.Size = new Size(800, 800);
            var window = Window.Create(options);

            GL gl = null;
            uint vbo = 0;
            uint vao = 0;
            uint[] textures = new uint[texture_names.Length];
            Shader shader = null;
            Matrix4x4 model = Matrix4x4.Identity;
            Matrix4x4 view = Matrix4x4.Identity;
            Matrix4x4 projection = Matrix4x4.Identity;
            var stopwatch = new Stopwatch();

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

                // set-up the textures
                gl.GenTextures((uint)textures.Length, textures);

                for (int i = 0; i < textures.Length; i++)
                {
                    var texture = textures[i];
                    gl.ActiveTexture((GLEnum)((int)GLEnum.Texture0 + i));
                    gl.BindTexture(GLEnum.Texture2D, texture);

                    gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.Repeat);
                    gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.Repeat);
                    gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
                    gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);

                    var image = ImageUtils.LoadEmbeddedImage(texture_names[i]);
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
                    const uint stride = 5 * sizeof(float);
                    gl.VertexAttribPointer(0, 3, GLEnum.Float, false, stride, (void*)0);
                    gl.VertexAttribPointer(1, 2, GLEnum.Float, false, stride, (void*)(3 * sizeof(float)));
                }
                gl.EnableVertexAttribArray(0);
                gl.EnableVertexAttribArray(1);

                // set-up the shader
                shader = new Shader(vertexShaderSource, fragmentShaderSource, gl);
                for (int i = 0; i < textures.Length; i++)
                {
                    shader.Set($"texture{i + 1}", i);
                }
                // model = Matrix4x4.CreateRotationX(-55.0f.ToRadians());
                view = Matrix4x4.CreateTranslation(0, 0, -3.0f);
                projection = Matrix4x4.CreatePerspectiveFieldOfView(45.0f.ToRadians(), 1.0f, 0.1f, 100.0f);

                // setup gl stuff
                gl.ClearColor(Color.DarkGoldenrod);
                gl.Enable(GLEnum.DepthTest);

                // start the stopwatch
                stopwatch.Start();
            };
            window.Update += _ =>
            {
                var axis = Vector3.Normalize(new Vector3(0.5f, 1.0f, 0.0f));
                var angle = (float)stopwatch.Elapsed.TotalSeconds * 35.0f.ToRadians();
                var quat = Quaternion.CreateFromAxisAngle(axis, angle);
                model = Matrix4x4.CreateFromQuaternion(quat);

                shader.Set(nameof(model), ref model);
                shader.Set(nameof(view), ref view);
                shader.Set(nameof(projection), ref projection);
            };
            window.Render += _ =>
            {
                // clear the buffer
                gl.Clear((uint)GLEnum.ColorBufferBit | (uint)GLEnum.DepthBufferBit);

                // draw
                for (int i = 0; i < textures.Length; i++)
                {
                    var texture = textures[i];
                    gl.ActiveTexture((GLEnum)((int)GLEnum.Texture0 + i));
                    gl.BindTexture(GLEnum.Texture2D, texture);
                }
                shader.Use();
                gl.BindVertexArray(vao);
                gl.DrawArrays(GLEnum.Triangles, 0, 36);
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
