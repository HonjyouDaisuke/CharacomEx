using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using MathNet.Numerics.Statistics;
using System.IO;
using System.Windows;

namespace CharacomEx
{
    class ImageEffect
    {
        /// <summary>
		/// 2次元の点をあらわす構造体
		/// </summary>
		public struct DoublePoint
        {
            public double X; // x 座標
            public double Y; // y 座標

            public override string ToString()
            {
                return "(" + X + ", " + Y + ")";
            }
        }

        /// <summary>
        /// 2021.08.08 D.Honjyou
        /// Bitmapのコピーを作成
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public BitmapSource CopyBitmap(BitmapSource src)
        {
            //BitmapをPbrga32に変換する
            FormatConvertedBitmap bitmap = new FormatConvertedBitmap(src, PixelFormats.Pbgra32, null, 0);

            //画像サイズの配列を作る
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int i, j, k;
            byte[] orgPixels = new byte[width * height * 4];
            byte[] outPixels = new byte[width * height * 4];


            //BitmapSourceから配列へコピー
            int stride = (width * bitmap.Format.BitsPerPixel + 7) / 8;
            bitmap.CopyPixels(orgPixels, stride, 0);

            for (j = 1; j < height; j++)
            {
                for (i = 1; i < width; i++)
                {
                    for (k = 0; k < 4; k++)
                    {
                        outPixels[(j * width + i) * 4 + k] = orgPixels[(j * width + i) * 4 + k];
                    }
                }
            }

            //BitmapSourceに再変換
            BitmapSource outBmp = BitmapSource.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, orgPixels, stride);
            return (outBmp);
        }


