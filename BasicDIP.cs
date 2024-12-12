using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ImageProcessing
{
    static class BasicDIP
    {
        public static void CopyImage(ref Bitmap a, ref Bitmap b)
        {
            // Create a new bitmap with the same dimensions and pixel format as the source
            b = new Bitmap(a.Width, a.Height, a.PixelFormat);

            // Define a rectangle covering the entire area of the bitmap
            var rect = new Rectangle(0, 0, a.Width, a.Height);

            // Lock bits of both source and destination images
            var aData = a.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, a.PixelFormat);
            var bData = b.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, b.PixelFormat);

            // Calculate the number of bytes in the image
            int bytes = Math.Abs(aData.Stride) * a.Height;

            // Create a byte array to hold the pixel data
            byte[] pixelData = new byte[bytes];

            // Copy data from the source image to the byte array
            System.Runtime.InteropServices.Marshal.Copy(aData.Scan0, pixelData, 0, bytes);

            // Copy data from the byte array to the destination image
            System.Runtime.InteropServices.Marshal.Copy(pixelData, 0, bData.Scan0, bytes);

            // Unlock bits in both images
            a.UnlockBits(aData);
            b.UnlockBits(bData);
        }



        public static void Rotate(ref Bitmap a, ref Bitmap b, int value)
        {
            float angleRad = (float)(value * Math.PI / 180);
            int centerX = a.Width / 2;
            int centerY = a.Height / 2;
            int width = a.Width;
            int height = a.Height;

            float cosA = (float)Math.Cos(angleRad);
            float sinA = (float)Math.Sin(angleRad);
            b = new Bitmap(width, height);

            BitmapData aData = a.LockBits(new Rectangle(0, 0, a.Width, a.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData bData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int strideA = aData.Stride;
            int strideB = bData.Stride;

            unsafe
            {
                byte* pA = (byte*)aData.Scan0;
                byte* pB = (byte*)bData.Scan0;

                for (int xp = 0; xp < width; xp++)
                {
                    for (int yp = 0; yp < height; yp++)
                    {
                        int x0 = xp - centerX;
                        int y0 = yp - centerY;

                        int xs = (int)(x0 * cosA + y0 * sinA) + centerX;
                        int ys = (int)(-x0 * sinA + y0 * cosA) + centerY;

                        if (xs >= 0 && xs < width && ys >= 0 && ys < height)
                        {
                            // Calculate positions in byte arrays
                            int indexA = ys * strideA + xs * 3;
                            int indexB = yp * strideB + xp * 3;

                            // Copy pixel data from source to destination
                            pB[indexB] = pA[indexA];
                            pB[indexB + 1] = pA[indexA + 1];
                            pB[indexB + 2] = pA[indexA + 2];
                        }
                    }
                }
            }

            a.UnlockBits(aData);
            b.UnlockBits(bData);
        }
        public static void Hist(ref Bitmap a, ref Bitmap b)
        {
            Color sample;
            Byte graydata;

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    sample = a.GetPixel(x, y);
                    graydata = (Byte)((sample.R + sample.G + sample.B) / 3);
                    Color greyscaled = Color.FromArgb(graydata, graydata, graydata);

                    a.SetPixel(x, y, greyscaled);
                }
            }
            int[] histData = new int[256];
            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    sample = a.GetPixel(x, y);
                    histData[sample.R]++;
                }
            }
            b = new Bitmap(256, 800);
            for (int x = 0; x < 256; x++)
            {
                for (int y = 0; y < 800; y++)
                {
                    b.SetPixel(x, y, Color.White);
                }
            }
            for (int x = 0; x < 256; x++)
            {
                for (int y = 0; y < Math.Min(histData[x] / 5, b.Height - 1); y++)
                {
                    b.SetPixel(x, (b.Height - 1) - y, Color.Black);
                }
            }
        }

        public static void Brightness(ref Bitmap a, ref Bitmap b, int value)
        {
            b = new Bitmap(a.Width, a.Height);

            BitmapData aData = a.LockBits(new Rectangle(0, 0, a.Width, a.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData bData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int strideA = aData.Stride;
            int strideB = bData.Stride;
            int width = a.Width;
            int height = a.Height;

            unsafe
            {
                byte* pA = (byte*)aData.Scan0;
                byte* pB = (byte*)bData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Calculate the position in the byte array
                        int indexA = y * strideA + x * 3;
                        int indexB = y * strideB + x * 3;

                        // Get original RGB values
                        int blue = pA[indexA];
                        int green = pA[indexA + 1];
                        int red = pA[indexA + 2];

                        // Apply brightness adjustment with clamping
                        pB[indexB] = (byte)Math.Min(Math.Max(blue + value, 0), 255);
                        pB[indexB + 1] = (byte)Math.Min(Math.Max(green + value, 0), 255);
                        pB[indexB + 2] = (byte)Math.Min(Math.Max(red + value, 0), 255);
                    }
                }
            }
            a.UnlockBits(aData);
            b.UnlockBits(bData);
        }

        public static void Greyscale(ref Bitmap a, ref Bitmap b)
        {
            if (a != null)
            {
                // Lock the bits of both bitmaps
                BitmapData aData = a.LockBits(new Rectangle(0, 0, a.Width, a.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                BitmapData bData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                int strideA = aData.Stride;
                int strideB = bData.Stride;
                int width = a.Width;
                int height = a.Height;

                unsafe
                {
                    byte* pA = (byte*)aData.Scan0;
                    byte* pB = (byte*)bData.Scan0;

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            // Get the RGB values
                            byte blue = pA[(y * strideA) + x * 3];
                            byte green = pA[(y * strideA) + x * 3 + 1];
                            byte red = pA[(y * strideA) + x * 3 + 2];

                            // Calculate the grayscale value
                            byte grayscale = (byte)((red + green + blue) / 3);

                            // Set the grayscale value in the output bitmap
                            pB[(y * strideB) + x * 3] = grayscale;
                            pB[(y * strideB) + x * 3 + 1] = grayscale;
                            pB[(y * strideB) + x * 3 + 2] = grayscale;
                        }
                    }
                }

                // Unlock the bits
                a.UnlockBits(aData);
                b.UnlockBits(bData);
            }
        }

        public static void Invert(ref Bitmap a, ref Bitmap b)
        {
            int width = a.Width;
            int height = a.Height;
            b = new Bitmap(width, height);

            BitmapData aData = a.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData bData = b.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int strideA = aData.Stride;
            int strideB = bData.Stride;

            unsafe
            {
                byte* pA = (byte*)aData.Scan0;
                byte* pB = (byte*)bData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int indexA = y * strideA + x * 3;
                        int indexB = y * strideB + x * 3;

                        // Invert color
                        pB[indexB] = (byte)(255 - pA[indexA]);       // Blue
                        pB[indexB + 1] = (byte)(255 - pA[indexA + 1]); // Green
                        pB[indexB + 2] = (byte)(255 - pA[indexA + 2]); // Red
                    }
                }
            }

            a.UnlockBits(aData);
            b.UnlockBits(bData);
        }
        public static void MirrorVertical(ref Bitmap a, ref Bitmap b)
        {
            int width = a.Width;
            int height = a.Height;
            b = new Bitmap(width, height);

            BitmapData aData = a.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData bData = b.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int strideA = aData.Stride;
            int strideB = bData.Stride;

            unsafe
            {
                byte* pA = (byte*)aData.Scan0;
                byte* pB = (byte*)bData.Scan0;

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int indexA = y * strideA + x * 3;
                        int indexB = (height - 1 - y) * strideB + x * 3;

                        pB[indexB] = pA[indexA];        // Blue
                        pB[indexB + 1] = pA[indexA + 1]; // Green
                        pB[indexB + 2] = pA[indexA + 2]; // Red
                    }
                }
            }

            a.UnlockBits(aData);
            b.UnlockBits(bData);
        }

        public static void MirrorHorizontal(ref Bitmap a, ref Bitmap b)
        {
            int width = a.Width;
            int height = a.Height;
            b = new Bitmap(width, height);

            BitmapData aData = a.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData bData = b.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int strideA = aData.Stride;
            int strideB = bData.Stride;

            unsafe
            {
                byte* pA = (byte*)aData.Scan0;
                byte* pB = (byte*)bData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int indexA = y * strideA + x * 3;
                        int indexB = y * strideB + (width - 1 - x) * 3;

                        pB[indexB] = pA[indexA];        // Blue
                        pB[indexB + 1] = pA[indexA + 1]; // Green
                        pB[indexB + 2] = pA[indexA + 2]; // Red
                    }
                }
            }

            a.UnlockBits(aData);
            b.UnlockBits(bData);
        }
        public static void Sepiafy(ref Bitmap a, ref Bitmap b)
        {
            int width = a.Width;
            int height = a.Height;
            b = new Bitmap(width, height);

            BitmapData aData = a.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData bData = b.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int strideA = aData.Stride;
            int strideB = bData.Stride;

            unsafe
            {
                byte* pA = (byte*)aData.Scan0;
                byte* pB = (byte*)bData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int indexA = y * strideA + x * 3;
                        int indexB = y * strideB + x * 3;

                        // Apply sepia tone
                        byte originalR = pA[indexA + 2];
                        byte originalG = pA[indexA + 1];
                        byte originalB = pA[indexA];

                        pB[indexB + 2] = (byte)Math.Max(0, Math.Min(255, (int)(0.43 * originalR))); // Red channel
                        pB[indexB + 1] = (byte)Math.Max(0, Math.Min(255, (int)(0.26 * originalG))); // Green channel
                        pB[indexB] = (byte)Math.Max(0, Math.Min(255, (int)(0.078 * originalB)));    // Blue channel
                    }
                }
            }

            a.UnlockBits(aData);
            b.UnlockBits(bData);
        }
        public static bool Contrast(ref Bitmap b, sbyte nContrast)
        {
            if (nContrast < -100 || nContrast > 100) return false;

            double contrastFactor = (100.0 + nContrast) / 100.0;
            contrastFactor *= contrastFactor; // Square the factor for contrast adjustment

            // GDI+ still lies to us - the return format is BGR, NOT RGB.
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                int nOffset = stride - b.Width * 3;

                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < b.Width; ++x)
                    {
                        // Process pixel in BGR format
                        for (int colorIndex = 0; colorIndex < 3; colorIndex++)
                        {
                            // Read the color component
                            int colorValue = p[colorIndex];

                            // Apply contrast adjustment
                            double pixel = (colorValue / 255.0 - 0.5) * contrastFactor + 0.5;
                            pixel *= 255;

                            // Clamp pixel value to [0, 255]
                            p[colorIndex] = (byte)(pixel < 0 ? 0 : (pixel > 255 ? 255 : pixel));
                        }

                        p += 3; // Move to the next pixel
                    }
                    p += nOffset; // Move to the next row
                }
            }

            b.UnlockBits(bmData);
            return true;
        }
        public static void Scale(ref Bitmap a, ref Bitmap b, int value)
        {
            if (a == null)
                return;

            int newWidth = (int)(value / 50f * a.Width);
            int newHeight = (int)(value / 50f * a.Height);

            b = new Bitmap(newWidth, newHeight);

            for (int i = 0; i < newWidth; ++i)
                for (int j = 0; j < newHeight; ++j)
                    b.SetPixel(i, j, a.GetPixel(
                        i * a.Width / newWidth,
                        j * a.Height / newHeight
                        )
                     );
        }
        public static void BinaryStuff(ref Bitmap a, ref Bitmap b, int value)
        {
            if (a == null)
                return;

            b = new Bitmap(a.Width, a.Height);
            int threshold = value;

            for (int i = 0; i < a.Width; ++i)
                for (int j = 0; j < a.Height; ++j)
                {
                    Color pixel = a.GetPixel(i, j);
                    b.SetPixel(i, j,
                        (pixel.R + pixel.G + pixel.B) / 3 < threshold ?
                        Color.Black : Color.White
                        );
                }
        }
    }
}