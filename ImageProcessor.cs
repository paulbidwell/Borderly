using Borderly.Config;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Processing;

namespace Borderly.Services
{
    public class ImageProcessor(Settings settings) : IImageProcessor
    {
        public void Process(string inputPath, Profile profile)
        {
            try
            {
                using var image = Image.Load(inputPath) ?? throw new InvalidOperationException($"Failed to load image from {inputPath}");

                var resizeWidth = profile.ResizeWidth ?? 0;
                var resizeHeight = profile.ResizeHeight ?? 0;

                if (profile.ResizeWidthPercentage.HasValue)
                {
                    resizeWidth = image.Width * profile.ResizeWidthPercentage.Value / 100;
                }

                if (profile.ResizeHeightPercentage.HasValue)
                {
                    resizeHeight = image.Height * profile.ResizeHeightPercentage.Value / 100;
                }

                if (resizeWidth > 0 || resizeHeight > 0)
                {
                    var resizeOptions = new ResizeOptions
                    {
                        Size = new Size(
                            resizeWidth > 0 ? resizeWidth : 0,
                            resizeHeight > 0 ? resizeHeight : 0),
                        Mode = ResizeMode.Max
                    };

                    image.Mutate(x => x.Resize(resizeOptions));
                }

                var borderWidth = profile.BorderWidth;
                image.Mutate(x => x.Pad(image.Width + borderWidth * 2, image.Height + borderWidth * 2, Color.White));

                var filename = Path.GetFileNameWithoutExtension(inputPath);
                var extension = Path.GetExtension(inputPath).ToLowerInvariant();
                var folder = Path.Combine(settings.OutputDirectory, profile.Name);

                Directory.CreateDirectory(folder);

                var path = Path.Combine(folder, $"{filename}_{profile.Name}{extension}");

                switch (extension)
                {
                    case ".jpg":
                    case ".jpeg":
                        var jpegEncoder = new JpegEncoder { Quality = profile.Quality };
                        image.Save(path, jpegEncoder);
                        break;
                    case ".png":
                        var pngEncoder = new PngEncoder();
                        image.Save(path, pngEncoder);
                        break;
                    case ".tif":
                    case ".tiff":
                        var tiffEncoder = new TiffEncoder();
                        image.Save(path, tiffEncoder);
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported image format: {extension}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
