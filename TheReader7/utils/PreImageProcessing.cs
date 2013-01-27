using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows;

namespace TheReader7
{
    class PreImageProcessing
    {
        public static WriteableBitmap deskew(WriteableBitmap bmp)
        {
            gmseDeskew sk = new gmseDeskew(bmp);
            double skewAngle = -1 * sk.GetSkewAngle();
            return WriteableBitmapExtensions.RotateFree(bmp, skewAngle);
        }

        /// <summary>
        /// returns a RectangleGeometry surrounding the text
        /// </summary>
        public static Rect detectTextBlob(WriteableBitmap bmp)
        {
            int t = detectTop(bmp);     // top
            int b = detectBottom(bmp);  // bottom
            int l = detectLeft(bmp);    // left
            int r = detectRight(bmp);   // right

            // fix for half cutting text
            t -= 10;
            b += 10;
            l -= 10;
            r += 15;

            int wid = r - l;
            int high = b - t;
            Rect rec = new Rect(l, t, wid, high);
            return rec;
        }

        /// <summary>
        /// A simple version of Canny Edge detection
        /// </summary>
        public static WriteableBitmap edgeDetection(WriteableBitmap bmp)
        {
            WriteableBitmap temp = new WriteableBitmap(bmp.PixelWidth, bmp.PixelHeight);
            bmp = ToGrayScale(bmp);
            bmp = WriteableBitmapExtensions.Convolute(bmp, WriteableBitmapExtensions.KernelGaussianBlur3x3);
            double delta = 1.0;
            // thetaThr = theta threshold value
            // magThr = magnitude threshold value
            int theta = 200, thetaThr = 360, magThr = 20;
            for (int r = 0; r < bmp.PixelHeight; r++)
            {
                for (int c = 0; c < bmp.PixelWidth; c++)
                {
                    double pdx = pixDiffdx(bmp, c, r);
                    double pdy = pixDiffdy(bmp, c, r);
                    double th = gradTheta(bmp, c, r, pdx, pdy);
                    double gm = gradMagnitude(pdx, pdy);

                    // Can also use this 
                    // if(Math.Abs(th) <= thetaThr && gm >= magThr)
                    // if more intensity of brightness is needed

                    if (gm >= 20 && (th < 90 && th > -90))
                    {
                        // is an edge
                        temp.SetPixel(c, r, Colors.White);
                    }
                    else
                    {
                        // not an edge
                        temp.SetPixel(c, r, Colors.Black);
                    }
                }
            }
            return temp;

        }

        /// <summary>
        /// returns true if the pixel is not at the border
        /// </summary>
        private static bool inRange(WriteableBitmap bmp, int col, int row)
        {
            return (col > 1 && col < (bmp.PixelWidth - 1) && row > 1 && row < (bmp.PixelHeight - 1));
        }

        /// <summary>
        /// Computes luminosity changes in X direction
        /// </summary>
        private static double pixDiffdx(WriteableBitmap bmp, int col, int row, double delta = 1.0)
        {
            if (!inRange(bmp, col, row))
            {
                return delta;
            }
            double dx = WriteableBitmapExtensions.GetBrightness(bmp, col + 1, row) -
                WriteableBitmapExtensions.GetBrightness(bmp, col - 1, row);
            if (dx == 0)
            {
                dx = delta;
            }
            return dx;
        }

        /// <summary>
        /// Computes luminosity changes in Y direction
        /// </summary>
        private static double pixDiffdy(WriteableBitmap bmp, int col, int row, double delta = 1.0)
        {
            if (!inRange(bmp, col, row))
            {
                return delta;
            }
            double dy = WriteableBitmapExtensions.GetBrightness(bmp, col, row - 1) -
                WriteableBitmapExtensions.GetBrightness(bmp, col, row + 1);
            if (dy == 0)
            {
                dy = delta;
            }
            return dy;
        }

        /// <summary>
        /// Computes magnitude of gradient
        /// </summary>
        private static double gradMagnitude(double pixDiffx, double pixDiffy)
        {
            return Math.Sqrt(Math.Pow(pixDiffx, 2.0) + Math.Pow(pixDiffy, 2.0));
        }

