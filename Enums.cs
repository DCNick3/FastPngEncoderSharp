namespace FastPngEncoderSharp
{
    public enum ColorType : uint
    {
        Gray = 0,
        Palette = 3,
        Rgb = 2,
        RgbAlpha = 6,
        GrayAlpha = 4,
    }

    public enum InterlaceType : uint
    {
        None = 0,
        Adam7 = 1
    }

    public enum CompressionType : uint
    {
        Default = 0
    }

    public enum FilterType : uint
    {
        Default = 0,
        IntrapixelDifferencing = 64,
    }

    public enum TransformType : uint
    {
        Identity = 0x0,
        Strip16 = 0x1,
        StripAlpha = 0x2,
        Packing = 0x4,
        PackSwap = 0x8,
        Expand = 0x10,
        InvertMono = 0x20,
        Shift = 0x40,
        Bgr = 0x80,
        SwapAlpha = 0x100,
        SwapEndian = 0x200,
        InvertAlpha = 0x400,
        StripFillerBefore = 0x800,
        StripFillerAfter = 0x1000,
        GrayToRgb = 0x2000,
        Expand16 = 0x4000,
        Scale16 = 0x8000,
    }
}