        public BitmapSource TwoColorProc(BitmapSource src, int Threshold = 100)
        {
            //BitmapをPbgra32に変換する
            FormatConvertedBitmap bitmap = new FormatConvertedBitmap(src, PixelFormats.Pbgra32, null, 0);

            //画像サイズの配列を作る
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            byte[] orgPixels = new byte[width * height * 4];

            //BitmapSourceから配列へコピー
            int stride = (width * bitmap.Format.BitsPerPixel + 7) / 8;
            bitmap.CopyPixels(orgPixels, stride, 0);

            //モノクロ化
            int r, g, b;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    r = orgPixels[(y * (int)width + x) * 4 + 0];
                    g = orgPixels[(y * (int)width + x) * 4 + 1];
                    b = orgPixels[(y * (int)width + x) * 4 + 2];

                    double gray = r * 0.3 + g * 0.59 + b * 0.1;
                    orgPixels[(y * (int)width + x) * 4 + 0] = (byte)gray;
                    orgPixels[(y * (int)width + x) * 4 + 1] = (byte)gray;
                    orgPixels[(y * (int)width + x) * 4 + 2] = (byte)gray;

                }
            }

            //ヒストグラム作成
            int[] hist = new int[256];
            for (int i = 0; i < 256; i++) hist[i] = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    hist[orgPixels[(y * width + x) * 4 + 1]]++;
                }
            }

            //判別分析法
            int t = 0;
            double max = 0.0;
            for (int i = 0; i < 256; i++)
            {
                int w1 = 0;
                int w2 = 0;
                long sum1 = 0;
                long sum2 = 0;
                double m1 = 0.0;
                double m2 = 0.0;

                for (int j = 0; j <= i; j++)
                {
                    w1 += hist[j];
                    sum1 += j * hist[j];
                }

                for (int j = i + 1; j < 256; ++j)
                {
                    w2 += hist[j];
                    sum2 += j * hist[j];
                }

                if (w1 > 0) m1 = (double)sum1 / w1;
                if (w2 > 0) m2 = (double)sum2 / w2;
                double tmp = ((double)w1 * w2 * (m1 - m2) * (m1 - m2));

                if (tmp > max)
                {
                    max = tmp;
                    t = i;
                }
            }

            // tで2値化
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int d = (int)orgPixels[(y * (int)width + x) * 4 + 0];
                    if (d > t * (double)((double)Threshold / 100.0)) d = 255;
                    else d = 0;
                    orgPixels[(y * width + x) * 4 + 0] = (byte)d;
                    orgPixels[(y * width + x) * 4 + 1] = (byte)d;
                    orgPixels[(y * width + x) * 4 + 2] = (byte)d;
                }
            }

            //BitmapSourceに再変換
            BitmapSource outBmp = BitmapSource.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, orgPixels, stride);
            return (outBmp);
        }

        // ノイズ除去
        public BitmapSource Noize(BitmapSource src)
        {
            //BitmapをPbrga32に変換する
            FormatConvertedBitmap bitmap = new FormatConvertedBitmap(src, PixelFormats.Pbgra32, null, 0);

            //画像サイズの配列を作る
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int all_sum = 0;
            byte[] orgPixels = new byte[width * height * 4];

            //BitmapSourceから配列へコピー
            int stride = (width * bitmap.Format.BitsPerPixel + 7) / 8;
            bitmap.CopyPixels(orgPixels, stride, 0);

            int j, k, l, m, sum;
            int a, b, c, d, e;

            for (j = 1; j < height - 1; j++)
            {
                for (k = 1; k < width - 1; k++)
                {
                    a = (orgPixels[(j * width + k) * 4 + 0] == 255) ? 0 : 1;
                    b = (orgPixels[((j - 1) * width + k) * 4 + 0] == 255) ? 0 : 1;
                    c = (orgPixels[((j + 1) * width + k) * 4 + 0] == 255) ? 0 : 1;
                    d = (orgPixels[(j * width + (k - 1)) * 4 + 0] == 255) ? 0 : 1;
                    e = (orgPixels[(j * width + (k + 1)) * 4 + 0] == 255) ? 0 : 1;
                    if (a == 1 && (b + c + d + e) == 0)
                    {
                        orgPixels[(j * width + k) * 4 + 0] = 255;
                        orgPixels[(j * width + k) * 4 + 1] = 255;
                        orgPixels[(j * width + k) * 4 + 2] = 255;
                        all_sum++;
                    }
                }
            }

            //メディアンフィルタ
            var data = new double[9];
            for (j = 1; j < height - 1; j++)
            {
                for (k = 1; k < width - 1; k++)
                {
                    data[0] = orgPixels[((j - 1) * width + (k - 1)) * 4 + 0];
                    data[1] = orgPixels[((j - 1) * width + (k - 0)) * 4 + 0];
                    data[2] = orgPixels[((j - 1) * width + (k + 1)) * 4 + 0];
                    data[3] = orgPixels[((j - 0) * width + (k - 1)) * 4 + 0];
                    data[4] = orgPixels[((j - 0) * width + (k - 0)) * 4 + 0];
                    data[5] = orgPixels[((j - 0) * width + (k + 1)) * 4 + 0];
                    data[6] = orgPixels[((j + 1) * width + (k - 1)) * 4 + 0];
                    data[7] = orgPixels[((j + 1) * width + (k - 0)) * 4 + 0];
                    data[8] = orgPixels[((j + 1) * width + (k + 1)) * 4 + 0];
                    byte medi = (byte)data.Median();
                    orgPixels[(j * width + k) * 4 + 0] = medi;
                    orgPixels[(j * width + k) * 4 + 1] = medi;
                    orgPixels[(j * width + k) * 4 + 2] = medi;

                }
            }

            System.Diagnostics.Debug.WriteLine("pre除去数:" + all_sum.ToString());
            for (j = 2; j < height - 2; j += 5)
            {
                for (k = 2; k < width - 2; k += 5)
                {
                    if (orgPixels[(j * width + k) * 4 + 0] != 255)
                    {
                        sum = 0; a = 0; b = 0; c = 0; d = 0;

                        for (l = (-2); l < 3; l++)
                        {
                            for (m = (-2); m < 3; m++)
                            {
                                sum += (orgPixels[((j - l) * width + (k + m)) * 4 + 0] == 255) ? 0 : 1;
                            }
                        }
                        for (l = (-2); l < 3; l++)
                        {
                            c += (orgPixels[((j + l) * width + (k - 2)) * 4 + 0] == 255) ? 0 : 1;
                            d += (orgPixels[((j + l) * width + (k + 2)) * 4 + 0] == 255) ? 0 : 1;
                        }
                        for (m = (-2); m < 3; m++)
                        {
                            a += (orgPixels[((j - 2) * width + (k + m)) * 4 + 0] == 255) ? 0 : 1;
                            b += (orgPixels[((j + 2) * width + (k + m)) * 4 + 0] == 255) ? 0 : 1;
                        }
                        if (sum < 10 && a == 0 && b == 0 && c == 0 && d == 0)
                        {
                            for (l = (-2); l < 3; l++)
                            {
                                for (m = (-2); m < 3; m++)
                                {
                                    orgPixels[((j + l) * width + (k + m)) * 4 + 0] = 255;
                                    orgPixels[((j + l) * width + (k + m)) * 4 + 1] = 255;
                                    orgPixels[((j + l) * width + (k + m)) * 4 + 2] = 255;
                                    all_sum++;
                                }
                            }
                        }
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine("除去数:" + all_sum.ToString());
            //BitmapSourceに再変換
            BitmapSource outBmp = BitmapSource.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, orgPixels, stride);
            return (outBmp);
        }

        /// <summary>
        /// BitmapSourceをbyte[]型へ変換
        /// </summary>
        /// <param name="bs"></param>
        /// <returns></returns>
        public byte[] ToBinary(BitmapSource img)
        {
            if (img == null) return null;

            //BitmapSource bmp = (BitmapSource)img;
            //img.Source = bmp;

            // BitmapSourceの派生クラス「RenderTargetBitmap」で、画像を取ってくる
            //System.Diagnostics.Debug.WriteLine("->with = " + bmp.PixelWidth.ToString() + ",Height = " + bmp.PixelHeight.ToString());
            //System.Diagnostics.Debug.WriteLine("+>with = " + img.Width.ToString() + ",Height = " + img.Height.ToString());
            //var canvas = new RenderTargetBitmap((int)bmp.PixelWidth, (int)bmp.PixelHeight, 96, 96, PixelFormats.Pbgra32);
            //canvas.Render(img); // canvasに画像を描画

            using (var ms = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(img));
                encoder.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return ms.GetBuffer();

            }

        }

        /// <summary>
        /// byte[]型をBitmapSourceへ変換
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        public BitmapSource ToBitmapSource(byte[] byteArray)
        {
            using (var ms = new MemoryStream(byteArray))
            {
                var image = new BitmapImage();

                ms.Seek(0, SeekOrigin.Begin);
                image.BeginInit();
                //当初はアイコンのみを扱うつもりだったが様々なサイズの画像を扱うためコメントアウト
                //image.DecodePixelHeight = 16;
                //image.DecodePixelWidth = 16;

                image.CacheOption = BitmapCacheOption.OnLoad;
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();

                return image;
            }
        }

        /// <summary>
        /// 2021.08.25 D,Honjyou
        /// ビットマップの比較同じならTrueをかえす。
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool BitmapCompare(BitmapSource a, BitmapSource b)
        {
            //BitmapをPbrga32に変換する
            FormatConvertedBitmap bmp_a = new FormatConvertedBitmap(a, PixelFormats.Pbgra32, null, 0);
            FormatConvertedBitmap bmp_b = new FormatConvertedBitmap(b, PixelFormats.Pbgra32, null, 0);

            //画像サイズの配列を作る
            int width_a = a.PixelWidth;
            int height_a = a.PixelHeight;
            int width_b = b.PixelWidth;
            int height_b = b.PixelHeight;

            int i;
            byte[] pix_a = new byte[width_a * height_a * 4];
            byte[] pix_b = new byte[width_b * height_b * 4];

            //BitmapSourceから配列へコピー
            int s_a = (width_a * bmp_a.Format.BitsPerPixel + 7) / 8;
            int s_b = (width_b * bmp_b.Format.BitsPerPixel + 7) / 8;
            bmp_a.CopyPixels(pix_a, s_a, 0);
            bmp_b.CopyPixels(pix_b, s_b, 0);

            bool bRet = true;

            for (i = 0; i < pix_a.Length; i++)
            {
                if (pix_a[i] != pix_b[i]) bRet = false;
            }
            return bRet;
        }

        /// <summary>
        /// 20201.08.08 D.Honjyou
        /// ビットマップを真っ白にする
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public BitmapSource BitmapWhitening(BitmapSource src)
        {
            //BitmapをPbrga32に変換する
            FormatConvertedBitmap bitmap = new FormatConvertedBitmap(src, PixelFormats.Pbgra32, null, 0);
            
            //画像サイズの配列を作る
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int i, j;
            byte[] orgPixels = new byte[width * height * 4];

            //BitmapSourceから配列へコピー
            int stride = (width * bitmap.Format.BitsPerPixel + 7) / 8;
            bitmap.CopyPixels(orgPixels, stride, 0);

            for (j = 1; j < height; j++)
            {
                for (i = 1; i < width; i++)
                {
                    orgPixels[(j * width + i) * 4 + 0] = 255;
                    orgPixels[(j * width + i) * 4 + 1] = 255;
                    orgPixels[(j * width + i) * 4 + 2] = 255;
                }
            }

            
            //BitmapSourceに再変換
            BitmapSource outBmp = BitmapSource.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, orgPixels, stride);
            return (outBmp);
        }

        //現画像から２値化画像をもとに抽出
        public BitmapSource ExtractionProc(BitmapSource Src, BitmapSource twoSrc, BitmapSource SelectedArea)
        {
            //BitmapをPbrga32に変換する
            FormatConvertedBitmap s_bmp1 = new FormatConvertedBitmap(Src, PixelFormats.Pbgra32, null, 0);           //原画像
            FormatConvertedBitmap s_bmp2 = new FormatConvertedBitmap(twoSrc, PixelFormats.Pbgra32, null, 0);        //2値画像
            FormatConvertedBitmap s_bmp3 = new FormatConvertedBitmap(SelectedArea, PixelFormats.Pbgra32, null, 0);  //矩形エリア

            //画像サイズの配列を作る
            int width = s_bmp1.PixelWidth;
            int height = s_bmp1.PixelHeight;
            byte[] orgPixels1 = new byte[width * height * 4];
            byte[] orgPixels2 = new byte[width * height * 4];
            byte[] orgPixels3 = new byte[width * height * 4];

            //BitmapSourceから配列へコピー
            int stride = (width * s_bmp1.Format.BitsPerPixel + 7) / 8;
            int stride3 = (width * s_bmp3.Format.BitsPerPixel + 7) / 8;
            s_bmp1.CopyPixels(orgPixels1, stride, 0);
            s_bmp2.CopyPixels(orgPixels2, stride, 0);
            s_bmp3.CopyPixels(orgPixels3, stride3, 0);

            int base_index;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    base_index = (y * (int)width + x) * 4;
                    if ((orgPixels2[base_index + 0] != 0 || orgPixels2[base_index + 1] != 0 || orgPixels2[base_index + 2] != 0) || (orgPixels3[base_index + 0] != 255 || orgPixels3[base_index + 1] != 0 || orgPixels3[base_index + 2] != 0))
                    {
                        orgPixels1[(y * (int)width + x) * 4 + 0] = 255;
                        orgPixels1[(y * (int)width + x) * 4 + 1] = 255;
                        orgPixels1[(y * (int)width + x) * 4 + 2] = 255;
                    }
                    

                }
            }
            BitmapSource outBmp;
            outBmp = BitmapSource.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, orgPixels1, stride);

            return (outBmp);
        }

        //陰影除去プロセス
        public BitmapSource IneiProc(BitmapSource src)
        {
            //BitmapをPbrga32に変換する
            FormatConvertedBitmap bitmap = new FormatConvertedBitmap(src, PixelFormats.Pbgra32, null, 0);

            //画像サイズの配列を作る
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            byte[] orgPixels = new byte[width * height * 4];
            byte[] orgPixels2 = new byte[width * height * 4];

            //BitmapSourceから配列へコピー
            int stride = (width * bitmap.Format.BitsPerPixel + 7) / 8;
            bitmap.CopyPixels(orgPixels, stride, 0);
            bitmap.CopyPixels(orgPixels2, stride, 0);

            //陰影チェック
            double r, g, b;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool check = false;
                    double p = 0.3;
                    double t = 1.0 + p;
                    double m = 1.0 - p;
                    r = (double)orgPixels[(y * (int)width + x) * 4 + 0];
                    g = (double)orgPixels[(y * (int)width + x) * 4 + 1];
                    b = (double)orgPixels[(y * (int)width + x) * 4 + 2];

                    //System.Diagnostics.Debug.WriteLine("((" + r.ToString() + "<" + (b * m).ToString() + ") && ("+ r.ToString() + "<" +(b * t).ToString()+")) || (("+r.ToString()+ ">"+ (g * m).ToString()+") && (" +r.ToString() + "<" +(g * t).ToString() + "))");
                    if (((g < r * m) || (g > r * t)) || ((b < r * m) || (b > r * t))) check = true;
                    if (((b < g * m) || (b > g * t)) || ((r < g * m) || (r > g * t))) check = true;
                    if (((r < b * m) || (r > b * t)) || ((g < b * m) || (g > b * t))) check = true;

                    if (check)
                    {
                        //System.Diagnostics.Debug.WriteLine("Inei!!");
                        orgPixels[(y * (int)width + x) * 4 + 0] = (byte)0;
                        orgPixels[(y * (int)width + x) * 4 + 1] = (byte)0;
                        orgPixels[(y * (int)width + x) * 4 + 2] = (byte)0;
                    }
                    else
                    {
                        orgPixels[(y * (int)width + x) * 4 + 0] = (byte)255;
                        orgPixels[(y * (int)width + x) * 4 + 1] = (byte)255;
                        orgPixels[(y * (int)width + x) * 4 + 2] = (byte)255;
                        //br = orgPixels[(y * (int)width + x) * 4 + 0];
                        //bg = orgPixels[(y * (int)width + x) * 4 + 1];
                        //bb = orgPixels[(y * (int)width + x) * 4 + 2];

                    }


                }
            }

            //BitmapSourceに再変換
            BitmapSource outBmp = BitmapSource.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, orgPixels, stride);
            //return (outBmp);
            outBmp = Noize(outBmp);

            //BitmapをPbrga32に変換する
            bitmap = new FormatConvertedBitmap(outBmp, PixelFormats.Pbgra32, null, 0);

            //BitmapSourceから配列へコピー
            stride = (width * bitmap.Format.BitsPerPixel + 7) / 8;
            bitmap.CopyPixels(orgPixels, stride, 0);
            for (int y = 20; y < height - 10; y++)
            {
                for (int x = 20; x < width - 10; x++)
                {
                    if (orgPixels[(y * (int)width + x) * 4] == 0)
                    {
                        r = 0;
                        g = 0;
                        b = 0;

                        for (int j = -10; j < 10; j++)
                        {
                            for (int i = -10; i < 10; i++)
                            {
                                r += orgPixels2[((y - 20 + j) * width + (x + i - 20)) * 4 + 0];
                                g += orgPixels2[((y - 20 + j) * width + (x + i - 20)) * 4 + 1];
                                b += orgPixels2[((y - 20 + j) * width + (x + i - 20)) * 4 + 2];
                            }

                        }

                        //System.Diagnostics.Debug.WriteLine("Inei!!");
                        orgPixels2[(y * (int)width + x) * 4 + 0] = (byte)(r / 400);
                        orgPixels2[(y * (int)width + x) * 4 + 1] = (byte)(g / 400);
                        orgPixels2[(y * (int)width + x) * 4 + 2] = (byte)(b / 400);
                    }

                }
            }

            //BitmapSourceに再変換
            outBmp = BitmapSource.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, orgPixels2, stride);

            return (outBmp);
        }


        #region 色を比較する
        /// <summary>
        /// 2021.08.08 D.Honjyou
        /// 色の比較　色Aと色Bが同じならTrue
        /// </summary>
        /// <param name="A">色A</param>
        /// <param name="B">色B</param>
        /// <returns>同じであればTrueを返す</returns>
        public bool ColorCompare(Color A, Color B)
        {
            bool iRet = false;

            if (A.R == B.R && A.G == B.G && A.B == B.B/** && A.A == B.A*/)
            {
                iRet = true;
            }

            return (iRet);
        }
        #endregion


        /// <summary>
        /// 2021.08.08 D.Honjyou
        /// 重心の座標を得る
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public DoublePoint GetGravityPointDouble(BitmapSource src)
        {
            //BitmapをPbrga32に変換する
            FormatConvertedBitmap bitmap = new FormatConvertedBitmap(src, PixelFormats.Pbgra32, null, 0);

            //画像サイズの配列を作る
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            //int all_sum = 0;
            byte[] orgPixels = new byte[width * height * 4];

            //BitmapSourceから配列へコピー
            int stride = (width * bitmap.Format.BitsPerPixel + 7) / 8;
            bitmap.CopyPixels(orgPixels, stride, 0);

            int i, j, sum_a, sum_b;
            DoublePoint GravityP = new DoublePoint();
            Color c;
            byte r, g, b;

            // x軸方向の重心
            sum_a = 0; sum_b = 0;
            for (i = 0; i < width; i++)
            {
                for (j = 0; j < height; j++)
                {
                    r = orgPixels[(j * width + i) * 4 + 0];
                    g = orgPixels[(j * width + i) * 4 + 1];
                    b = orgPixels[(j * width + i) * 4 + 2];
                    c = Color.FromRgb(r, g, b);
                    if (ColorCompare(c, Colors.White) == false && orgPixels[(j * width + i) * 4 + 3] != 0) 
                    {
                        sum_a += i;
                        sum_b++;
                    }
                }
            }
            GravityP.X = (sum_b != 0) ? sum_a / sum_b : 0.0;

            // y軸方向の重心
            sum_a = 0; sum_b = 0;
            for (i = 0; i < width; i++)
            {
                for (j = 0; j < height; j++)
                {
                    r = orgPixels[(j * width + i) * 4 + 0];
                    g = orgPixels[(j * width + i) * 4 + 1];
                    b = orgPixels[(j * width + i) * 4 + 2];
                    c = Color.FromRgb(r, g, b);
                    if (ColorCompare(c, Colors.White) == false && orgPixels[(j * width + i) * 4 + 3] != 0)
                    {
                        sum_a += j;
                        sum_b++;
                    }
                }
            }
            GravityP.Y = (sum_b != 0) ? sum_a / sum_b : 0.0;
            System.Diagnostics.Debug.WriteLine($"キャンバスサイズ = {src.Width} , {src.Height} 重心座標 = {GravityP.X} , {GravityP.Y}");
            return (GravityP);
        }

        /// <summary>
        /// 2021.08.08 D.Honjyou
        /// 元画像の位置を変更
        /// </summary>
        /// <param name="src">元画像</param>
        /// <param name="x">移動先のX座標</param>
        /// <param name="y">移動先のY座標</param>
        /// <returns></returns>
        public BitmapSource PutBitmap_ToPoint(BitmapSource src, int x, int y)
        {
            //BitmapをPbrga32に変換する
            FormatConvertedBitmap bitmapSrc = new FormatConvertedBitmap(src, PixelFormats.Pbgra32, null, 0);
            
            //画像サイズの配列を作る
            int width = bitmapSrc.PixelWidth;
            int height = bitmapSrc.PixelHeight;
            int i, j, k;
            byte[] srcPixels = new byte[width * height * 4];
            byte[] outPixels = new byte[width * height * 4];
            byte a;
            //BitmapSourceから配列へコピー
            int stride = (width * bitmapSrc.Format.BitsPerPixel + 7) / 8;
            bitmapSrc.CopyPixels(srcPixels, stride, 0);


            for (j = 1; j < height; j++)
            {
                for (i = 1; i < width; i++)
                {
                    for (k = 0; k < 4; k++)
                    {
                        outPixels[(j * width + i) * 4 + k] = 255;
                    }
                }
            }


            for (j = 1; j < height; j++)
            {
                for (i = 1; i < width; i++)
                {
                    if((j + y) < height && (j + y) >= 0 && (i + x) < width && (i + x) >= 0)
                    {
                        a = srcPixels[(j * width + i) * 4 + 3];
                        if(a == 0) {
                            outPixels[((j + y) * width + i + x) * 4 + 0] = 255;
                            outPixels[((j + y) * width + i + x) * 4 + 1] = 255;
                            outPixels[((j + y) * width + i + x) * 4 + 2] = 255;
                            outPixels[((j + y) * width + i + x) * 4 + 3] = 255;
                        }
                        else
                        {
                            outPixels[((j + y) * width + i + x) * 4 + 0] = srcPixels[(j * width + i) * 4 + 0];
                            outPixels[((j + y) * width + i + x) * 4 + 1] = srcPixels[(j * width + i) * 4 + 1];
                            outPixels[((j + y) * width + i + x) * 4 + 2] = srcPixels[(j * width + i) * 4 + 2];
                            outPixels[((j + y) * width + i + x) * 4 + 3] = srcPixels[(j * width + i) * 4 + 3];
                        }
                        
                    }
                }
            }


            //BitmapSourceに再変換
            BitmapSource outBmp = BitmapSource.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, outPixels, stride);
            return (outBmp);
        }

        /// <summary>
        /// 2021.08.07 D.Honjyou
        /// 重心を中心に移動
        /// </summary>
        /// <param name="src">ソース画像</param>
        /// <returns>移動後の画像(BitmapSource)</returns>
        public BitmapSource MoveCenter_fromGravity(BitmapSource src)
        {
            //重心座標を取得→gpに格納
            DoublePoint gp = GetGravityPointDouble(src);
            int x, y;

            x = (int)((src.PixelWidth / 2.0) - gp.X);
            y = (int)((src.PixelHeight / 2.0) - gp.Y);

            System.Diagnostics.Debug.WriteLine($"MoveGravity  size={src.PixelWidth},{src.PixelHeight}, gravity={gp.X},{gp.Y}, start = {x},{y}");
            src = PutBitmap_ToPoint(src, x, y);
            
            return (src);
        }

        #region 真っ白かどうかのチェック （戻り値：true→何か入っている, false→真っ白）
        /// <summary>
        /// 真っ白かどうかのチェック
        /// 戻り値(bool)：true⇒何か入っている、false⇒真っ白
        /// </summary>
        /// <param name="src">チェックする画像(Bitmap)</param>
        /// <returns>true⇒何か入っている、false⇒真っ白</returns>
        public bool WhiteCanvasCheck(BitmapSource src)
        {
            bool bRet = false;
            //BitmapをPbrga32に変換する
            FormatConvertedBitmap bitmap = new FormatConvertedBitmap(src, PixelFormats.Pbgra32, null, 0);

            //画像サイズの配列を作る
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int i, j;
            byte r, g, b;
            Color c;
            byte[] orgPixels = new byte[width * height * 4];
            //BitmapSourceから配列へコピー
            int stride = (width * bitmap.Format.BitsPerPixel + 7) / 8;
            bitmap.CopyPixels(orgPixels, stride, 0);

            bRet = false;
            for (j = 0; j < height; j++)
            {
                for (i = 0; i < width; i++)
                {
                    r = orgPixels[(j * width + i) * 4 + 0];
                    g = orgPixels[(j * width + i) * 4 + 1];
                    b = orgPixels[(j * width + i) * 4 + 2];
                    c = Color.FromRgb(r, g, b);

                    if (ColorCompare(c, Colors.White) != true)
                    {
                        bRet = true;
                    }
                }
            }

            return bRet;
        }
        #endregion

        #region ２次モーメントの算出
        /// <summary>
        /// 2021.08.09 D.Honjyou
        /// 二次モーメントの算出
        /// </summary>
        /// <param name="bmp">ソース画像</param>
        /// <param name="GravPoint">重視座標</param>
        /// <returns></returns>
        private double TwoMorment(BitmapSource bmp, DoublePoint GravPoint)
        {
            int i, j, sum;
            double r_ave;
            double sX, sY;
            //BitmapをPbrga32に変換する
            FormatConvertedBitmap bitmap = new FormatConvertedBitmap(bmp, PixelFormats.Pbgra32, null, 0);

            //画像サイズの配列を作る
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            byte r, g, b;
            Color c;
            byte[] orgPixels = new byte[width * height * 4];
            //BitmapSourceから配列へコピー
            int stride = (width * bitmap.Format.BitsPerPixel + 7) / 8;
            bitmap.CopyPixels(orgPixels, stride, 0);

            sum = 0;
            r_ave = 0.0;

            for (j = 0; j < height; j++)
            {
                for (i = 0; i < width; i++)
                {
                    r = orgPixels[(j * width + i) * 4 + 0];
                    g = orgPixels[(j * width + i) * 4 + 1];
                    b = orgPixels[(j * width + i) * 4 + 2];
                    c = Color.FromRgb(r, g, b);

                    if (ColorCompare(c, Colors.White) != true)
                    {
                        sum++;
                        sX = i - GravPoint.X;
                        sY = j - GravPoint.Y;
                        r_ave += Math.Sqrt(Math.Pow(sX, 2) + Math.Pow(sY, 2));
                    }
                }
            }

            r_ave /= sum;
            return (r_ave);
        }
        #endregion

        #region 画素割合の算出
        /// <summary>
        /// 2021.08.14 D.Honjyou
        /// 画素割合の抽出
        /// </summary>
        /// <param name="bmp">ソース画像</param>
        /// <returns></returns>
        public double GetPixelRate(BitmapSource bmp)
        {
            int i, j, sum;
            double r_ave;
            int x1, x2, y1, y2;
            //BitmapをPbrga32に変換する
            FormatConvertedBitmap bitmap = new FormatConvertedBitmap(bmp, PixelFormats.Pbgra32, null, 0);

            //画像サイズの配列を作る
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            byte r, g, b, a;
            Color c;
            byte[] orgPixels = new byte[width * height * 4];
            //BitmapSourceから配列へコピー
            int stride = (width * bitmap.Format.BitsPerPixel + 7) / 8;
            bitmap.CopyPixels(orgPixels, stride, 0);

            sum = 0;
            x1 = 9999;
            x2 = 0;
            y1 = 9999;
            y2 = 0;
            for (j = 0; j < height; j++)
            {
                for (i = 0; i < width; i++)
                {
                    r = orgPixels[(j * width + i) * 4 + 0];
                    g = orgPixels[(j * width + i) * 4 + 1];
                    b = orgPixels[(j * width + i) * 4 + 2];
                    a = orgPixels[(j * width + i) * 4 + 3];
                    c = Color.FromRgb(r, g, b);

                    if (ColorCompare(c, Colors.White) != true && a != 0)
                    {
                        sum++;
                        if (x1 > i) x1 = i;
                        if (x2 < i) x2 = i;
                        if (y1 > j) y1 = j;
                        if (y2 < j) y2 = j;
                    }
                }
            }
            r_ave = (double)sum / (double)((x2 - x1) * (y2 - y1));
            System.Diagnostics.Debug.WriteLine($"sum={sum} {r_ave}={sum}/{width} * {height}");
            return (r_ave);
        }
        #endregion

        #region 縦横比の算出
        /// <summary>
        /// 2021.08.14 D.Honjyou
        /// 縦横比の算出
        /// </summary>
        /// <param name="bmp">ソース画像</param>
        /// <returns></returns>
        public double GetAspectRatio(BitmapSource bmp)
        {
            int i, j;
            int x1, x2, y1, y2;
            double Aratio;
            //BitmapをPbrga32に変換する
            FormatConvertedBitmap bitmap = new FormatConvertedBitmap(bmp, PixelFormats.Pbgra32, null, 0);

            //画像サイズの配列を作る
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            byte r, g, b, a;
            Color c;
            byte[] orgPixels = new byte[width * height * 4];
            //BitmapSourceから配列へコピー
            int stride = (width * bitmap.Format.BitsPerPixel + 7) / 8;
            bitmap.CopyPixels(orgPixels, stride, 0);

            x1 = 9999;
            x2 = 0;
            y1 = 9999;
            y2 = 0;
            for (j = 0; j < height; j++)
            {
                for (i = 0; i < width; i++)
                {
                    r = orgPixels[(j * width + i) * 4 + 0];
                    g = orgPixels[(j * width + i) * 4 + 1];
                    b = orgPixels[(j * width + i) * 4 + 2];
                    a = orgPixels[(j * width + i) * 4 + 3];
                    c = Color.FromRgb(r, g, b);

                    if (ColorCompare(c, Colors.White) != true && a != 0)
                    {
                        if (x1 > i) x1 = i;
                        if (x2 < i) x2 = i;
                        if (y1 > j) y1 = j;
                        if (y2 < j) y2 = j;     
                    }
                }
            }
            Aratio = (double)(x2 - x1) / (double)(y2 - y1);
            System.Diagnostics.Debug.WriteLine($"縦横比={Aratio} = {x2-x1} / {y2-y1}");
            return (Aratio);
        }
        #endregion

        #region 正方形にコピー
        public BitmapSource CopyToWhiteSquare(BitmapSource src, BitmapSource inputBmp)
        {
            int i, j;
            //BitmapをPbrga32に変換する
            //FormatConvertedBitmap Bitmap1 = new FormatConvertedBitmap(src, PixelFormats.Pbgra32, null, 0);
            //FormatConvertedBitmap Bitmap2 = new FormatConvertedBitmap(inputBmp, PixelFormats.Pbgra32, null, 0);

            //画像サイズの配列を作る
            int width1 = src.PixelWidth;
            int height1 = src.PixelHeight;
            int width2 = inputBmp.PixelWidth;
            int height2 = inputBmp.PixelHeight;

            byte[] Pixels1 = new byte[width1 * height1 * 4];
            byte[] Pixels2 = new byte[width2 * height2 * 4];
            //BitmapSourceから配列へコピー
            int stride1 = (width1 * src.Format.BitsPerPixel + 7) / 8;
            int stride2 = (width2 * inputBmp.Format.BitsPerPixel + 7) / 8;
            src.CopyPixels(Pixels1, stride1, 0);
            inputBmp.CopyPixels(Pixels2, stride2, 0);

            for (j = 1; j < height1; j++)
            {
                for (i = 1; i < width1; i++)
                {
                    if (height2 > j && width2 > i)
                    {
                        Pixels1[(j * width1 + i) * 4 + 0] = Pixels2[(j * width2 + i) * 4 + 0];
                        Pixels1[(j * width1 + i) * 4 + 1] = Pixels2[(j * width2 + i) * 4 + 1];
                        Pixels1[(j * width1 + i) * 4 + 2] = Pixels2[(j * width2 + i) * 4 + 2];
                        Pixels1[(j * width1 + i) * 4 + 3] = Pixels2[(j * width2 + i) * 4 + 3];
                    }
                    else
                    {
                        Pixels1[(j * width1 + i) * 4 + 0] = 255;
                        Pixels1[(j * width1 + i) * 4 + 1] = 255;
                        Pixels1[(j * width1 + i) * 4 + 2] = 255;
                        Pixels1[(j * width1 + i) * 4 + 3] = 255;
                    }


                }
            }


            //BitmapSourceに再変換
            width1 = 160;
            height1 = 160;
            BitmapSource outBmp = BitmapSource.Create(width1, height1, 96, 96, PixelFormats.Pbgra32, null, Pixels1, stride1);
            return outBmp;
        }
        #endregion
        public BitmapSource CopyToWhite(BitmapSource src, BitmapSource inputBmp)
        {
            int i, j;
            int startX, startY;
            int inPos, outPos;

            startX = 0;
            startY = 0;
            System.Diagnostics.Debug.WriteLine($"input = ({inputBmp.PixelWidth} , {inputBmp.PixelHeight})");
            //画像サイズの配列を作る
            int width1 = src.PixelWidth;
            int height1 = src.PixelHeight;
            int width2 = inputBmp.PixelWidth;
            int height2 = inputBmp.PixelHeight;
            //中心に来るようにスタート位置を調整
            //2022.03.14 D.Honjyou
            if (width2 > height2)
            {
                //元画像が横長の場合
                startY = (height1 - height2) / 2;
            }
            else
            {
                //元画像が縦長の場合
                startX = (width1 - width2) / 2;
            }
            System.Diagnostics.Debug.WriteLine($"Start = ({startX} , {startY})");

            byte[] Pixels1 = new byte[width1 * height1 * 4];
            byte[] Pixels2 = new byte[width2 * height2 * 4];
            //BitmapSourceから配列へコピー
            int stride1 = (width1 * src.Format.BitsPerPixel + 7) / 8;
            int stride2 = (width2 * inputBmp.Format.BitsPerPixel + 7) / 8;
            src.CopyPixels(Pixels1, stride1, 0);
            inputBmp.CopyPixels(Pixels2, stride2, 0);

            for (j = 0; j < height1; j++)
            {
                for (i = 0; i < width1; i++)
                {
                    outPos = (j  * width1 + i) * 4;

                    Pixels1[outPos + 0] = 255;
                    Pixels1[outPos + 1] = 255;
                    Pixels1[outPos + 2] = 255;
                    Pixels1[outPos + 3] = 255;
                }
            }


            for (j = 0; j < height2; j++)
            {
                for (i = 0; i < width2; i++)
                {
                    if ( j < height1 && i < width1)
                    {
                        inPos = (j * width2 + i) * 4;
                        outPos = ((j + startY) * width1 + startX + i) * 4;

                        Pixels1[outPos + 0] = Pixels2[inPos + 0];
                        Pixels1[outPos + 1] = Pixels2[inPos + 1];
                        Pixels1[outPos + 2] = Pixels2[inPos + 2];
                        Pixels1[outPos + 3] = Pixels2[inPos + 3];
                    }
                    
                   
                }
            }


            //BitmapSourceに再変換
            BitmapSource outBmp = BitmapSource.Create(width1, height1, 96, 96, PixelFormats.Pbgra32, null, Pixels1, stride1);
            return outBmp;
        }
        #region 大きさ正規化その２
        public BitmapSource Normalize2(BitmapSource src, int outWidth, int outHeight)
        {
            double aratio = GetAspectRatio(src);
            double scale;

            if(aratio < 1.0)
            {
                scale = (double)(outHeight / src.Height);
            }
            else
            {
                scale = (double)(outWidth / src.Width);
            }
            System.Diagnostics.Debug.WriteLine($" !!Normalize2!! aratio = {aratio} Height = {src.Height}  Width = {src.Width}  ==> Scale = {scale}");
            var transformedBitmap = new TransformedBitmap(src, new ScaleTransform(scale, scale));

            PixelFormat pf = PixelFormats.Bgr32;
            int rawStride = (outWidth * pf.BitsPerPixel + 7) / 8;
            byte[] rawImage = new byte[rawStride * outHeight];

            // Initialize the image with data.
            Random value = new Random();
            value.NextBytes(rawImage);

            // Create a BitmapSource.
            BitmapSource outbmp = BitmapSource.Create(outWidth, outHeight, 96, 96, pf, null, rawImage, rawStride);
            outbmp = CopyToWhite(outbmp, transformedBitmap);
            //CroppedBitmap cb = new CroppedBitmap(transformedBitmap, new Int32Rect(0, 0, outWidth, outHeight));
            return (BitmapSource)outbmp;

        }
        #endregion

        #region 大きさの正規化
        /// <summary>
        /// 2021.08.08 D.Honjyou
        /// 大きさの正規化
        /// </summary>
        /// <param name="src"></param>
        /// <param name="R"></param>
        /// <returns></returns>
        public BitmapSource Normalize(BitmapSource src, double R)
        {
            //double R = ((double)input.Height / 4.0) * 0.95;
            System.Diagnostics.Debug.WriteLine("Normalized");
            //キャンバスが真っ白だったらすぐ終了
            if (WhiteCanvasCheck(src) == false)
            {
                System.Diagnostics.Debug.WriteLine("キャンバスが真っ白のため何もしていません。");
                return(src);
            }

            /**
            BitmapSource tmp;
            tmp = TwoColorProc(src);
            **/
            
            //大きさの正規化プロセス開始
            double r_ave, cXm, cYm, RRave;
            DoublePoint Gravi;
            //BitmapをPbrga32に変換する
            FormatConvertedBitmap bitmap = new FormatConvertedBitmap(src, PixelFormats.Pbgra32, null, 0);
            
            
            //画像サイズの配列を作る
            long width = bitmap.PixelWidth;
            long height = bitmap.PixelHeight;
            int i, j, x, y;
            byte[] orgPixels = new byte[width * height * 4];
            byte[] tmpPixels = new byte[321 * 321 * 4];
            byte[] srctmpPixels = new byte[321 * 321 * 4];
            byte[] outPixels = new byte[321 * 321 * 4];
            int width2, height2;
            //BitmapSourceから配列へコピー
            long stride = (width * bitmap.Format.BitsPerPixel + 7) / 8;
            long stride2 = (320 * bitmap.Format.BitsPerPixel + 7) / 8;
            System.Diagnostics.Debug.WriteLine($"long = {stride} int = {(int)stride}");
            bitmap.CopyPixels(orgPixels, (int)stride, 0);

            //実験
            bitmap.CopyPixels(tmpPixels, (int)stride2, 0);
            bitmap.CopyPixels(srctmpPixels, (int)stride2, 0);
            BitmapSource ttt = BitmapSource.Create(320, 320, 96, 96, PixelFormats.Pbgra32, null, tmpPixels, (int)stride2);
            BitmapSource src_ttt = BitmapSource.Create(320, 320, 96, 96, PixelFormats.Pbgra32, null, srctmpPixels, (int)stride2);
            System.Diagnostics.Debug.WriteLine($"WhiteCanvas = {ttt.Width} * {ttt.Height} : r={tmpPixels[256800 + 0]} g={tmpPixels[256800 + 1]} b={tmpPixels[256800 + 2]} a={tmpPixels[256800 + 3 ]}");
            ttt = MoveCenter_fromGravity(ttt);
            ttt = TwoColorProc(ttt);
            src_ttt = MoveCenter_fromGravity(src_ttt);
            
            width2 = 320;
            height2 = 320;

            cXm = ((double)width2 / 2.0);
            cYm = ((double)height2 / 2.0);

            System.Diagnostics.Debug.WriteLine("---------");
            Gravi = GetGravityPointDouble(ttt);
            System.Diagnostics.Debug.WriteLine($"     tttキャンバスサイズ = {ttt.Width} , {ttt.Height} 重心座標 = {Gravi.X} , {Gravi.Y}");

            r_ave = TwoMorment(ttt, Gravi);

            if (Double.IsNaN(r_ave))
            {
                return(src);
            }
            RRave = r_ave / R;


            ttt.CopyPixels(tmpPixels, (int)stride2, 0);
            src_ttt.CopyPixels(srctmpPixels, (int)stride2, 0);
            for (j = 0; j < height2; j++)
            {
                for (i = 0; i < width2; i++)
                {
                    x = (int)(RRave * (i - cXm) + Gravi.X);
                    y = (int)(RRave * (j - cYm) + Gravi.Y);
                    
                    if ((x >= 0) && (x < width2) && (y >= 0) && (y < height2))
                    {
                        if(tmpPixels[(y * width2 + x) * 4 + 3] == 0)
                        {
                            outPixels[(j * width2 + i) * 4 + 0] = 255;
                            outPixels[(j * width2 + i) * 4 + 1] = 255;
                            outPixels[(j * width2 + i) * 4 + 2] = 255;
                            outPixels[(j * width2 + i) * 4 + 3] = 255;
                        }
                        else
                        {
                            outPixels[(j * width2 + i) * 4 + 0] = srctmpPixels[(y * width2 + x) * 4 + 0];
                            outPixels[(j * width2 + i) * 4 + 1] = srctmpPixels[(y * width2 + x) * 4 + 1];
                            outPixels[(j * width2 + i) * 4 + 2] = srctmpPixels[(y * width2 + x) * 4 + 2];
                            outPixels[(j * width2 + i) * 4 + 3] = srctmpPixels[(y * width2 + x) * 4 + 3];

                        }


                    }
                    else
                    {
                        outPixels[(j * width2 + i) * 4 + 0] = 255;
                        outPixels[(j * width2 + i) * 4 + 1] = 255;
                        outPixels[(j * width2 + i) * 4 + 2] = 255;
                        outPixels[(j * width2 + i) * 4 + 3] = 255;
                    }
                }
            }

            //BitmapSourceに再変換
            BitmapSource outBmp = BitmapSource.Create(width2, height2, 96, 96, PixelFormats.Pbgra32, null, outPixels, (int)stride2);
            //BitmapSource outBmp = BitmapSource.Create(width2, height2, 96, 96, PixelFormats.Pbgra32, null, tmpPixels, stride2);
            return (outBmp);
        }
        #endregion

        #region 特徴抽出(加重方向指数ヒストグラム特徴)
        private byte[,] GetArrayFromBmp(BitmapSource bmp)
        {
            int i, j;
            byte[,] output = new byte[(int)bmp.Height, (int)bmp.Width];
            //BitmapをPbrga32に変換する
            FormatConvertedBitmap bitmap = new FormatConvertedBitmap(bmp, PixelFormats.Pbgra32, null, 0);

            //画像サイズの配列を作る
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            byte r, g, b, a;
            Color c;
            byte[] orgPixels = new byte[width * height * 4];
            //BitmapSourceから配列へコピー
            int stride = (width * bitmap.Format.BitsPerPixel + 7) / 8;
            bitmap.CopyPixels(orgPixels, stride, 0);

            for (j = 0; j < bmp.Height; j++)
            {
                for (i = 0; i < bmp.Width; i++)
                {
                    r = orgPixels[(j * width + i) * 4 + 0];
                    g = orgPixels[(j * width + i) * 4 + 1];
                    b = orgPixels[(j * width + i) * 4 + 2];
                    a = orgPixels[(j * width + i) * 4 + 3];
                    c = Color.FromRgb(r, g, b);

                    if (ColorCompare(c, Colors.White) != true && a != 0) output[j, i] = 1;
                    else output[j, i] = 0;
                }
            }
            return (output);
        }

        void dataSyoki(byte[] data)
        {
            int i;

            for (i = 0; i < 160 * 20; i++)
            {
                data[i] = 0;
            }
        }
        void data2Syoki(byte[,,] data2, int w, int h)
        {
            int i, j, k;

            for (k = 0; k < 16; k++)
            {
                for (j = 0; j < h; j++)
                {
                    for (i = 0; i < w; i++)
                    {
                        data2[k, j, i] = 0;
                    }
                }
            }
        }

        void data3Syoki(byte[,,] data3)
        {
            int i, j, k;

            for (k = 0; k < 16; k++)
            {
                for (j = 0; j < 16; j++)
                {
                    for (i = 0; i < 16; i++)
                    {
                        data3[k, j, i] = 0;
                    }
                }
            }
        }
        void data4Syoki(double[,,] data4)
        {
            int i, j, k;

            for (k = 0; k < 16; k++)
            {
                for (j = 0; j < 8; j++)
                {
                    for (i = 0; i < 8; i++)
                    {
                        data4[k, j, i] = 0.0;
                    }
                }
            }
        }
        void data5Syoki(double[,,] data5)
        {
            int i, j, k;

            for (k = 0; k < 8; k++)
            {
                for (j = 0; j < 8; j++)
                {
                    for (i = 0; i < 8; i++)
                    {
                        data5[k, j, i] = 0.0;
                    }
                }
            }
        }
        void data6Syoki(double[,,] data6)
        {
            int i, j, k;

            for (k = 0; k < 4; k++)
            {
                for (j = 0; j < 8; j++)
                {
                    for (i = 0; i < 8; i++)
                    {
                        data6[k, j, i] = 0.0;
                    }
                }
            }
        }

        void syokika(byte[,] dat3, byte[,] dat2, int w, int h)
        {
            int i, j;

            for (i = 0; i < h + 2; i++)
            {
                for (j = 0; j < w + 2; j++)
                {
                    dat3[i, j] = 0;
                }
            }
            for (i = 0; i < h; i++)
            {
                for (j = 0; j < w; j++)
                {
                    dat3[i + 1, j + 1] = dat2[i, j];
                }
            }
        }

        void tuiseki2(byte[,] dat3, byte[,,] data2, int ap, int bp, int x, int y, int bx, int by, int sc)
        {
            int i, j, data;
            int[] px = { -1, -1, 0, 1, 1, 1, 0, -1 };
            int[] py = { 0, 1, 1, 1, 0, -1, -1, -1 };
            bp = 0; ap = 0;
            for (i = 0; i < 8; i++)
            {
                if (px[i] == bx - x && py[i] == by - y)
                {
                    sc = i + 1;
                }
            }
            for (i = 0; i < 8; i++)
            {
                if (px[i] == x - bx && py[i] == y - by)
                {
                    bp = (i + 4) % 8;
                }
            }
            for (i = 0; i < 8; i++)
            {
                if (0 != dat3[y + py[(sc + i) % 8], x + px[(sc + i) % 8]])
                {
                    dat3[y + py[(sc + i) % 8], x + px[(sc + i) % 8]] = 7;
                    by = y; bx = x;
                    y += py[(sc + i) % 8]; x += px[(sc + i) % 8];
                    for (j = 0; j < 8; j++)
                    {
                        if (px[j] == bx - x && py[j] == by - y)
                        {
                            ap = j;
                            data = bp + ap;
                            data2[data, by - 1, bx - 1]++;
                        }
                    }
                    break;
                }
            }
        }
        void tuiseki(byte[,] dat3, byte[,,] data2, int ap, int bp, int x, int y, int n, int num)
        {
            int i, j, data, sx, sy, sc, bx, by, check, sum;
            int[] px = { -1, -1, 0, 1, 1, 1, 0, -1 };
            int[] py = { 0, 1, 1, 1, 0, -1, -1, -1 };
            sx = x; sy = y;
            bx = 0; by = 0;
            bp = 10;
            sum = 0;
            sc = num;
            check = 0;
            do
            {
                if (check != 0)
                {
                    for (i = 0; i < 8; i++)
                    {
                        if (px[i] == bx - x && py[i] == by - y)
                        {
                            sc = i + 1;
                        }
                    }
                    for (i = 0; i < 8; i++)
                    {
                        if (px[i] == x - bx && py[i] == y - by)
                        {
                            bp = (i + 4) % 8;
                        }
                    }
                }
                check = 1;
                for (i = 0; i < 8; i++)
                {
                    if (0 != dat3[y + py[(sc + i) % 8], x + px[(sc + i) % 8]])
                    {
                        dat3[y + py[(sc + i) % 8], x + px[(sc + i) % 8]] = 7;
                        by = y; bx = x;
                        y += py[(sc + i) % 8]; x += px[(sc + i) % 8];
                        for (j = 0; j < 8; j++)
                        {
                            if (px[j] == bx - x && py[j] == by - y)
                            {
                                ap = j;
                                data = bp + ap;
                                if (sum == 1) data2[data, by - 1, bx - 1]++;
                                sum = 1;

                            }
                        }
                        break;
                    }
                }
            } while (x != sx || y != sy);
            x = sx; y = sy;
            tuiseki2(dat3, data2, ap, bp, x, y, bx, by, sc);
        }

        void rasta(byte[,] dat3, byte[,,] data2, int w, int h, int ap, int bp)
        {
            int i, j, n = 3;
            for (i = 0; i < h + 1; i++)
            {
                for (j = 0; j < w + 1; j++)
                {
                    if (dat3[i, j] == 0 && dat3[i, j + 1] == 1)
                    {
                        dat3[i, j + 1] = 7;
                        tuiseki(dat3, data2, ap, bp, j + 1, i, n, 0);
                    }
                    if (dat3[i, j] == 1 && dat3[i, j + 1] == 0)
                    {
                        dat3[i, j] = 7;
                        tuiseki(dat3, data2, ap, bp, j, i, n, 4);
                    }
                }
            }
            for (j = h + 1; j > 0; j--)
            {
                for (i = w + 1; i > 0; i--)
                {
                    if (dat3[j, i] == 0 && dat3[j, i - 1] == 1)
                    {
                        dat3[j, i - 1] = 7;
                        tuiseki(dat3, data2, ap, bp, j, i - 1, n, 2);
                    }
                    if (dat3[j, i] == 1 && dat3[j, i - 1] == 0)
                    {
                        dat3[j, i] = 7;
                        tuiseki(dat3, data2, ap, bp, j, i, n, 6);
                    }
                }
            }
        }

        void ryousika(byte[,,] data2, byte[,,] data3, int w, int h)
        {
            int i, j, k, m, n;

            for (i = 0; i < 16; i++)
            {
                for (j = 0; j < 16; j++)
                {
                    for (k = 0; k < 16; k++)
                    {
                        for (m = 0; m < h / 16; m++)
                        {
                            for (n = 0; n < w / 16; n++)
                            {
                                data3[i, j, k] += data2[i, (10 * j) + m, (10 * k) + n];
                            }
                        }
                    }
                }
            }
        }

        void gaus_fil<Type>(Type[,,] data3, double[,,] data4)
        {
            int m, n, i, j, k;
            double sum = 0.0;
            //Type work;
            double[,] gaus ={{0.00,0.09,0.17,0.09,0.00},
                             {0.09,0.57,1.05,0.57,0.09},
                             {0.17,1.05,1.94,1.05,0.17},
                             {0.09,0.57,1.05,0.57,0.09},
                             {0.00,0.09,0.17,0.09,0.00}};
            System.Diagnostics.Debug.WriteLine(data4.GetLength(0).ToString() + "," + data4.GetLength(1).ToString() + "," + data4.GetLength(2).ToString());
            for (i = 0; i < data4.GetLength(0); i++)
            {
                for (j = 0; j < 8; j++)
                {
                    for (k = 0; k < 8; k++)
                    {
                        sum = 0.0;
                        for (m = 0; m < 5; m++)
                        {
                            for (n = 0; n < 5; n++)
                            {
                                if (2 * j + m - 2 > 0 && 2 * j + m - 2 < 16 && 2 * k + n - 2 > 0 && 2 * k + n - 2 < 16)
                                {
                                    //work = data3[i,2*j+m-2,2*k+n-2];
                                    //sum+=data3[i,2*j+m-2,2*k+n-2]*gaus[m,n];
                                    sum += Convert.ToDouble(data3[i, 2 * j + m - 2, 2 * k + n - 2]) * gaus[m, n];
                                }
                            }
                        }
                        data4[i, j, k] = sum;
                    }
                }
            }
        }

        void houkou(double[,,] data4, double[,,] data5)
        {
            int i, j, k;

            for (i = 0; i < 16; i += 2)
            {
                for (j = 0; j < 8; j++)
                {
                    for (k = 0; k < 8; k++)
                    {
                        if (i == 0) data5[i / 2, j, k] = data4[15, j, k] + data4[0, j, k] * 2 + data4[1, j, k];
                        else data5[i / 2, j, k] = data4[i - 1, j, k] + data4[i, j, k] * 2 + data4[i + 1, j, k];
                    }
                }
            }
        }

        void douitusi(double[,,] data5, double[,,] data6)
        {
            int i, j, k;

            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    for (k = 0; k < 8; k++)
                    {
                        if (i > 3) data6[i - 4, j, k] += data5[i, j, k];
                        else data6[i, j, k] += data5[i, j, k];
                    }
                }
            }
        }

        void kajyu_data(double[,,] data6, double[] nyuuryoku)
        {
            int i, j, k;
            for (i = 0; i < 256; i++)
            {
                nyuuryoku[i] = 0;
            }
            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    for (k = 0; k < 8; k++)
                    {
                        nyuuryoku[i * 64 + j * 8 + k] = data6[i, j, k];
                    }
                }
            }
        }

        public void GetKajyu(BitmapSource bmp, double[] outDat)
        {
            int j, m, n;
            byte[,] data1 = new byte[(int)bmp.Height, (int)bmp.Width];
            byte[,] dat3 = new byte[(int)bmp.Height + 2, (int)bmp.Width + 2];
            byte[,,] data2 = new byte[16, (int)bmp.Height, (int)bmp.Width];
            byte[,,] data3 = new byte[16, 16, 16];
            double[,,] data4 = new double[16, 8, 8];
            double[,,] data5 = new double[8, 8, 8];
            double[,,] data6 = new double[4, 8, 8];
            //double[] outDat = new double[4];
            double maxd, mind, work;
            double R1 = 10.0;
            int ap, bp;

            ap = 0; bp = 0;
            Noize(bmp);
            //Normalize(bmp, R);
            data1 = GetArrayFromBmp(bmp);
            data2Syoki(data2, (int)bmp.Width, (int)bmp.Height);
            data3Syoki(data3);
            data4Syoki(data4);
            data5Syoki(data5);
            data6Syoki(data6);

            syokika(dat3, data1, (int)bmp.Width, (int)bmp.Height); //
            rasta(dat3, data2, (int)bmp.Width, (int)bmp.Height, ap, bp);
            ryousika(data2, data3, (int)bmp.Width, (int)bmp.Height);
            gaus_fil(data3, data4);
            houkou(data4, data5);
            douitusi(data5, data6);

            //kajyu_data(data6, kajyu);
            maxd = 0.0;
            mind = 10000.0;
            for (j = 0; j < 4; j++)
            {
                for (m = 0; m < 8; m++)
                {
                    for (n = 0; n < 8; n++)
                    {
                        if (maxd < data6[j, m, n]) maxd = data6[j, m, n];
                        if (mind > data6[j, m, n]) mind = data6[j, m, n];
                    }
                }
            }
            work = (maxd - mind) / R1;
            for (j = 0; j < 4; j++)
            {
                for (m = 0; m < 8; m++)
                {
                    for (n = 0; n < 8; n++)
                    {
                        data6[j, m, n] = data6[j, m, n] / work;
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine("-----------------------------------------------------------");
            for (j = 0; j < 4; j++)
            {
                outDat[j] = 0.0;
                for (m = 0; m < 8; m++)
                {
                    for (n = 0; n < 8; n++)
                    {
                        //System.Diagnostics.Debug.Write( $"{data6[j, m, n]},");
                        outDat[j] += data6[j, m, n];
                    }
                    
                }
                outDat[j] /= 64;
                System.Diagnostics.Debug.WriteLine(outDat[j].ToString());
            }
            //kajyu_data(data6, kajyuView);
        }
        #endregion
    }
}