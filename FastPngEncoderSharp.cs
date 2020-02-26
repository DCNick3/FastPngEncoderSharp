using System;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

// ReSharper disable InconsistentNaming

namespace FastPngEncoderSharp
{
    public class FastPngEncoder
    {

        public static unsafe void WritePngToFile<T>(string filename, Image<T> image) where T : unmanaged, IPixel<T>
        {
            if (typeof(Rgba32).IsAssignableFrom(typeof(T)))
            {
                sbyte* errorMessage = null;
                int result;
                fixed (void* data = image.GetPixelSpan())
                {
                    result = PngInteropHelper.write_png_to_file(filename, (uint) image.Width, (uint) image.Height, 8,
                        ColorType.RgbAlpha, InterlaceType.None, CompressionType.Default, FilterType.Default, TransformType.Identity,
                        (byte*) data, (uint)image.Width * 4, &errorMessage);
                }

                var errorMessageString = errorMessage == null ? null : new string(errorMessage);
                
                switch (result)
                {
                    case 0:
                        return;
                    case -1:
                        throw new Exception($"Opening file failed: {errorMessageString}.");
                    case -2:
                    case -3:
                    case -5:
                        throw new Exception("Allocation failed");
                    case -4:
                        throw new Exception($"libpng error: {errorMessageString}");
                    default:
                        throw new Exception("Unknown error");
                }
            }

            throw new NotImplementedException();
        }
        
        public static unsafe class PngInteropHelper
        {
            private const string LibraryName = "png_interop_helper";

            [DllImport(LibraryName)]
            public static extern int write_png_to_file(string filename, uint width, uint height, uint bit_width,
                ColorType color_type, InterlaceType interlace_type, CompressionType compression_type,
                FilterType filter_type, TransformType transform_type,
                byte* image_data, uint row_size, sbyte** perror_message);
        }
    }
}
