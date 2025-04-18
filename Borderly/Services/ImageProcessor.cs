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
                using var image = Image.Load(inputPath)
                    ?? throw new InvalidOperationException($"Failed to load image from {inputPath}");

                var resizeWidth = 0;

                if (!string.IsNullOrWhiteSpace(profile.ResizeWidth))
                {
                    var resizeInput = profile.ResizeWidth.Trim().ToLowerInvariant();
                    if (resizeInput.EndsWith('%'))
                    {
                        if (int.TryParse(resizeInput[..^1].Trim(), out var percent))
                        {
                            resizeWidth = image.Width * percent / 100;
                        }
                        else
                        {
                            resizeWidth = 0;
                        }
                    }
                    else
                    {
                        if (resizeInput.EndsWith("px"))
                        {
                            resizeInput = resizeInput[..^2].Trim();
                        }

                        if (!int.TryParse(resizeInput, out resizeWidth))
                        {
                            resizeWidth = 0;
                        }
                    }
                }

                var resizeHeight = 0;

                if (!string.IsNullOrWhiteSpace(profile.ResizeHeight))
                {
                    var resizeInput = profile.ResizeHeight.Trim().ToLowerInvariant();
                    if (resizeInput.EndsWith('%'))
                    {
                        if (int.TryParse(resizeInput[..^1].Trim(), out var percent))
                        {
                            resizeHeight = image.Height * percent / 100;
                        }
                        else
                        {
                            resizeHeight = 0;
                        }
                    }
                    else
                    {
                        if (resizeInput.EndsWith("px"))
                        {
                            resizeInput = resizeInput[..^2].Trim();
                        }

                        if (!int.TryParse(resizeInput, out resizeHeight))
                        {
                            resizeHeight = 0;
                        }
                    }
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

                var borderColourHex = string.IsNullOrWhiteSpace(profile.BorderColour) ? "#ffffff" : profile.BorderColour;
                var borderColour = Color.ParseHex(borderColourHex);

                var borderInput = profile.BorderWidth.Trim().ToLowerInvariant();
                int borderWidth;

                if (borderInput.EndsWith('%'))
                {
                    if (int.TryParse(borderInput[..^1].Trim(), out var percent))
                    {
                        borderWidth = image.Width * percent / 100;
                    }
                    else
                    {
                        borderWidth = 0;
                    }
                }
                else
                {
                    if (borderInput.EndsWith("px"))
                    {
                        borderInput = borderInput[..^2].Trim();
                    }

                    if (!int.TryParse(borderInput, out borderWidth))
                    {
                        borderWidth = 0;
                    }
                }

                image.Mutate(x => x.Pad(image.Width + borderWidth * 2, image.Height + borderWidth * 2, borderColour));

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