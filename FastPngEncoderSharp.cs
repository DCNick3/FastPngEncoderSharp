﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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

        private static unsafe void WritePngToFileNative<T>(string filename, Image<T> image,
            Rectangle region) where T : unmanaged, IPixel<T>
        {
            ColorType colorType;
            uint bitWidth;
            
            if (!new Rectangle(Point.Empty, image.Size()).Contains(region))
                throw new ArgumentException("Region should be smaller than the source bounds.", nameof(region));

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
                case L8 _:
                    bitWidth = 8;
                    colorType = ColorType.Gray;
                    break;
                case L16 _:
                    bitWidth = 16;
                    colorType = ColorType.Gray;
                    break;
                default:
                    throw new NotImplementedException("The pixel format is not implemented");
            }
            
            sbyte* errorMessage = null;
            int result;
            if (!image.TryGetSinglePixelSpan(out var singlePixelSpan))
                throw new NotSupportedException(
                    "To use the image with FastPngEncoderSharp the backing buffer must be continuous");
            
            fixed (byte* data = MemoryMarshal.Cast<T, byte>(singlePixelSpan))
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
        
        public static void WritePngToFile<T>(string filename, Image<T> image,
            Rectangle region) where T : unmanaged, IPixel<T>
        {
            if (PngInteropHelper.IsAvailable)
                WritePngToFileNative(filename, image, region);
            else
            {
                if (image.Bounds() != region) 
                    image.Mutate(o => o.Crop(region));
                using (var fileStream = File.Create(filename))
                    image.SaveAsPng(fileStream);
            }
        }
        
        public static unsafe class PngInteropHelper
        {
            private const string LibraryName = "png_interop_helper";
            public static readonly bool IsAvailable; 
            
            static PngInteropHelper()
            {
                try
                {
                    Marshal.PrelinkAll(typeof (PngInteropHelper));
                    IsAvailable = true;
                }
                catch (DllNotFoundException)
                {
                    IsAvailable = false;
                }
            }

            [DllImport(LibraryName)]
            public static extern int write_png_to_file(string filename, 
                byte** rows, uint width, uint height, uint bit_width,
                ColorType color_type, InterlaceType interlace_type, CompressionType compression_type,
                FilterType filter_type, TransformType transform_type,
                uint row_size, sbyte** perror_message);
        }
    }
}
