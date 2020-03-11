using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

// ReSharper disable InconsistentNaming

namespace FastPngEncoderSharp
{
    public class FastPngEncoder
    {
        public static void WritePngToFile<T>(string filename, Image<T> image, (int, int) size) where T : unmanaged, IPixel<T>
        {
            WritePngToFile(filename, image, new Rectangle(0, 0, size.Item1, size.Item2));
        }
        
        
        public static void WritePngToFile<T>(string filename, Image<T> image) where T : unmanaged, IPixel<T>
        {
            WritePngToFile(filename, image, new Rectangle(0, 0, image.Width, image.Height));
        }
        
        public static unsafe void WritePngToFile<T>(string filename, Image<T> image,
            Rectangle region) where T : unmanaged, IPixel<T>
        {
            ColorType colorType;
            uint bitWidth;
            
            T test = default;
            switch (test)
            {
                case Rgba32 _:
                    bitWidth = 8;
                    colorType = ColorType.RgbAlpha;
                    break;
                case Rgb24 _:
                    bitWidth = 8;
                    colorType = ColorType.Rgb;
                    break;
                case Gray8 _:
                    bitWidth = 8;
                    colorType = ColorType.Gray;
                    break;
                case Gray16 _:
                    bitWidth = 16;
                    colorType = ColorType.Gray;
                    break;
                default:
                    throw new NotImplementedException("The pixel format is not implemented");
            }
            
            sbyte* errorMessage = null;
            int result;
            fixed (byte* data = MemoryMarshal.Cast<T, byte>(image.GetPixelSpan()))
            {
                var rows = stackalloc byte*[region.Height];

                for (var j = 0; j < region.Height; j++)
                    rows[j] = data + sizeof(T) * (region.X + (region.Y + j) * image.Width);

                result = PngInteropHelper.write_png_to_file(filename, 
                    rows, (uint) region.Width, (uint) region.Height, bitWidth,
                    colorType, InterlaceType.None, CompressionType.Default, FilterType.Default, TransformType.Identity, 
                    (uint)(image.Width * sizeof(T)), &errorMessage);
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
        
        public static unsafe class PngInteropHelper
        {
            private const string LibraryName = "png_interop_helper";

            [DllImport(LibraryName)]
            public static extern int write_png_to_file(string filename, 
                byte** rows, uint width, uint height, uint bit_width,
                ColorType color_type, InterlaceType interlace_type, CompressionType compression_type,
                FilterType filter_type, TransformType transform_type,
                uint row_size, sbyte** perror_message);
        }
    }
}
