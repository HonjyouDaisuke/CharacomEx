using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using MathNet.Numerics.Statistics;

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


        public BitmapSource TwoColorProc(BitmapSource src)
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
                    //System.Diagnostics.Debug.WriteLine(t.ToString() + ":" + d.ToString());
                    if (d > t) d = 255;
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
        /// 20201.08.08 D.Honjyou
        /// ビットマップを真っ白にする
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public BitmapSource BitmapWitening(BitmapSource src)
        {
            //BitmapをPbrga32に変換する
            FormatConvertedBitmap bitmap = new FormatConvertedBitmap(src, PixelFormats.Pbgra32, null, 0);

            //画像サイズの配列を作る
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int i, j, k;
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
            FormatConvertedBitmap s_bmp1 = new FormatConvertedBitmap(Src, PixelFormats.Pbgra32, null, 0);
            FormatConvertedBitmap s_bmp2 = new FormatConvertedBitmap(twoSrc, PixelFormats.Pbgra32, null, 0);
            FormatConvertedBitmap s_bmp3 = new FormatConvertedBitmap(SelectedArea, PixelFormats.Pbgra32, null, 0);

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
            int all_sum = 0;
            byte[] orgPixels = new byte[width * height * 4];

            //BitmapSourceから配列へコピー
            int stride = (width * bitmap.Format.BitsPerPixel + 7) / 8;
            bitmap.CopyPixels(orgPixels, stride, 0);

            int i, j, a, sum_a, sum_b;
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
                    if (ColorCompare(c, Colors.White) == false)
                    {
                        sum_a += i;
                        sum_b++;
                    }
                }
            }
            GravityP.X = (sum_b != 0) ? (double)((double)sum_a / (double)sum_b) : 0.0;

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
                    if (ColorCompare(c, Colors.White) == false)
                    {
                        sum_a += j;
                        sum_b++;
                    }
                }
            }
            GravityP.Y = (sum_b != 0) ? (double)((double)sum_a / (double)sum_b) : 0.0;
            System.Diagnostics.Debug.WriteLine($"重心座標 = {GravityP.X} , {GravityP.Y}");
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
                        outPixels[((j + y) * width + i + x) * 4 + 0] = srcPixels[(j * width + i) * 4 + 0];
                        outPixels[((j + y) * width + i + x) * 4 + 1] = srcPixels[(j * width + i) * 4 + 1];
                        outPixels[((j + y) * width + i + x) * 4 + 2] = srcPixels[(j * width + i) * 4 + 2];
                        outPixels[((j + y) * width + i + x) * 4 + 3] = srcPixels[(j * width + i) * 4 + 3];
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
            int i, j, k;
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
        private double TowMorment(BitmapSource bmp, DoublePoint GravPoint)
        {
            int i, j, sum;
            double r_ave;
            double sX, sY;
            //BitmapをPbrga32に変換する
            FormatConvertedBitmap bitmap = new FormatConvertedBitmap(bmp, PixelFormats.Pbgra32, null, 0);

            //画像サイズの配列を作る
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int k;
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
                        sX = (double)i - GravPoint.X;
                        sY = (double)j - GravPoint.Y;
                        r_ave = r_ave + Math.Sqrt(Math.Pow(sX, 2) + Math.Pow(sY, 2));
                    }
                }
            }

            r_ave = r_ave / (double)sum;
            return (r_ave);
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
            src = TwoColorProc(src);

            
            //大きさの正規化プロセス開始
            double r_ave, cXm, cYm, RRave;
            DoublePoint Gravi;
            //BitmapをPbrga32に変換する
            FormatConvertedBitmap bitmap = new FormatConvertedBitmap(src, PixelFormats.Pbgra32, null, 0);

            //画像サイズの配列を作る
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int i, j, k, x, y;
            byte r, g, b;
            Color c;
            byte[] orgPixels = new byte[width * height * 4];
            //byte[] outPixels = new byte[width * height * 4];
            byte[] outPixels = new byte[321 * 321 * 4];
            int width2, height2;
            //BitmapSourceから配列へコピー
            int stride = (width * bitmap.Format.BitsPerPixel + 7) / 8;
            int stride2 = (320 * bitmap.Format.BitsPerPixel + 7) / 8;
            bitmap.CopyPixels(orgPixels, stride, 0);
            int maxX, maxY, minX, minY;

            width2 = 320;
            height2 = 320;

            cXm = ((double)width / 2.0);
            cYm = ((double)height / 2.0);

            Gravi = GetGravityPointDouble(src);
            r_ave = TowMorment(src, Gravi);

            if (Double.IsNaN(r_ave))
            {
                return(src);
            }
            RRave = r_ave / R;


            
            for (j = 0; j < height2; j++)
            {
                for (i = 0; i < width2; i++)
                {
                    x = (int)(RRave * ((double)i - cXm) + Gravi.X);
                    y = (int)(RRave * ((double)j - cYm) + Gravi.Y);
                    if ((x >= 0) && (x < width) && (y >= 0) && (y < height))
                    {
                        r = orgPixels[(y * width + x) * 4 + 0];
                        g = orgPixels[(y * width + x) * 4 + 1];
                        b = orgPixels[(y * width + x) * 4 + 2];
                        c = Color.FromArgb(255, r, g, b);

                        if (ColorCompare(c, Colors.Black) == true)
                        {
                            outPixels[(j * width2 + i) * 4 + 0] = 0;
                            outPixels[(j * width2 + i) * 4 + 1] = 0;
                            outPixels[(j * width2 + i) * 4 + 2] = 0;
                            outPixels[(j * width2 + i) * 4 + 3] = 255;
                            //System.Diagnostics.Debug.WriteLine($"outpixcel {i},{j} =>Black");
                            //bmp.SetPixel(i, j, Color.Black);
                        }
                        else
                        {
                            outPixels[(j * width2 + i) * 4 + 0] = 255;
                            outPixels[(j * width2 + i) * 4 + 1] = 255;
                            outPixels[(j * width2 + i) * 4 + 2] = 255;
                            outPixels[(j * width2 + i) * 4 + 3] = 255;
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
            BitmapSource outBmp = BitmapSource.Create(width2, height2, 96, 96, PixelFormats.Pbgra32, null, outPixels, stride2);
            return (outBmp);
        }
        #endregion
    }
}