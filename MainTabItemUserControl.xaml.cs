using System;
using System.Collections.Generic;
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

        public MainTabItemUserControl()
        {
            InitializeComponent();
        }
        
        public void ClearRectView()
        {

            for(int i = 0; i < inkCanvas.Children.Count; i++)
            {
                if(inkCanvas.Children[i].GetType().ToString() == "System.Windows.Shapes.Line")
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
           
            // 元になる画像を画面から読み込む
            BitmapSource bmp = (BitmapSource)MainImage.Source;
            // 原画像を２値化する
            BitmapSource bmp2 = imgEffect.TwoColorProc(bmp);

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

            var Oya = (MainWindow)Application.Current.MainWindow;

           

            //切り出した矩形をメイン画像に追加
            //勝手に表示されるように作成
            CharaImageClass charaImage = new CharaImageClass();
            charaImage.CharaImageTitle = Oya.GetCharaName() + "-" + Oya.Project.MainImages[Oya.MainImageIndex].MainImageTitle + "-" + (GetNumOfChara(Oya.Project.MainImages[Oya.MainImageIndex], Oya.GetCharaName()) + 1).ToString("00");
            charaImage.CharaImageName = charaImage.CharaImageTitle;
            charaImage.CharaRect = rect;
            charaImage.CharaImage.Source = imgEffect.Normalize2(bmp, 160, 160);
            System.Diagnostics.Debug.WriteLine($"メニューの個別文字名 = {Oya.GetCharaName()}, 文字の個数={GetNumOfChara(Oya.Project.MainImages[Oya.MainImageIndex], Oya.GetCharaName())}");
            System.Diagnostics.Debug.WriteLine(" ??? Height = " + charaImage.CharaRect.Height + "  Width = " + charaImage.CharaRect.Width);

            (Oya.Project.MainImages[Oya.MainImageIndex]).CharaImages.Add(charaImage);

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

            foreach(CharaImageClass c in m.CharaImages)
            {
                if(c.CharaImageTitle.Substring(0,1) == charaName)
                {
                    if (count < int.Parse(Right(c.CharaImageTitle, 2)))
                    {
                        count = int.Parse(Right(c.CharaImageTitle, 2));
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
        private void inkCanvas_DrawRectangle( Rect rect)
        {
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
    }
}
