﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CharacomEx
{
    /// <summary>
    /// MainTabItemUserControl.xaml の相互作用ロジック
    /// </summary>
    public partial class MainTabItemUserControl : UserControl
    {
        private Image _mainImage;
        public Image MainImage { get => _mainImage; set => _mainImage = value; }
        private ImageEffect imgEffect = new ImageEffect();
        private double raito = 1.0;

        public MainTabItemUserControl()
        {
            InitializeComponent();
        }

        public void ClearRectView()
        {

            for (int i = 0; i < inkCanvas.Children.Count; i++)
            {
                if (inkCanvas.Children[i].GetType().ToString() == "System.Windows.Shapes.Line")
                {
                    inkCanvas.Children.RemoveAt(i);
                    i--;
                }
            }


            //inkCanvas.Children.GetType
            //inkCanvas.Children.Clear();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = new { MainImage = MainImage.Source };
        }

        private void inkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            var Oya = (MainWindow)Application.Current.MainWindow;

            // 元になる画像を画面から読み込む
            BitmapSource bmp = (BitmapSource)MainImage.Source;
            // 原画像を２値化する
            System.Diagnostics.Debug.WriteLine($"------閾値：{(int)Oya.SliderT.Value}");
            BitmapSource bmp2 = imgEffect.TwoColorProc(bmp, (int)Oya.SliderT.Value);

            System.Diagnostics.Debug.WriteLine($"<---------- width = {bmp.PixelWidth} , actual= {bmp.PixelHeight}");
            //var image = new RenderTargetBitmap((int)_project.MainImages[MainImageIndex].MainImage.ActualWidth, (int)_project.MainImages[MainImageIndex].MainImage.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            var image = new RenderTargetBitmap(bmp.PixelWidth, bmp.PixelHeight, 96, 96, PixelFormats.Pbgra32);
            image.Render(MainImage);

            Point min = new Point(9999.0, 9999.0);
            Point max = new Point(0.0, 0.0);
            Point bp = new Point(0, 0);

            // 元画像に色々書き込む
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext drawContent = dv.RenderOpen())
            {

                foreach (Point p in ((InkCanvas)sender).Strokes[0].StylusPoints)
                {
                    bp = p;
                    if (min.X > p.X) min.X = p.X;
                    if (min.Y > p.Y) min.Y = p.Y;
                    if (max.X < p.X) max.X = p.X;
                    if (max.Y < p.Y) max.Y = p.Y;
                }

                var streamGeometry = new StreamGeometry();
                using (var geometryContext = streamGeometry.Open())
                {
                    geometryContext.BeginFigure(((InkCanvas)sender).Strokes[0].StylusPoints[0].ToPoint(), true, false);
                    var points = new PointCollection();
                    for (var i = 1; i < ((InkCanvas)sender).Strokes[0].StylusPoints.Count; i++)
                    {
                        points.Add(((InkCanvas)sender).Strokes[0].StylusPoints[i].ToPoint());
                        // デバッグ出力
                        //System.Diagnostics.Debug.WriteLine(points[i-1].X.ToString() + "," + points[i-1].Y.ToString());
                    }
                    geometryContext.PolyLineTo(points, true, false);
                }

                drawContent.DrawGeometry(new SolidColorBrush(Colors.Blue), new Pen(Brushes.Blue, 1), streamGeometry);

            }

            // いろいろ書いたDrawingVisualを、RenderTargetBitmap(BitmapSourceの子クラス)に取り込む
            var bmp3 = new RenderTargetBitmap((int)image.Width, (int)image.Height, 96, 96, PixelFormats.Pbgra32);
            bmp3.Render(dv);
            Rect rect = new Rect(min, max);

            if (rect.Width < 5 || rect.Height < 5)
            {
                ((InkCanvas)sender).Strokes.Clear();
                return;
            }
            //var croppedBitmap = new CroppedBitmap(image, new Int32Rect((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height));
            var croppedBitmap = new CroppedBitmap(bmp, new Int32Rect((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height));
            var croppedBitmap2 = new CroppedBitmap(bmp2, new Int32Rect((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height));
            var croppedBitmap3 = new CroppedBitmap(bmp3, new Int32Rect((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height));
            bmp = imgEffect.ExtractionProc(croppedBitmap, croppedBitmap2, croppedBitmap3);

            ((InkCanvas)sender).Strokes.Clear();

            inkCanvas_DrawRectangle(rect);

            //2022.06.05 D.Honjyou
            //CachedBitmapになってしまったのを回避するためいったんバイナリに変換
            byte[] tmp = Oya.ToBinary((BitmapSource)bmp);
            bmp = Oya.ToBitmapSource(tmp);

            bmp = imgEffect.BitmapRotate(bmp, rotateTransform.Angle);
            System.Diagnostics.Debug.WriteLine($"回転角度＝{rotateTransform.Angle}");
            //2022.03.27 D.Honjyou
            //切り出した画像の保存サイズをメニューから引用
            int SaveSize = 0;
            if (Oya.SaveSizeS.IsChecked) SaveSize = 160;
            if (Oya.SaveSizeM.IsChecked) SaveSize = 320;
            if (Oya.SaveSizeL.IsChecked) SaveSize = 480;
            if (Oya.SaveSizeXL.IsChecked) SaveSize = 640;

            //切り出した矩形をメイン画像に追加
            //勝手に表示されるように作成
            CharaImageClass charaImage = new CharaImageClass();
            charaImage.CharaImageTitle = Oya.GetCharaName() + "_" + Oya.Project.MainImages[Oya.MainImageIndex].MainImageTitle + "-" + (GetNumOfChara(Oya.Project.MainImages[Oya.MainImageIndex], Oya.GetCharaName()) + 1).ToString("00");
            charaImage.CharaImageName = charaImage.CharaImageTitle;
            charaImage.CharaRect = rect;
            charaImage.CharaImage.Source = imgEffect.Normalize2(bmp, SaveSize, SaveSize);
            System.Diagnostics.Debug.WriteLine($"メニューの個別文字名 = {Oya.GetCharaName()}, 文字の個数={GetNumOfChara(Oya.Project.MainImages[Oya.MainImageIndex], Oya.GetCharaName())} ;; savesize={SaveSize}");
            System.Diagnostics.Debug.WriteLine(" ??? Height = " + charaImage.CharaRect.Height + "  Width = " + charaImage.CharaRect.Width);

            //2022.03.30 D.Honjyou
            //ListViewへの追加を一番下ではなく一番上に変更
            Oya.Project.MainImages[Oya.MainImageIndex].CharaImages.Insert(0, charaImage);

        }

        /// <summary>
        /// 文字列の末尾から指定した長さの文字列を取得する
        /// </summary>
        /// <param name="str">文字列</param>
        /// <param name="len">長さ</param>
        /// <returns>取得した文字列</returns>
        public static string Right(string str, int len)
        {
            if (len < 0)
            {
                throw new ArgumentException("引数'len'は0以上でなければなりません。");
            }
            if (str == null)
            {
                return "";
            }
            if (str.Length <= len)
            {
                return str;
            }
            return str.Substring(str.Length - len, len);
        }

        /// <summary>
        /// 2022.03.15 D.Honjyou
        /// プロジェクト内の個別文字を探す
        /// </summary>
        /// <param name="charaName">CharactorName</param>
        private int GetNumOfChara(MainImageClass m, string charaName)
        {
            int count = 0;

            foreach (CharaImageClass c in m.CharaImages)
            {
                if (c.CharaImageTitle.Contains("_"))
                {
                    System.Diagnostics.Debug.WriteLine($"検索文字名={charaName} ,タイトル={c.CharaImageTitle} ,{c.CharaImageTitle.IndexOf("_")}文字目までが名前 →　{c.CharaImageTitle.Substring(0, c.CharaImageTitle.IndexOf("_"))}");
                    if (c.CharaImageTitle.Substring(0, c.CharaImageTitle.IndexOf("_")) == charaName)
                    {
                        if (count < int.Parse(Right(c.CharaImageTitle, 2)))
                        {
                            count = int.Parse(Right(c.CharaImageTitle, 2));
                        }
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// 2021.07.29 D.Honjyou
        /// 赤い矩形を描く
        /// </summary>
        /// <param name="inkCanvas">InkCanvas</param>
        /// <param name="rect">矩形データ</param>
        private void inkCanvas_DrawRectangle(Rect rect)
        {
            RectangleGeometry rg = new RectangleGeometry();
            rg.Rect = rect;

            Path p = new Path();
            p.Stroke = Brushes.Red;
            p.StrokeThickness = 3;
            p.StrokeDashArray = new DoubleCollection { 2, 2 };
            p.Data = rg;

            

            inkCanvas.Children.Add(p);
            /**
            // 四角形をかく
            Line myLine1 = new Line();
            myLine1.Stroke = Brushes.Red;
            myLine1.X1 = rect.X;
            myLine1.X2 = rect.X + rect.Width;
            myLine1.Y1 = rect.Y;
            myLine1.Y2 = rect.Y;
            myLine1.HorizontalAlignment = HorizontalAlignment.Left;
            myLine1.VerticalAlignment = VerticalAlignment.Center;
            myLine1.StrokeThickness = 3;
            myLine1.StrokeDashArray = new DoubleCollection { 2, 2 };
            inkCanvas.Children.Add(myLine1);

            Line myLine2 = new Line();
            myLine2.Stroke = Brushes.Red;
            myLine2.X1 = rect.X;
            myLine2.X2 = rect.X + rect.Width;
            myLine2.Y1 = rect.Y + rect.Height;
            myLine2.Y2 = rect.Y + rect.Height;
            myLine2.StrokeThickness = 3;
            myLine2.StrokeDashArray = new DoubleCollection { 2, 2 };
            inkCanvas.Children.Add(myLine2);

            Line myLine3 = new Line();
            myLine3.Stroke = Brushes.Red;
            myLine3.X1 = rect.X;
            myLine3.X2 = rect.X;
            myLine3.Y1 = rect.Y;
            myLine3.Y2 = rect.Y + rect.Height;
            myLine3.StrokeThickness = 3;
            myLine3.StrokeDashArray = new DoubleCollection { 2, 2 };
            inkCanvas.Children.Add(myLine3);

            Line myLine4 = new Line();
            myLine4.Stroke = Brushes.Red;
            myLine4.X1 = rect.X + rect.Width;
            myLine4.X2 = rect.X + rect.Width;
            myLine4.Y1 = rect.Y;
            myLine4.Y2 = rect.Y + rect.Height;
            myLine4.StrokeThickness = 3;
            myLine4.StrokeDashArray = new DoubleCollection { 2, 2 };
            inkCanvas.Children.Add(myLine4);
            **/
        }

        public void RedrawRectangle()
        {
            ClearRectView();
            var Oya = (MainWindow)Application.Current.MainWindow;
            //2021.07.31 メインタブの画像に切り出し矩形を作成
            foreach (CharaImageClass c in Oya.Project.MainImages[Oya.MainImageIndex].CharaImages)
            {
                System.Diagnostics.Debug.WriteLine($"RedrawRect -- Chara:{c.CharaImageName}...");
                inkCanvas_DrawRectangle(c.CharaRect);
            }

        }

        public void DrawTargetRectangle(Rect rect)
        {
            int thin = 8;
            RectangleGeometry rg = new RectangleGeometry();
            rg.Rect = rect;

            Path p = new Path();
            p.Stroke = Brushes.Red;
            p.StrokeThickness = thin;
            p.Data = rg;

            inkCanvas.Children.Add(p);

            /**
            // 四角形をかく
            Line myLine1 = new Line();
            myLine1.Stroke = Brushes.Red;
            myLine1.X1 = rect.X;
            myLine1.X2 = rect.X + rect.Width;
            myLine1.Y1 = rect.Y;
            myLine1.Y2 = rect.Y;
            myLine1.HorizontalAlignment = HorizontalAlignment.Left;
            myLine1.VerticalAlignment = VerticalAlignment.Center;
            myLine1.StrokeThickness = thin;
            inkCanvas.Children.Add(myLine1);

            Line myLine2 = new Line();
            myLine2.Stroke = Brushes.Red;
            myLine2.X1 = rect.X;
            myLine2.X2 = rect.X + rect.Width;
            myLine2.Y1 = rect.Y + rect.Height;
            myLine2.Y2 = rect.Y + rect.Height;
            myLine2.StrokeThickness = thin;
            inkCanvas.Children.Add(myLine2);

            Line myLine3 = new Line();
            myLine3.Stroke = Brushes.Red;
            myLine3.X1 = rect.X;
            myLine3.X2 = rect.X;
            myLine3.Y1 = rect.Y;
            myLine3.Y2 = rect.Y + rect.Height;
            myLine3.StrokeThickness = thin;
            inkCanvas.Children.Add(myLine3);

            Line myLine4 = new Line();
            myLine4.Stroke = Brushes.Red;
            myLine4.X1 = rect.X + rect.Width;
            myLine4.X2 = rect.X + rect.Width;
            myLine4.Y1 = rect.Y;
            myLine4.Y2 = rect.Y + rect.Height;
            myLine4.StrokeThickness = thin;
            inkCanvas.Children.Add(myLine4);
            **/
        }

        void ViewCenter(double center_x, double center_y)
        {
            double sv_width_2 = scrollViewer.ActualWidth / 2;
            double sv_height_2 = scrollViewer.ActualHeight / 2;
            double offset_x = center_x - sv_width_2;
            double offset_y = center_y - sv_height_2;
            if (offset_x < 0) offset_x = 0;
            else if ((inkCanvas.ActualWidth - offset_x) < sv_width_2)
                offset_x = inkCanvas.ActualWidth - sv_width_2;
            if (offset_y < 0) offset_y = 0;
            else if ((inkCanvas.ActualHeight - offset_y) < sv_height_2)
                offset_y = inkCanvas.ActualHeight - sv_height_2;
            scrollViewer.ScrollToHorizontalOffset(offset_x);
            scrollViewer.ScrollToVerticalOffset(offset_y);
        }

        public void inkCanvas_ScaleChange(int s)
        {
            raito = (double)s * 0.01;
            Matrix m0 = new Matrix();
            //canvasサイズの変更 <=== *=raitoだったら、永遠に拡大し続けてしまう。。。。
            if (raito >= 1.0)
            {
                inkCanvas.Height = ImageDoc1.Height * raito;
                inkCanvas.Width = ImageDoc1.Width * raito;
            }
            System.Diagnostics.Debug.WriteLine($" canvas = ({inkCanvas.Width},{inkCanvas.Height}) ImageDoc1 = ({ImageDoc1.Width},{ImageDoc1.Height}) ratio = {raito} s = {s}");
            System.Diagnostics.Debug.WriteLine($"1 HOffset = {scrollViewer.HorizontalOffset} :: vOffset = {scrollViewer.VerticalOffset}");
            System.Diagnostics.Debug.WriteLine($"s viewW = {scrollViewer.ViewportWidth} :: view = {scrollViewer.ViewportHeight}");
            //センターの算出
            double cx, cy;
            cx = scrollViewer.HorizontalOffset + scrollViewer.ViewportWidth / 2.0;
            cy = scrollViewer.VerticalOffset + scrollViewer.ViewportHeight / 2.0;
            //ViewCenter(cx, cy);
            
            //canvasの拡大縮小
            //m0.Scale(raito, raito);
            m0.ScaleAt(raito, raito, cx, cy);
            matrixTransform.Matrix = m0;


            // scrollViewerのスクロールバーの位置をマウス位置を中心とする。
            //Point mousePoint = e.GetPosition(scrollViewer);
            //Double x_barOffset = scrollViewer.HorizontalOffset * raito;
            //scrollViewer.ScrollToHorizontalOffset(x_barOffset);

            //Double y_barOffset = scrollViewer.VerticalOffset * raito;
            //scrollViewer.ScrollToVerticalOffset(y_barOffset);
            //System.Diagnostics.Debug.WriteLine($"- x_barOffset = {x_barOffset} :: y_barOffset = {y_barOffset}");
            var Oya = (MainWindow)Application.Current.MainWindow;
            Oya.SetMagnification(raito);
            System.Diagnostics.Debug.WriteLine($"e viewW = {scrollViewer.ViewportWidth} :: view = {scrollViewer.ViewportHeight}");
            System.Diagnostics.Debug.WriteLine($"2 HOffset = {scrollViewer.HorizontalOffset} :: vOffset = {scrollViewer.VerticalOffset}");

        }
        /// <summary>
        /// 2022.03.16 D.Honjyou
        /// キャンバスの上でマウスをコロコロすると拡大縮小するようにする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void inkCanvas_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if((Keyboard.Modifiers & ModifierKeys.Control) <= 0)
            {
                return;
            }

            //後続のイベント処理をしない
            e.Handled = true;
            double pre_raito;
            pre_raito = raito;

            //マウスのホイールイベントを受け取り、スライダーをずらす
            Matrix m0 = new Matrix();
            if(e.Delta > 0)
            {
                raito += 0.1;
            }
            else
            {
                raito -= 0.1;
            }
            //拡大縮小が最大、最小の場合は受け付けない
            if(raito > 5.0 || raito < 0.2)
            {
                raito = pre_raito;
                return;
            }
            //canvasサイズの変更 <=== *=raitoだったら、永遠に拡大し続けてしまう。。。。
            if (raito >= 1.0)
            {
                inkCanvas.Height = ImageDoc1.Height * raito;
                inkCanvas.Width = ImageDoc1.Width * raito;
            }
            System.Diagnostics.Debug.WriteLine($" canvas = ({inkCanvas.Width},{inkCanvas.Height}) ImageDoc1 = ({ImageDoc1.Width},{ImageDoc1.Height}) ratio = {raito}");
            //センターの算出
            double cx, cy;
            cx = scrollViewer.HorizontalOffset + scrollViewer.ViewportWidth / 2.0;
            cy = scrollViewer.VerticalOffset + scrollViewer.ViewportHeight / 2.0;
            //canvasの拡大縮小
            //m0.Scale(raito, raito);
            m0.ScaleAt(raito, raito, cx, cy);
            matrixTransform.Matrix = m0;

            // scrollViewerのスクロールバーの位置をマウス位置を中心とする。
            //Point mousePoint = e.GetPosition(scrollViewer);
            //Double x_barOffset = (scrollViewer.HorizontalOffset + mousePoint.X) * raito - mousePoint.X;
            //scrollViewer.ScrollToHorizontalOffset(x_barOffset);

            //Double y_barOffset = (scrollViewer.VerticalOffset + mousePoint.Y) * raito - mousePoint.Y;
            //scrollViewer.ScrollToVerticalOffset(y_barOffset);
            var Oya = (MainWindow)Application.Current.MainWindow;
            Oya.SetMagnification(raito);
            
        }

        /// <summary>
        /// 20022.04.17 D.Honjyou
        /// 矩形がシングルクリックで選択されたときに中心に移動する。
        /// </summary>
        /// <param name="center"></param>
        public void SetCentering(Rect center)
        {
            // scrollViewerのスクロールバーの位置をマウス位置を中心とする。
            
            //Double x_barOffset = (center.Left + (center.Width / 2) - (scrollViewer.ActualWidth / 2)) * raito;
            //Double y_barOffset = (center.Top + (center.Height / 2) - (scrollViewer.ActualHeight /2)) * raito;
            Double x_barOffset = (center.Left + (center.Width / 2)) * raito - (scrollViewer.ActualWidth / 2);
            Double y_barOffset = (center.Top + (center.Height / 2)) * raito - (scrollViewer.ActualHeight / 2);

            System.Diagnostics.Debug.WriteLine($"ratio = {raito} ;;; screen = {scrollViewer.ActualWidth},{scrollViewer.ActualHeight} ::: Offset = {scrollViewer.HorizontalOffset}, {scrollViewer.VerticalOffset} :: point = {center.X} , {center.Y} ;; bar= {x_barOffset} , {y_barOffset}");

            //var Oya = (MainWindow)Application.Current.MainWindow;
            scrollViewer.ScrollToHorizontalOffset(x_barOffset);
            scrollViewer.ScrollToVerticalOffset(y_barOffset);
            //Oya.SetMagnification(raito);


        }
        private void ImageDoc1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            inkCanvas.Height = e.NewSize.Height;
            inkCanvas.Width = e.NewSize.Width;
        }

        /// <summary>
        /// メイン画像を回転させる
        /// </summary>
        /// <param name="LorR"></param>
        public void RotateImage(int LorR)
        {
            System.Diagnostics.Debug.WriteLine($"回転させます。{LorR}");

            rotateTransform.Angle += (90 * LorR);
            //double w, h;
            //w = inkCanvas.Width;
            //h = inkCanvas.Height;
            //inkCanvas.Height = w;
            //inkCanvas.Width = h;
            System.Diagnostics.Debug.WriteLine($"回転後のサイズ{inkCanvas.Width},{inkCanvas.Height}-{ImageDoc1.ActualWidth},{ImageDoc1.ActualHeight}");
        }
    }
}
