
# Overview

This is a quick and dirty bindings to libpng used to get (a bit) faster encoding of the png files for SixLabors.ImageSharp.

# Limitations

Only `Rgba32`, `Rgb24`, `Gray8` and `Gray16` pixel formats are supported. This can be easily fixed if needed (PRs are welcome).

As of now only writing to file (by it's name) is supported. Lifting this limitation will require some more code in the native part.

