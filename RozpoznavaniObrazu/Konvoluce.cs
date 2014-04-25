using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RozpoznavaniObrazu
{
    class Konvoluce
    {   
        /* funguje, ale lehký šum
        private double[,] maska =
        new double[,] { { -1,  0,  0,  0,  0, }, 
                        {  0, -2,  0,  0,  0, },
                        {  0,  0,  6,  0,  0, },
                        {  0,  0,  0, -2,  0, },
                        {  0,  0,  0,  0, -1, }, };
         private double faktor = 1;
        private double bias = 0;*/
        
        /* lehký šum
        private double[,] maska =
       new double[,] { { -5,  0,  0, },  
                        {  0,  0,  0, },  
                        {  0,  0,  5, }, }; 
         private double faktor = 1;
        private double bias = 0;*/

        /* první obrázky mají velký šum, jinak vždy se střídají 3 černé pixely a 1 bily
        private double[,] maska =
        new double[,] { { 1,  1, 1, },  
                        { 1, -7, 1, },  
                        { 1,  1, 1, }, }; 
        private double faktor = 1;
        private double bias = 0;*/

        //embos 45, funguje skoro bez šumu
        private double[,] maska = new double[,] { { -1, -1, 0, }, { -1, 0, 1, }, { 0, 1, 1, }, };
        private double faktor = 1;
        private double bias = 128;

        public BitmapSource konvoluce(BitmapImage obrazek)
        {
            BitmapSource bitmapSource = new FormatConvertedBitmap(obrazek, PixelFormats.Rgb24, null, 0);
            WriteableBitmap wbmap = new WriteableBitmap(bitmapSource);
            
            int sirka = 768;
            int vyska = 576;

            int stride = vyska * 4;// možno přehodit sirku za vysku a naopak
            int size = sirka * stride;

            byte[] pixely = new byte[size];
            byte[] vyslednePixely = new byte[size];


            bitmapSource.CopyPixels(pixely, stride, 0);

            double modra = 0.0;
            double zelena = 0.0;
            double cervena = 0.0;

            int maskaSirka = maska.GetLength(1);
            int maskaVyska = maska.GetLength(0);

            int maskaOffset = (maskaSirka - 1) / 2;
            int vypOffset = 0;

            int byteOffset = 0;

             for (int offsetY = maskaOffset; offsetY < vyska - maskaOffset; offsetY++)
                {
                for (int offsetX = maskaOffset; offsetX < sirka - maskaOffset; offsetX++)
                {
                    modra = 0;
                    zelena = 0;
                    cervena = 0;

                    byteOffset = offsetY * stride + offsetX * 4;

                    for (int filterY = -maskaOffset; filterY <= maskaOffset; filterY++)
                    {
                        for (int filterX = -maskaOffset; filterX <= maskaOffset; filterX++)
                        {
                            vypOffset = byteOffset + (filterX * 4) + (filterY * stride);//Umyslná chyba(místo 3 je 4)
                            
                            modra += (double)(pixely[vypOffset]) * maska[filterY + maskaOffset, filterX + maskaOffset];
                            
                            zelena += (double)(pixely[vypOffset + 1]) * maska[filterY + maskaOffset, filterX + maskaOffset];
                            
                            cervena += (double)(pixely[vypOffset + 2]) * maska[filterY + maskaOffset, filterX + maskaOffset];
                        }
                    }

                    modra = faktor * modra + bias;
                    zelena = faktor * zelena + bias;
                    cervena = faktor * cervena + bias;

                    if (modra > 255)
                    { 
                        modra = 255; 
                    }
                    else if (modra < 0)
                    {
                        modra = 0;
                    }

                    if (zelena > 255)
                    {
                        zelena = 255; 
                    }
                    else if (zelena < 0)
                    { 
                        zelena = 0; 
                    }

                    if (cervena > 255)
                    { 
                        cervena = 255; 
                    }
                    else if (cervena < 0)
                    { 
                        cervena = 0; 
                    }

                    vyslednePixely[byteOffset] = (byte)(modra);
                    vyslednePixely[byteOffset + 1] = (byte)(zelena);
                    vyslednePixely[byteOffset + 2] = (byte)(cervena);
                    vyslednePixely[byteOffset + 3] = 255;//Umyslná chyba(není nutný)
                }
            }                                                

            var resultData = BitmapSource.Create(sirka, vyska, 96d, 96d, PixelFormats.Bgr24, null, vyslednePixely, stride);

            return resultData;
        }
    }
}
