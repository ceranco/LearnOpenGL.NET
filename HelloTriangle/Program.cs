using System;
using System.Diagnostics;
using System.Drawing;
using Silk.NET.Input;
using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using Utils;

namespace HelloTriangle
{
    static class Program
    {
        private static readonly float[] vertices =
        {
            // VERTICES
            -0.5f, -0.5f, 0.0f, // Bottom Left
             0.5f, -0.5f, 0.0f, // Bottom Right
             0.0f,  0.5f, 0.0f, // Top
            // COLORS
            1.0f, 0.0f, 0.0f, // Bottom Left
            0.0f, 1.0f, 0.0f, // Bottom Right
            0.0f, 0.0f, 1.0f  // Top
        };

        private const string vertexShaderSource = @"
            #version 330 core
            layout (location = 0) in vec3 aPosition;
            layout (location = 1) in vec3 aColor;
            out vec3 color;
            uniform float offset;

            void main()
            {
                gl_Position = vec4(aPosition.x + offset, -aPosition.y, aPosition.z, 1.0);
                color = aColor;
            }
        ";
        private const string fragmentShaderSource = @"
            #version 330 core
            in vec3 color;
            out vec4 fragColor;
            void main()
            {
                fragColor = vec4(color, 1.0);
            }
        ";

        internal static void Main(string[] args)
        {
            var options = WindowOptions.Default;
            options.Title = "Hello Triangle!";
            options.Size = new Size(800, 800);
            var window = Window.Create(options);

            GL gl = null;
            uint vbo = 0;
            uint vao = 0;
            Shader shader = null;
            Stopwatch stopwatch = new Stopwatch();

            window.Load += () =>
            {
                // register the key handler
                window.CreateInput().Keyboards.ForEach(kbd => kbd.KeyDown += OnKeyDown);

                // retreive the gl context
                gl = GL.GetApi();

                // set-up the VBO
                vbo = gl.GenBuffer();
                gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
                gl.BufferData(GLEnum.ArrayBuffer, (uint)vertices.Length * sizeof(float), new Span<float>(vertices), GLEnum.StaticDraw);

                // set-up the VAO
                vao = gl.GenVertexArray();
                gl.BindVertexArray(vao);

                unsafe
                {
                    gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 3 * sizeof(float), (void*)0);
                    gl.VertexAttribPointer(1, 3, GLEnum.Float, false, 3 * sizeof(float), (void*)(3 * 3 * sizeof(float)));
                }
                gl.EnableVertexAttribArray(0);
                gl.EnableVertexAttribArray(1);

                // set-up the shader program
                shader = new Shader(vertexShaderSource, fragmentShaderSource, gl);

                // set the clear color
                gl.ClearColor(0.3f, 0.2f, 0.15f, 1.0f);

                // start the stopwatch
                stopwatch.Start();
            };
            window.Update += (_) =>
            {
                float offset = MathF.Sin(stopwatch.ElapsedMilliseconds * 0.075f * MathF.PI / 180) * 0.5f;
                shader.Set("offset", offset);
            };
            window.Render += (_) =>
            {
                shader.Use();
                gl.BindVertexArray(vao);

                gl.Clear((uint)GLEnum.ColorBufferBit);
                gl.DrawArrays(GLEnum.Triangles, 0, 3);
            };
            window.Resize += (size) =>
            {
                gl.Viewport(size);
            };

            window.Run();

            /// <summary>
            /// Handler for key-down events.
            /// </summary>
            /// <param name="keyboard">The <see cref="IKeyboard"/> from which the event originated.</param>
            /// <param name="key">The <see cref="Key"/> that was pressed.</param>
            /// <param name="code">The keycode of the <see cref="Key"/>.</param>
            void OnKeyDown(IKeyboard keyboard, Key key, int code)
            {
                if (key == Key.Escape)
                {
                    window.Close();
                }
            }
        }
    }
}
