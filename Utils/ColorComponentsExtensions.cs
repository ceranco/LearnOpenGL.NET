using System;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace Utils
{
    public static class ColorComponentsExtensions
    {
        /// <summary>
        /// Converts the given <see cref="ColorComponents"/> to their respective <see cref="GLEnum"/>.
        /// </summary>
        /// <param name="components">The <see cref="ColorComponents"/> to convert.</param>
        /// <returns>The respective <see cref="GLEnum"/>.</returns>
        public static GLEnum ToEnum(this ColorComponents components)
        {
            switch (components)
            {
                case ColorComponents.RedGreenBlue:
                    return GLEnum.Rgb;
                case ColorComponents.RedGreenBlueAlpha:
                    return GLEnum.Rgba;
                default:
                    throw new ArgumentException("Can't convert the given components.", nameof(components));
            }
        }
    }
}