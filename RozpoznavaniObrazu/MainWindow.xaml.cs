using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Diagnostics;
using System.Threading;

namespace RozpoznavaniObrazu
{
    public partial class MainWindow : Window
    {
        #region deklarace proměných
        //sirka raw = 768;
        //vyska raw = 576;
        private static int sirka = 768;
        private static int vyska = 576;

        int cisloObr = 60;
        string sCisloObr;

        int[,] tvar;// = new int[sirka, vyska];

        BitmapSource obrazek;
        BitmapImage obrazekI;

        int x1 = 0;
        int y1 = 0;

        int x2 = 0;
        int y2 = 0;

        int x3 = 0;
        int y3 = 0;

        int x4 = 0;
        int y4 = 0;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        #region základní zpracování

        private BitmapSource nacteniRawDat(string cislo)
        { 
            byte[] data;
            using (var ms = new MemoryStream())
            {
                using (var f = File.OpenRead(@"Images\\" + cislo + "koala.raw"))//"c:\\000koala.raw"))
                {
                    byte[] buffer = new byte[100];
                    int read;
                    while ((read = f.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                }
                data = ms.ToArray();
            }

            var width = sirka;
            var height = vyska;
            var dpiX = 96d;
            var dpiY = 96d;
            var pixelFormat = PixelFormats.Rgb24;
            var bytesPerPixel = (pixelFormat.BitsPerPixel + 7) / 8;
            var stride = bytesPerPixel * width;

            List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
            colors.Add(System.Windows.Media.Colors.Red);
            colors.Add(System.Windows.Media.Colors.Blue);
            colors.Add(System.Windows.Media.Colors.Green);
            BitmapPalette myPalette = new BitmapPalette(colors);
            

            var bitmap = BitmapSource.Create(width, height, dpiX, dpiY, pixelFormat, myPalette, data, stride);

            obrazek = bitmap;
            Obrazek.Source = bitmap;

            return bitmap;
        }

        private BitmapImage prevodObrazku(BitmapSource obrazek)
        {
            JpegBitmapEncoder enkoder = new JpegBitmapEncoder();
            MemoryStream pamet = new MemoryStream();
            BitmapImage novyObrazek = new BitmapImage();

            enkoder.Frames.Add(BitmapFrame.Create(obrazek));
            enkoder.Save(pamet);

            novyObrazek.BeginInit();
            novyObrazek.StreamSource = new MemoryStream(pamet.ToArray());
            novyObrazek.EndInit();

            pamet.Close();

            return novyObrazek;
        }

        private void nacteniObrazku()
        {
            cisloObr++;

            if (cisloObr >= 727)
            {
                cisloObr = 70;
                return;
            }

            if (cisloObr < 10)
            {
                sCisloObr = "00" + cisloObr.ToString();
            }

            else if (cisloObr < 100)
            {
                sCisloObr = "0" + cisloObr.ToString();
            }

            else
            {
                sCisloObr = cisloObr.ToString();
            }

            udalosti.Items.Add(sCisloObr+"koala.raw");
            nacteniRawDat(sCisloObr);
        }
        
        private void zmenaBarev()
        {
            tvar = new int[sirka, vyska];

            int cerna = 0;
            int bila = 255;
            int seda;
            byte[] barva = {0, 0, 0, 0};

            WriteableBitmap novyObrazek = new WriteableBitmap(obrazekI);

            int lSirka = novyObrazek.PixelWidth; 
            int lVyska = novyObrazek.PixelHeight;
            int stride = lSirka * 4;
            int size = lVyska * stride;

            byte[] pixely = new byte[size];

            novyObrazek.CopyPixels(pixely, stride, 0);

            for (int i = 0; i < lVyska; i++)
            {
                for (int j = 0; j < lSirka; j++)
                {
                    int index = i * stride + 4 * j;

                    byte red = pixely[index+2];
                    byte green = pixely[index + 1];
                    byte blue = pixely[index];
                   
                    if (red > 200 && blue < 150 && green < 150)// && red <200)
                        //(red > 109 && green > 60/*78*/ && blue >  67/*127*/ && 
                        //red < 247 /*158*/ && green < 154 /*129*/ && blue < 190/*255*/ )
                        {
                            //seda = (red + green + blue) / 3;
                            pixely[index] = (byte)cerna;
                            barva[0] = (byte)cerna;
                            barva[1] = (byte)cerna;
                            barva[2] = (byte)cerna;

                            tvar[j, i] = 1;
                        }
                        else
                        {
                            //seda = (red + green + blue) /3;
                            pixely[index] = (byte)bila;
                            barva[0] = (byte)bila;
                            barva[1] = (byte)bila;
                            barva[2] = (byte)bila;
                        }
                        novyObrazek.WritePixels(new Int32Rect(j, i, 1, 1), barva, stride, 0);
                    }
            }
            obrazek = novyObrazek;
            //Obrazek.Source = novyObrazek; 
        }

        #endregion

        #region nalezení šipky a trojúhelníku

        private void odstraneniSamostatnychPixelu()
        {
            int pocet = 0;
            for (int i = 1; i < sirka-1; i++)
            {
                for (int j = 1; j < vyska-1; j++)
                {
                    if (tvar[i, j] == 1)
                    {
                        int cislo = 0;
                        if (tvar[i + 1, j] != 1)
                        {
                            cislo++;
                        }
                        if (tvar[i, j + 1] != 1)
                        {
                            cislo++;
                        }
                        if (tvar[i + 1, j + 1] != 1)
                        {
                            cislo++;
                        }
                        if (tvar[i - 1, j] != 1)
                        {
                            cislo++;
                        }
                        if (tvar[i, j - 1] != 1)
                        {
                            cislo++;
                        }
                        if (tvar[i - 1, j - 1] != 1)
                        {
                            cislo++;
                        }
                        if (tvar[i + 1, j - 1] != 1)
                        {
                            cislo++;
                        }
                        if (tvar[i - 1, j + 1] != 1)
                        {
                            cislo++;
                        }
                        if (cislo >= 5)
                        {
                            tvar[i, j] = 0;
                            pocet++;
                        }
                    }
                }
            }
            udalosti.Items.Add("-----------------------------------------");
            udalosti.Items.Add("Počet odstraněných pixelu: "+pocet);
        }

        private void nalezeniSouradniceX1()
        {
            for (int i = 0; i < sirka; i++)
            {
                for (int j = 0; j < vyska; j++)
                {
                    if (tvar[i,j]==1)
                    {
                        x1 = i;
                        y1 = j;
                        udalosti.Items.Add("-----------------------------------------");
                        udalosti.Items.Add("x1: "+x1+" y1: "+y1);
                        return;
                    }
                }
            }
        }

        private void nalezeniSouradniceX2()
        {
            for (int i = 0; i < vyska; i++)
            {
                for (int j = 0; j < sirka; j++)
                {
                    if (tvar[j, i] == 1)
                    {
                        x2 = j;
                        y2 = i;
                        udalosti.Items.Add("x2: " + x2+ " y2: " + y2);
                        return;
                    }
                }
            }
        }

        private void nalezeniSouradniceY1()
        {
            for (int i = sirka-1; i > 0; i--)
            {
                for (int j = vyska-1; j > 0; j--)
                {
                    if (tvar[i, j] == 1)
                    {
                        x3 = i;
                        y3 = j;
                        udalosti.Items.Add("x3: " + x3 + " y3: " + y3);
                        return;
                    }
                }
            }
        }

        private void nalezeniSouradniceY2()
        {
            for (int i = vyska-1; i > 0; i--)
            {
                for (int j = sirka-1; j > 0; j--)
                {
                    if (tvar[j, i] == 1)
                    {
                        x4 = j;
                        y4 = i;
                        udalosti.Items.Add("x4: " + x4 + " y4: " + y4);
                        return;
                    }
                }
            }
        }

        private bool nalezeniSipky()
        {
            if ( Math.Abs(x2-x4) > 17)//15 - fungovala, ale nepoznala něco málo asi dvě šipky
            {
                //udalosti.Items.Add("Neni šipka: x2, x4 "+x2+" "+x4+" "+ Math.Abs(x2-x4));
                return false;
            }
            if (Math.Abs(y2 - y4) < 26 && 
                Math.Abs(y2 - y4) > 220)
            {                
                //udalosti.Items.Add("Neni šipka: y2, y4 "+y2+" "+y4+" "+ Math.Abs(y2-y4));
                return false;
            }

            if (Math.Abs(x3 - x1) < 112 &&
                Math.Abs(x3 - x1) > 446)
            {
                //udalosti.Items.Add("Neni šipka: x1, x3 " + x1 + " " + x3 + " " + Math.Abs(x1 - x3));
                return false;
            }

            if (Math.Abs(y3 - y1) > 16)
            {                
                //udalosti.Items.Add("Neni šipka: y1, y3 "+y1+" "+y3+" "+ Math.Abs(y1-y3));
                return false;
            }

            if (Math.Abs(y4 - y3) < 14)
            {
                //udalosti.Items.Add("Neni šipka: y3, y4 " + y4 + " " + y3 + " " + Math.Abs(y4 - y3));
                return false;
            }

            udalosti.Items.Add("-----------------------------------------");
            if (x1 < 20 || x2 < 20 || x3 < 20 || x4 < 20 ||
                x1 > sirka - 20 || x2 > sirka - 20 || x3 > sirka - 20 || x4 > sirka - 20)
            {
                //udalosti.Items.Add("Šipka není celá");
                return false;
            }
            else
            {
                int X = x1 + ((x3 - x1) / 2);
                int Y = (x2 + x4) / 2;
                if (X < Y)
                {
                    udalosti.Items.Add("Šipka doprava");
                    return true;
                }
                else if (X > Y)
                {
                    udalosti.Items.Add("Šipka doleva");
                    return true;
                }
                else
                {
                    //udalosti.Items.Add("Nelze jednoznačně rozpoznat");
                    return false;
                }
            }
        }

        private bool nalezeniTrojuhelniku()
        {
            int pocet = 0;
            if (x2 < 10 || y2 < 10)
            {
                return false;
            }

            if (y2 < 50 && y2 > 200)
            {
                //udalosti.Items.Add("neni troj y2: " + y2);
                return false;
            }
            for (int i = -10; i < 10; i++)
            {
                for (int j = -10; j < 10; j++)
                {
                    if (tvar[x2 + i, y2 + 1] == 1)
                    {
                        pocet++;
                    }
                }
            }

            int y22 = 0;
            for (int i = 20; i < vyska-y2-20; i++)
            {
                if (tvar[x2,y2+i]==1)
                {
                    y22 = y2 + 1;
                    //udalosti.Items.Add("y22: " + y22);
                }
            }

            if (pocet >= 4 /*&& y22 > 19*/ && y22 < 200) //Math.Abs(y2-y22)<200 && Math.Abs(y2-y22)>50)
            {
                udalosti.Items.Add("-----------------------------------------");
                udalosti.Items.Add("Trojúhelník");// + pocet);
                return true;
            }
            else
            {
                //udalosti.Items.Add("Nelze rozpoznat   pocet: "+pocet+"  y22 "+y22);
                return false;
            }            
        }
        
        #endregion

        #region hlavní program

        private void hlavni()
        {
            udalosti.Items.Clear();
            info.Content = "";

            nacteniObrazku();
            Obrazek.Source = obrazek;

            obrazekI = prevodObrazku(obrazek);

            Konvoluce kon = new Konvoluce();
            obrazek = kon.konvoluce(obrazekI);

            obrazekI = prevodObrazku(obrazek);
            zmenaBarev();

            odstraneniSamostatnychPixelu();

            nalezeniSouradniceX1();
            nalezeniSouradniceX2();
            nalezeniSouradniceY1();
            nalezeniSouradniceY2();

            if (nalezeniSipky())
            {
                return;
            }
            else if (nalezeniTrojuhelniku())
            {
                return;
            }
            else
            {
                udalosti.Items.Add("-----------------------------------------");
                udalosti.Items.Add("Nebyl nalezen žádný objekt");
            }
        }

        #endregion

        #region tlačítka

        private void Obrazek_MouseMove(object sender, MouseEventArgs e)
        {
           info.Content = Mouse.GetPosition(Obrazek);
        }

        private void btnNacteniObrazku_Click(object sender, RoutedEventArgs e)
        {
            udalosti.Items.Clear();
            info.Content = "";
            
            nacteniObrazku();
            Obrazek.Source = obrazek;
        }

        private void BtnNalezeniTvaru_Click(object sender, RoutedEventArgs e)
        {
            hlavni();            
        }

        private void BtnDetekceRohu_Click(object sender, RoutedEventArgs e)
        {
            Detekce_rohu detekceRohu = new Detekce_rohu();
            obrazek = detekceRohu.detekce(obrazek, 50);
            obrazekI = prevodObrazku(obrazek);
            Obrazek.Source = obrazekI;
        }

        private void BtnKonvoluce_Click(object sender, RoutedEventArgs e)
        {            
            obrazekI = prevodObrazku(obrazek);

            Konvoluce kon = new Konvoluce();
            obrazek = kon.konvoluce(obrazekI);
            obrazekI = prevodObrazku(obrazek);
            Obrazek.Source = obrazekI;
        }

        private void BtnPrevodBarev_Click(object sender, RoutedEventArgs e)
        {
            obrazekI = prevodObrazku(obrazek);
            zmenaBarev();
            obrazekI = prevodObrazku(obrazek);
            Obrazek.Source = obrazekI;
        }

        private void BtnTvar_Click(object sender, RoutedEventArgs e)
        {
            odstraneniSamostatnychPixelu();

            nalezeniSouradniceX1();
            nalezeniSouradniceX2();
            nalezeniSouradniceY1();
            nalezeniSouradniceY2();

            if (nalezeniSipky())
            {
                return;
            }
            else if (nalezeniTrojuhelniku())
            {
                return;
            }
            else
            {
                udalosti.Items.Add("Nebyl nalezen žádný objekt");
            }
        }

        #endregion
    }
}
