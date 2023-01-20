// Copyright (c) 2021 tuke productions. All rights reserved.
//using SciColorMaps.Portable;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Spectrogram
{
    internal class Image
    {
        public static SKBitmap ApplyColormap(SKBitmap bmp, Colormap colormap)
        {
            // Farbtabellen erstellen
            byte[] atable = new byte[256];
            byte[] rtable = new byte[256];
            byte[] gtable = new byte[256];
            byte[] btable = new byte[256];

            // Farben der ColorMap holen
            //ColorMap colorMap = new ColorMap(colormap.ToString());
            //var mapColors = colorMap.Colors().ToList();

            // Farbtabellen zuweisen
            //for (int i = 0; i < 256; i++)
            //{
            //    atable[i] = (byte)255;
            //    rtable[i] = mapColors[i][0];
            //    gtable[i] = mapColors[i][1];
            //    btable[i] = mapColors[i][2];
            //}

            SKBitmap colorBitmap = new SKBitmap(bmp.Width, bmp.Height, SKColorType.Alpha8, SKAlphaType.Opaque);
            SKCanvas sKCanvas = new SKCanvas(colorBitmap);

            SKPaint paint = new SKPaint();
            paint.ColorFilter = SKColorFilter.CreateTable(atable, rtable, gtable, btable);

            sKCanvas.DrawBitmap(bmp, 0, 0, paint: paint);

            // return colorBitmap;
            return bmp;
        }

        public static SKBitmap GetBitmap(List<double[]> ffts, Colormap cmap, double intensity = 1, bool dB = false, bool roll = false, int rollOffset = 0)
        {
            if (ffts.Count == 0)
                throw new ArgumentException("This Spectrogram contains no FFTs (likely because no signal was added)");

            int width = ffts.Count;
            int Height = ffts[0].Length;

            // 8-bit alpha-only color
            SKBitmap bmp = new SKBitmap(width, Height, SKColorType.Alpha8, SKAlphaType.Opaque);

            // Bitmap bmp = new Bitmap(Width, Height, PixelFormat.Format8bppIndexed);
            // cmap.Apply(bmp);

            // var lockRect = new Rectangle(0, 0, Width, Height); BitmapData bitmapData =
            // bmp.LockBits(lockRect, ImageLockMode.ReadOnly, bmp.PixelFormat);

            // SKBitmap.RowBytes == BitmapData.Stride int stride = bmp.RowBytes;

            // byte[] bytes = new byte[bitmapData.Stride * bmp.Height]; byte[] bytes = new
            // byte[stride * bmp.Height];

            Parallel.For(0, width, col =>
            {
                int sourceCol = col;
                if (roll)
                {
                    sourceCol += width - rollOffset % width;
                    if (sourceCol >= width)
                        sourceCol -= width;
                }

                for (int row = 0; row < Height; row++)
                {
                    // var test = bmp.GetPixel(col, row);
                    double value = ffts[sourceCol][row];
                    if (dB)
                        value = 20 * Math.Log10(value + 1);
                    value *= intensity;
                    value = Math.Min(value, 255);

                    // int bytePosition = (Height - 1 - row) * stride + col;
                    // bytes[bytePosition] = (byte)value;

                    var color = new SKColor(255, 255, 255, (byte)value);
                    bmp.SetPixel(col, row, color);
                }
            });

            // IntPtr pix = bmp.GetPixels(); Marshal.Copy(bytes, 0, pix, bytes.Length);
            // bmp.SetPixels(pix);

            #region Rotate&Flip

            SKBitmap rotated = new SKBitmap(bmp.Width, bmp.Height, SKColorType.Alpha8, SKAlphaType.Opaque);

            var surface = new SKCanvas(rotated);

            surface.Translate(rotated.Width, 0);
            surface.Scale(-1, 1, 0, 0);
            surface.Translate(rotated.Width / 2, rotated.Height / 2);
            surface.RotateDegrees(180);
            surface.Translate(-rotated.Width / 2, -rotated.Height / 2);
            surface.DrawBitmap(bmp, 0, 0);

            #endregion Rotate&Flip

            // Marshal.Copy(bytes, 0, bitmapData.Scan0, bytes.Length);
            // bmp.UnlockBits(bitmapData);

            return ApplyColormap(rotated, cmap);
        }
    }
}