        /// <summary>
        /// Computes theta angle of gradient
        /// </summary>
        private static double gradTheta(WriteableBitmap bmp, int col, int row, double pixDiffdx, double pixDiffdy, double delta = 1.0, double theta = -200)
        {
            if (!inRange(bmp, col, row))
            {
                return theta;
            }
            if (pixDiffdx == pixDiffdy && pixDiffdx == delta)
            {
                return theta;
            }
            double th = Math.Atan2(pixDiffdy, pixDiffdx) * (180 / Math.PI);
            if (th < 0)
            {
                return Math.Floor(th);
            }
            else if (th > 0)
            {
                return Math.Ceiling(th);
            }
            else
            {
                return th;
            }
        }


        /// <summary>
        /// Converts a WritableBitmap to grayscale image
        /// </summary>
        public static WriteableBitmap ToGrayScale(WriteableBitmap bmp)
        {
            for (int i = 0; i < bmp.PixelHeight; i++)
            {
                for (int j = 0; j < bmp.PixelWidth; j++)
                {
                    byte grayScale = Convert.ToByte((bmp.GetPixel(j, i).R * 0.3) + (bmp.GetPixel(j, i).G * 0.59) + (bmp.GetPixel(j, i).B * 0.11));
                    Color c = Color.FromArgb(255, grayScale, grayScale, grayScale);
                    bmp.SetPixel(j, i, c);
                }
            }
            return bmp;
        }

        /// <summary>
        /// returns the threshold of text, if the threshold count is greater than it, then the row contains text
        /// </summary>
        private static int thCountX(int width)
        {
            return width / 5;
        }

        /// <summary>
        /// returns the threshold of text, if the threshold count is greater than it, then the column contains text
        /// </summary>
        private static int thCountY(int height)
        {
            return (height / 10);
        }

        private static int detectTop(WriteableBitmap bmp)
        {
            for (int r = 0; r < bmp.PixelHeight - 1; r++)
            {
                int count = 0;
                for (int c = 0; c < bmp.PixelWidth - 1; c++)
                {
                    Color color = bmp.GetPixel(c, r);
                    if (color == Colors.White)
                    {
                        count++;
                    }
                }
                if (count > thCountX(bmp.PixelWidth))
                {
                    return r;
                }
            }
            return 0;
        }

        private static int detectLeft(WriteableBitmap bmp)
        {
            for (int c = 0; c < bmp.PixelWidth - 1; c++)
            {
                int count = 0;
                for (int r = 0; r < bmp.PixelHeight - 1; r++)
                {
                    Color color = bmp.GetPixel(c, r);
                    if (color == Colors.White)
                    {
                        count++;
                    }
                }
                if (count > thCountY(bmp.PixelHeight))
                {
                    return c;
                }
            }
            return 0;
        }

        private static int detectBottom(WriteableBitmap bmp)
        {
            for (int r = (bmp.PixelHeight - 1); r > 0; r--)
            {
                int count = 0;
                for (int c = (bmp.PixelWidth - 1); c > 0; c--)
                {
                    Color color = bmp.GetPixel(c, r);
                    if (color == Colors.White)
                    {
                        count++;
                    }
                }
                if (count > thCountX(bmp.PixelWidth) / 8)
                {
                    return r;
                }
            }
            return 0;
        }

        private static int detectRight(WriteableBitmap bmp)
        {
            for (int c = (bmp.PixelWidth - 1); c > 0; c--)
            {
                int count = 0;
                for (int r = (bmp.PixelHeight - 1); r > 0; r--)
                {
                    Color color = bmp.GetPixel(c, r);
                    if (color == Colors.White)
                    {
                        count++;
                    }
                }
                if (count > thCountY(bmp.PixelHeight) / 8)
                {
                    return c;
                }
            }
            return 0;
        }

    }


    public class gmseDeskew
    {
        // Representation of a line in the image.
        public class HougLine
        {
            // Count of points in the line.
            public int Count;
            // Index in Matrix.
            public int Index;
            // The line is represented as all x,y that solve y*cos(alpha)-x*sin(alpha)=d
            public double Alpha;
            public double d;
        }
        // The Bitmap
        WriteableBitmap cBmp;
        // The range of angles to search for lines
        double cAlphaStart = -20;
        double cAlphaStep = 0.2;
        int cSteps = 40 * 5;
        // Precalculation of sin and cos.
        double[] cSinA;
        double[] cCosA;
        // Range of d
        double cDMin;
        double cDStep = 1;
        int cDCount;
        // Count of points that fit in a line.

