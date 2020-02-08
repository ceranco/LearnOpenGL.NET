using System;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;

namespace HelloWindow
{
    static class Program
    {
        static void Main(string[] args)
        {
            var options = WindowOptions.Default;
            options.Title = "Hello Window!";

            var window = Window.Create(options);

            window.Run();
        }
    }
}
