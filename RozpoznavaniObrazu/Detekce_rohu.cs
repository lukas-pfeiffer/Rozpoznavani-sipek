using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RozpoznavaniObrazu
{
    class Detekce_rohu
    {
        byte[] oznaceniB = { 200, 200, 200, 0 };
        byte[] oznaceniC = { 0, 0, 0, 0 };
        byte[] oznaceniR = { 0, 0, 255, 0 };

        //Detekce rohu
        public BitmapSource detekce(BitmapSource obrazek, int threshold)       
        {
            int t2 = threshold;

            int seda;
            byte[] barva = { 0, 0, 0, 0 };
            BitmapSource bitmapSource = new FormatConvertedBitmap(obrazek, PixelFormats.Rgb24, null, 0);
            WriteableBitmap wbmap = new WriteableBitmap(bitmapSource);

            int vyska = wbmap.PixelHeight;
            int sirka = wbmap.PixelWidth;

            int stride = wbmap.PixelWidth * 4;
            int size = wbmap.PixelHeight * stride;

            byte[] pixely = new byte[size];
            wbmap.CopyPixels(pixely, stride, 0);

            //Převod na šedotonovy obraz
            for (int y = 0; y < wbmap.PixelHeight; y++)
            {
                for (int x = 0; x < wbmap.PixelWidth; x++)
                {
                    int index = y * stride + 4 * x;
                    byte red = pixely[index];
                    byte green = pixely[index + 1];
                    byte blue = pixely[index + 2];
                    // byte alpha = pixely[index + 3];

                    seda = (red + green + blue) / 3;
                    pixely[index] = (byte)seda;
                    barva[0] = (byte)seda;
                    barva[1] = (byte)seda;
                    barva[2] = (byte)seda;

                    wbmap.WritePixels(new Int32Rect(x, y, 1, 1), barva, stride, 0);
                }
            }

            List<Souradnice> seznamRohu = new List<Souradnice>();
            //Obrazek1.Source = wbmap;
            WriteableBitmap cdmap = new WriteableBitmap(wbmap);

            //Trajkovic-uv detektor rohu
            int[,] corner = new int[cdmap.PixelWidth, cdmap.PixelHeight];
            int ra, rb, iAa, iA, iC, iB, iBb;
            
            int[,] m = new int[cdmap.PixelWidth, cdmap.PixelHeight];
            
            int b1, b2, b, a, c;
            for (int y = 1; y < cdmap.PixelHeight - 1; y++)   // simple cornerness measure, minimal variatons among directions
            {
                for (int x = 1; x < cdmap.PixelWidth - 1; x++)
                {
                    int index = y * stride + 4 * x;
                    iC = pixely[index]; //ve skutecnosti odkazuje na cervenou barvu
                    index = y * stride + 4 * (x + 1);
                    iA = pixely[index];
                    index = y * stride + 4 * (x - 1);
                    iAa = pixely[index];
                    index = (y + 1) * stride + 4 * x;
                    iB = pixely[index];
                    index = (y - 1) * stride + 4 * x;
                    iBb = pixely[index];

                    ra = (iA - iC) * (iA - iC) + (iAa - iC) * (iAa - iC);
                    rb = (iB - iC) * (iB - iC) + (iBb - iC) * (iBb - iC);
                    c = ra;
                    b1 = (iB - iA) * (iA - iC) + (iBb - iAa) * (iAa - iC);
                    b2 = (iB - iAa) * (iAa - iC) + (iBb - iA) * (iA - iC);
                    b = Math.Min(b1, b2);
                    a = rb - ra - 2 * b;

                    if ((b < 0) & ((a + b) > 0))
                    {
                        m[x, y] = c - ((b * b) / a);
                    }
                    else
                    {
                        m[x, y] = Math.Min(ra, rb);
                    }
                }
            }

            for (int y = 1; y < cdmap.PixelHeight - 1; y++)
            {
                for (int x = 1; x < cdmap.PixelWidth - 1; x++)
                {
                    if (m[x, y] > t2)
                    {
                        cdmap.WritePixels(new Int32Rect(x, y, 1, 1), oznaceniB, stride, 0);
                        seznamRohu.Add(new Souradnice { coordX = x, coordY = y });
                        wbmap.WritePixels(new Int32Rect(x, y, 1, 1), oznaceniR, stride, 0);
                    }
                    else
                    {
                        cdmap.WritePixels(new Int32Rect(x, y, 1, 1), oznaceniC, stride, 0);
                    }
                }
            }
            return wbmap;
        }
    }
}
