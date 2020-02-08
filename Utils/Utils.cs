using System.Reflection;
using StbImageSharp;

namespace Utils
{
    public static class ImageUtils
    {
        /// <summary>
        /// Loads the embedded image from the assembly in which this function is called.
        /// </summary>
        /// <param name="path">The path to the embedded image (relative to the .csproj).</param>
        /// <returns>The loaded image.</returns>
        public static ImageResult LoadEmbeddedImage(string path)
        {
            var assembly = Assembly.GetCallingAssembly();
            ImageResult image;
            using (var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{path}"))
            {
                image = new ImageStreamLoader().Load(stream);
            }
            return image;
        }
    }
}