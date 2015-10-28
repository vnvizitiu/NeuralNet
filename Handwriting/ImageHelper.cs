﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handwriting {

    public static class ImageHelper {

        public static Bitmap CropWhitespace(this Bitmap bmp, bool square) {
            int bitPerPixel = Image.GetPixelFormatSize(bmp.PixelFormat);

            if(bitPerPixel != 24 && bitPerPixel != 32)
                throw new InvalidOperationException($"Invalid PixelFormat: {bitPerPixel}b");

            var bottom = 0;
            var left = bmp.Width;
            var right = 0;
            var top = bmp.Height;

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

            unsafe
            {
                byte* dataPtr = (byte*)bmpData.Scan0;

                for(var y = 0; y < bmp.Height; y++) {
                    for(var x = 0; x < bmp.Width; x++) {
                        var rgbPtr = dataPtr + (x * (bitPerPixel / 8));

                        var b = rgbPtr[0];
                        var g = rgbPtr[1];
                        var r = rgbPtr[2];

                        byte? a = null;
                        if(bitPerPixel == 32) {
                            a = rgbPtr[3];
                        }

                        if(b != 0xFF || g != 0xFF || r != 0xFF || (a.HasValue && a.Value != 0xFF)) {
                            if(x < left)
                                left = x;

                            if(x >= right)
                                right = x + 1;

                            if(y < top)
                                top = y;

                            if(y >= bottom)
                                bottom = y + 1;
                        }
                    }

                    dataPtr += bmpData.Stride;
                }
            }

            bmp.UnlockBits(bmpData);

            if(left < right && top < bottom) {
                var width = right - left;
                var height = bottom - top;

                if(square) {
                    var largestDimention = width > height ? width : height;
                    return bmp.Clone(new Rectangle(left, top, largestDimention, largestDimention), bmp.PixelFormat);
                } else {
                    return bmp.Clone(new Rectangle(left, top, width, height), bmp.PixelFormat);
                }
            }

            return null; // Entire image should be cropped, so just return null
        }

        public static Bitmap Resize(this Bitmap image, int width, int height, bool highQuality) {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using(var graphics = Graphics.FromImage(destImage)) {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = highQuality ? CompositingQuality.HighQuality : CompositingQuality.HighSpeed;
                graphics.InterpolationMode = highQuality ? InterpolationMode.HighQualityBicubic : InterpolationMode.Low;
                graphics.SmoothingMode = highQuality ? SmoothingMode.HighQuality : SmoothingMode.None;
                graphics.PixelOffsetMode = highQuality ? PixelOffsetMode.HighQuality : PixelOffsetMode.None;

                using(var wrapMode = new ImageAttributes()) {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}