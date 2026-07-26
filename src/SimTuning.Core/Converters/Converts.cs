// Copyright (c) 2025 tuke productions. All rights reserved.
using SkiaSharp;
using System;
using System.IO;

namespace SimTuning.Core.Converters
{
    public class Converts
    {
        /// <summary>
        /// Sks the bitmap to stream.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <returns></returns>
        public static Stream SKBitmapToStream(SKBitmap bitmap)
        {
            // create an image COPY
            SKImage image = SKImage.FromBitmap(bitmap);

            // encode the image (defaults to PNG)
            SKData encoded = image.Encode();

            // get a stream over the encoded data
            return encoded.AsStream();
        }
    }
}