        int[] cHMatrix;
        // Calculate the skew angle of the image cBmp.
        public double GetSkewAngle()
        {
            gmseDeskew.HougLine[] hl = null;
            int i = 0;
            double sum = 0;
            int count = 0;

            // Hough Transformation
            Calc();
            // Top 20 of the detected lines in the image.
            hl = GetTop(20);
            // Average angle of the lines
            for (i = 0; i <= 19; i++)
            {
                sum += hl[i].Alpha;
                count += 1;
            }
            return sum / count;
        }

        // Calculate the Count lines in the image with most points.
        private HougLine[] GetTop(int Count)
        {
            HougLine[] hl = null;
            int i = 0;
            int j = 0;
            HougLine tmp = null;
            int AlphaIndex = 0;
            int dIndex = 0;

            hl = new HougLine[Count + 1];
            for (i = 0; i <= Count - 1; i++)
            {
                hl[i] = new HougLine();
            }
            for (i = 0; i <= cHMatrix.Length - 1; i++)
            {
                if (cHMatrix[i] > hl[Count - 1].Count)
                {
                    hl[Count - 1].Count = cHMatrix[i];
                    hl[Count - 1].Index = i;
                    j = Count - 1;
                    while (j > 0 && hl[j].Count > hl[j - 1].Count)
                    {
                        tmp = hl[j];
                        hl[j] = hl[j - 1];
                        hl[j - 1] = tmp;
                        j -= 1;
                    }
                }
            }
            for (i = 0; i <= Count - 1; i++)
            {
                dIndex = hl[i].Index / cSteps;
                AlphaIndex = hl[i].Index - dIndex * cSteps;
                hl[i].Alpha = GetAlpha(AlphaIndex);
                hl[i].d = dIndex + cDMin;
            }
            return hl;
        }
        public gmseDeskew(WriteableBitmap bmp)
        {
            cBmp = bmp;
        }
        // Hough Transforamtion:
        private void Calc()
        {
            int x = 0;
            int y = 0;
            int hMin = cBmp.PixelHeight / 4;
            int hMax = cBmp.PixelHeight * 3 / 4;

            Init();
            for (y = hMin; y <= hMax; y++)
            {
                for (x = 1; x <= cBmp.PixelWidth - 2; x++)
                {
                    // Only lower edges are considered.
                    if (IsBlack(x, y))
                    {
                        if (!IsBlack(x, y + 1))
                        {
                            Calc(x, y);
                        }
                    }
                }
            }
        }
        // Calculate all lines through the point (x,y).
        private void Calc(int x, int y)
        {
            int alpha = 0;
            double d = 0;
            int dIndex = 0;
            int Index = 0;

            for (alpha = 0; alpha <= cSteps - 1; alpha++)
            {
                d = y * cCosA[alpha] - x * cSinA[alpha];
                dIndex = CalcDIndex(d);
                Index = dIndex * cSteps + alpha;
                try
                {
                    cHMatrix[Index] += 1;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }
        private int CalcDIndex(double d)
        {
            return Convert.ToInt32(d - cDMin);
        }
        private bool IsBlack(int x, int y)
        {
            Color c = default(Color);
            double luminance = 0;

            c = cBmp.GetPixel(x, y);
            luminance = (c.R * 0.299) + (c.G * 0.587) + (c.B * 0.114);
            return luminance < 140;
        }
        private void Init()
        {
            int i = 0;
            double angle = 0;

            // Precalculation of sin and cos.
            cSinA = new double[cSteps];
            cCosA = new double[cSteps];
            for (i = 0; i <= cSteps - 1; i++)
            {
                angle = GetAlpha(i) * Math.PI / 180.0;
                cSinA[i] = Math.Sin(angle);
                cCosA[i] = Math.Cos(angle);
            }
            // Range of d:
            cDMin = -cBmp.PixelWidth;
            cDCount = (int)(2 * (cBmp.PixelWidth + cBmp.PixelHeight) / cDStep);
            cHMatrix = new int[cDCount * cSteps + 1];
        }

        public double GetAlpha(int Index)
        {
            return cAlphaStart + Index * cAlphaStep;
        }
    }

}
