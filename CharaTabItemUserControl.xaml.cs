using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Forms.DataVisualization.Charting;


namespace CharacomEx
{
    /// <summary>
    /// CharaTabItemUserControl.xaml の相互作用ロジック
    /// </summary>
    public partial class CharaTabItemUserControl : UserControl
    {
        private double _penSize;
        private Color _penColor;
        private string _charaImageName;
        public double PenSize { get => _penSize; set => _penSize = value; }
        public Color PenColor { get => _penColor; set => _penColor = value; }
        private ImageEffect imageEffect = new ImageEffect();
        private BitmapSource charaSrcImage;

        //private int sIndex;
        /// <summary>
        /// MetroWindow
        /// </summary>
        public MetroWindow Metro { get; set; } = System.Windows.Application.Current.MainWindow as MetroWindow;
        public string CharaImageName { get => _charaImageName; set => _charaImageName = value; }
        public BitmapSource CharaSrcImage { get => charaSrcImage; set => charaSrcImage = value; }

        public CharaTabItemUserControl(BitmapSource img)
        {
            InitializeComponent();

            //sIndex = 3;
            //ペンサイズと色のデフォルト値を設定
            _penSize = 1;
            _penColor = Colors.Black;

            //消しゴムボタン等の初期値を設定
            PenToggle.IsChecked = true;
            EraserToggle.IsChecked = false;
            PenSizeCombo.SelectedIndex = 2;
            colColorPicker.SelectedColor = Colors.Black;
            
            charaSrcImage = img;
            charaInkCanvas.DataContext = CharaSrcImage;
            charaInkCanvas.Width = charaSrcImage.PixelWidth;
            charaInkCanvas.Height = charaSrcImage.PixelHeight;
            System.Diagnostics.Debug.WriteLine($"Src w = {charaInkCanvas.Width}   h = {charaInkCanvas.Height}");
            //ソース画像を表示
            //imgCharaSrc.Source = (BitmapSource)CharaSrcImage;


            //InitializeChart();
        }

        private void PenToggle_Checked(object sender, RoutedEventArgs e)
        {
            EraserToggle.IsChecked = false;
            SetPenAttributes();
            //await Metro.ShowMessageAsync("This is the title", "Some message");
        }

        private void EraserToggle_Checked(object sender, RoutedEventArgs e)
        {
            PenToggle.IsChecked = false;
            SetPenAttributes();
        }


        private void charaInkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {

            // レンダリングオブジェクトの描画先を作成する
            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();

            // InkCanvas 原寸大の描画「領域(rectangle)」を定義する
            var rect = new Rect(0, 0, this.charaInkCanvas.ActualWidth, this.charaInkCanvas.ActualHeight);
            // レンダリングオブジェクトに、背景画像を描画する
            drawingContext.DrawRectangle(this.charaInkCanvas.Background, null, rect);
            // レンダリングオブジェクトに、追加でストローク情報を描画する
            this.charaInkCanvas.Strokes.Draw(drawingContext);
            // レンダリングオブジェクト情報をフラッシュする
            drawingContext.Close();

            // 描画先をビットマップにする(96dpi)
            var renderTargetBitmap = new RenderTargetBitmap(
              (int)rect.Width, (int)rect.Height, 96d, 96d, PixelFormats.Default
            );
            renderTargetBitmap.Render(drawingVisual);

            var Oya = (MainWindow)Application.Current.MainWindow;
            Oya.Project.MainImages[Oya.MainImageIndex].CharaImages[Oya.CharaImageIndex].CharaImage.Source = renderTargetBitmap;
        }

        /// <summary>
        /// 2021.08.02 SetPenAttributes
        /// ペンサイズと色の変更（消しゴム判定付き）
        /// </summary>
        private void SetPenAttributes()
        {
            DrawingAttributes drawingAttributes = new DrawingAttributes();
            DrawingAttributes da = drawingAttributes;

            da.Width = _penSize;
            da.Height = _penSize;
            if ((bool)EraserToggle.IsChecked) da.Color = Colors.White;
            if ((bool)PenToggle.IsChecked) da.Color = _penColor;

            charaInkCanvas.DefaultDrawingAttributes = da;
        }
        /// <summary>
        /// 2021.08.01 ペンの太さ選択コンボボックス
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PenSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PenSizeCombo.SelectedIndex == 0) _penSize = 10;
            if (PenSizeCombo.SelectedIndex == 1) _penSize = 5;
            if (PenSizeCombo.SelectedIndex == 2) _penSize = 1;

            SetPenAttributes();

        }
        /// <summary>
        /// 2021.08.02 カラー選択(ColorPicker)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                _penColor = colColorPicker.SelectedColor.Value;
                SetPenAttributes();
                colColorPicker.IsDropDownOpen = false;
            }
        }

        /// <summary>
        /// 2021.08.04 D.Honjyou
        /// キャンバス上でマウスホイールをコロコロしたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void charaInkCanvas_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // コントロールキーを押していなかったら終わり
            if ((Keyboard.Modifiers & ModifierKeys.Control) <= 0)
            {
                return;
            }


            // 後続のイベントを実行しないための処理
            e.Handled = true;

            //  マウスホイールのイベントを受け取り、スライダーをずらす
            var index = ImageRate.Ticks.IndexOf(ImageRate.Value);
            if (0 < e.Delta)
            {
                index += 1;
            }
            else
            {
                index -= 1;
            }

            if (index < 0)
            {
                return;
            }
            else if (ImageRate.Ticks.Count <= index)
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine("コロコロ->" + index);

            Double scale = ImageRate.Ticks[index] / ImageRate.Value;
            ImageRate.Value = ImageRate.Ticks[index];
            lblScale.Text = ImageRate.Ticks[index].ToString() + "%";

            // canvasサイズの変更
            charaInkCanvas.Height *= scale;
            charaInkCanvas.Width *= scale;
            //imgCharaSrc.Height *= scale;
            //imgCharaSrc.Width *= scale;

            // canvasの拡大縮小
            Matrix m0 = new Matrix();
            m0.Scale(ImageRate.Value * 0.01, ImageRate.Value * 0.01);//元のサイズとの比
            //matrixTransform.Matrix = m0;

            // scrollViewerのスクロールバーの位置をマウス位置を中心とする。
            Point mousePoint = e.GetPosition(scrollViewer);
            Double x_barOffset = (scrollViewer.HorizontalOffset + mousePoint.X) * scale - mousePoint.X;
            scrollViewer.ScrollToHorizontalOffset(x_barOffset);

            Double y_barOffset = (scrollViewer.VerticalOffset + mousePoint.Y) * scale - mousePoint.Y;
            scrollViewer.ScrollToVerticalOffset(y_barOffset);
        }

        /// <summary>
        /// 2021.08.04 D.Honjyou
        /// キャンバスのサイズが変更されたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imgCharaSrc_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            charaInkCanvas.Height = e.NewSize.Height;
            charaInkCanvas.Width = e.NewSize.Width;
        }

        /// <summary>
        /// 2021.08.04 D.Honjyou
        /// スライダーが変更された
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageRateChaged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            System.Diagnostics.Debug.WriteLine("kimasita---" + ImageRate.Value);
            if (charaInkCanvas == null) return;
            lblScale.Text = ImageRate.Value + "%";

            charaInkCanvas.Height *= ImageRate.Value * 0.01;
            charaInkCanvas.Width *= ImageRate.Value * 0.01;
            //System.Diagnostics.Debug.WriteLine($"Canvas h={charaInkCanvas.Height},w={charaInkCanvas.Width}   Img h={imgCharaSrc.Height}, w={imgCharaSrc.Width}.");

        }

        /// <summary>
        /// 2021.08.06 ゴミ箱ボタンが押されたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrushButton_Click(object sender, RoutedEventArgs e)
        {
            var Oya = (MainWindow)Application.Current.MainWindow;
            System.Diagnostics.Debug.WriteLine("親のプロジェクトタイトル-->" + Oya.Project.ProjectTitle);
            Oya.DeleteCurrentRect(CharaImageName);
        }

        /// <summary>
        /// グリッド線の表示キッカー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DrawWaku(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"表示変更！！{this.Name}");
            //画像処理のテスト・・・
            DrawWakuInCanvas(charaInkCanvas);
            DrawWakuInImage(charaProcCanvas);
        }

        /// <summary>
        /// キャンバスにスケール線を引きます。(8*8マス)
        /// </summary>
        /// <param name="canvas">ラインを引くキャンバス</param>
        private void DrawWakuInCanvas(InkCanvas canvas)
        {
            int i;
            var Oya = (MainWindow)Application.Current.MainWindow;
            canvas.Children.Clear();
            if (Oya.MenuGridLineCheck.IsChecked == true){
                for (i = 1; i < 8; i++)
                {
                    //横ラインを引く
                    Line myLine1 = new Line();
                    myLine1.Stroke = Brushes.LightBlue;
                    myLine1.X1 = 0;
                    myLine1.X2 = canvas.Width;
                    myLine1.Y1 = (canvas.Height / 8 )* i;
                    myLine1.Y2 = (canvas.Height / 8) * i;
                    myLine1.HorizontalAlignment = HorizontalAlignment.Left;
                    myLine1.VerticalAlignment = VerticalAlignment.Center;
                    myLine1.StrokeThickness = 1;
                    if (i != 4)
                    {
                        myLine1.StrokeDashArray = new DoubleCollection { 2 };
                    }
                    else
                    {
                        myLine1.Stroke = Brushes.Red;
                    }
                    canvas.Children.Add(myLine1);
                }
                for (i = 1; i < 8; i++)
                {
                    //縦ラインを引く
                    Line myLine1 = new Line();
                    myLine1.Stroke = Brushes.LightBlue;
                    myLine1.X1 = (canvas.Width / 8) * i;
                    myLine1.X2 = (canvas.Width / 8) * i;
                    myLine1.Y1 = 0;
                    myLine1.Y2 = canvas.Height;
                    myLine1.HorizontalAlignment = HorizontalAlignment.Left;
                    myLine1.VerticalAlignment = VerticalAlignment.Center;
                    myLine1.StrokeThickness = 1;
                    if (i != 4)
                    {
                        myLine1.StrokeDashArray = new DoubleCollection { 2 };
                    }
                    else
                    {
                        myLine1.Stroke = Brushes.Red;
                    }
                    canvas.Children.Add(myLine1);
                }
            }
        }

        /// <summary>
        /// キャンバスに枠を描く（その２）
        /// </summary>
        /// <param name="canvas"></param>
        private void DrawWakuInImage(Canvas canvas)
        {
            int i;
            var Oya = (MainWindow)Application.Current.MainWindow;
            canvas.Children.Clear();
            if (Oya.MenuGridLineCheck.IsChecked == true)
            {
                for (i = 1; i < 8; i++)
                {
                    //横ラインを引く
                    Line myLine1 = new Line();
                    myLine1.Stroke = Brushes.LightBlue;
                    myLine1.X1 = 0;
                    myLine1.X2 = canvas.Width;
                    myLine1.Y1 = (canvas.Height / 8) * i;
                    myLine1.Y2 = (canvas.Height / 8) * i;
                    myLine1.HorizontalAlignment = HorizontalAlignment.Left;
                    myLine1.VerticalAlignment = VerticalAlignment.Center;
                    myLine1.StrokeThickness = 1;
                    if(i != 4)
                    {
                        myLine1.StrokeDashArray = new DoubleCollection { 2 };
                    }
                    else
                    {
                        myLine1.Stroke = Brushes.Red;
                    }

                    canvas.Children.Add(myLine1);
                }
                for (i = 1; i < 8; i++)
                {
                    //縦ラインを引く
                    Line myLine1 = new Line();
                    myLine1.Stroke = Brushes.LightBlue;
                    myLine1.X1 = (canvas.Width / 8) * i;
                    myLine1.X2 = (canvas.Width / 8) * i;
                    myLine1.Y1 = 0;
                    myLine1.Y2 = canvas.Height;
                    myLine1.HorizontalAlignment = HorizontalAlignment.Left;
                    myLine1.VerticalAlignment = VerticalAlignment.Center;
                    myLine1.StrokeThickness = 1;
                    if (i != 4)
                    {
                        myLine1.StrokeDashArray = new DoubleCollection { 2 };
                    }
                    else
                    {
                        myLine1.Stroke = Brushes.Red;
                    }
                    
                    canvas.Children.Add(myLine1);
                }
            }
        }

        public void ImageProcessExe()
        {
            var Oya = (MainWindow)Application.Current.MainWindow;

            BitmapSource proc_bmp;
            imageEffect.GetGravityPointDouble((BitmapSource)charaSrcImage);
            proc_bmp = (BitmapSource)charaSrcImage;
            if(Oya.MenuNomalizeCheck.IsChecked == true) proc_bmp = imageEffect.Normalize(proc_bmp, 96+38+96);
            if(Oya.MenuCenterCheck.IsChecked == true) proc_bmp = imageEffect.MoveCenter_fromGravity(proc_bmp);

            //画像処理が終わったら、処理後ウィンドウに表示（charaProcCanvas)
            //2021.08.08 D.Honjyou
            //CachedBitmapになってしまったのを回避するためいったんバイナリに変換
            byte[] tmp = Oya.ToBinary(proc_bmp);
            BitmapSource srcBmp = Oya.ToBitmapSource(tmp);
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.Stretch = Stretch.None;
            imageBrush.ImageSource = (BitmapImage)srcBmp;

            charaProcCanvas.DataContext = proc_bmp;
            charaProcCanvas.Background = imageBrush;
            charaProcCanvas.Width = proc_bmp.Width;
            charaProcCanvas.Height = proc_bmp.Height;
            DrawWaku(null, null);

            double rate, Aratio;
            double[] Kajyu = new double[4]; 
            rate = imageEffect.GetPixelRate(proc_bmp);
            Aratio = imageEffect.GetAspectRatio(proc_bmp);
            imageEffect.GetKajyu(proc_bmp,Kajyu);
            System.Diagnostics.Debug.WriteLine($"画素数の割合＝{rate}%");
            System.Diagnostics.Debug.WriteLine($"縦横比＝{Aratio}%");

            //レーダーチャートを作成
            InitializeChart(rate, Aratio, Kajyu);
        }

        /// <summary>
        /// グラフを初期化する
        /// </summary>
        private void InitializeChart(double Rate, double Aratio, double[] Kajyu)
        {
            System.Diagnostics.Debug.WriteLine("レーダーチャート描きます。");
            //グルーピングのデータ作成
            string[] items = { "画素数比率", "縦横比", "加重─", "加重／", "加重│", "加重＼" };
            List<(string, double[])> datas = new List<(string, double[])>();
            datas.Add(("画像処理結果", new double[] { Rate*100, Aratio*100, Kajyu[0]*100, Kajyu[1]*100, Kajyu[2]*100, Kajyu[3]*100 }));
            //datas.Add(("Core-i5", new double[] { 6, 2.8, 12, 32, 50 }));
            //datas.Add(("Core-i3", new double[] { 4, 2.0, 8, 16, 30 }));
            //datas.Add(("Pentium", new double[] { 2, 1.8, 4, 8, 10 }));
            //datas.Add(("Celeron", new double[] { 2, 1.5, 4, 8, 5 }));
            DrawRadar(MyChart, "レーダー", items, datas);
        }

        /// <summary>
        /// レーダーチャート
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="title"></param>
        /// <param name="labels"></param>
        /// <param name="values"></param>
        private void DrawRadar(Chart chart, string title, string[] labels, List<(string legend, double[] ys)> values)
        {
            ChartInit(chart, title, false);


            //labelの分だけルール
            for (int i = 0; i < values.Count; i++)
            {
                //シリーズの生成
                var seri = new Series(values[i].legend) { ChartType = SeriesChartType.Radar, IsVisibleInLegend = true };

                for (int j = 0; j < labels.Length; j++)
                {
                    DataPoint dp = new DataPoint();
                    dp.SetValueXY(labels[j], values[i].ys[j]);
                    seri.Points.Add(dp);
                }

                //チャートにシリーズを登録
                chart.Series.Add(seri);
            }
        }


        /// <summary>
        /// チャートの初期化
        /// </summary>
        /// <param name="chart"></param>
        private void ChartInit(Chart chart, string title, bool Zoom = true)
        {
            //タイトル／エリア／シリーズのクリア
            chart.Titles.Clear();
            chart.ChartAreas.Clear();
            chart.Series.Clear();
            chart.Legends.Clear();

            //タイトルの設定
            chart.Titles.Add(title);
            System.Diagnostics.Debug.WriteLine($"Title={title}");

            //凡例表示エリアの登録
            chart.Legends.Add("");

            //チャートエリアの生成と登録
            var area = new ChartArea();
            chart.ChartAreas.Add(area);

            //X軸とY軸のオブジェクトを取得
            var axis_x = area.AxisX;
            var axis_y = area.AxisY;

            //X軸の補助線を設定
            axis_x.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            axis_x.MinorGrid.LineColor = System.Drawing.Color.LightGray;
            axis_x.MinorGrid.LineDashStyle = ChartDashStyle.Dash;

            //Y軸の補助線を設定
            axis_y.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            axis_y.MinorGrid.LineColor = System.Drawing.Color.LightGray;
            axis_y.MinorGrid.LineDashStyle = ChartDashStyle.Dash;

            //ズーム機能を有効化
            axis_x.ScaleView.Zoomable = Zoom;
            axis_y.ScaleView.Zoomable = Zoom;

            //ズーム機能を実現するためのイベントハンドラ定義
            if (Zoom)
            {
                //マウスホイールボタンのクリックによるズーム解除
                chart.MouseClick += (s, e) =>
                {
                    if (e.Button == System.Windows.Forms.MouseButtons.Middle)
                    {
                        axis_x.ScaleView.ZoomReset();
                        axis_y.ScaleView.ZoomReset();
                    }
                };

                //マウスホィールによるズーム処理
                chart.MouseWheel += (s, e) =>
                {
                    try
                    {
                        double xmin = axis_x.ScaleView.ViewMinimum;
                        double xmax = axis_x.ScaleView.ViewMaximum;
                        double xpos = axis_x.PixelPositionToValue(e.Location.X);
                        double xsize = (xmax - xmin) * ((e.Delta > 0) ? 0.25 : 1);
                        axis_x.ScaleView.Zoom(Math.Round(xpos - xsize, 0, MidpointRounding.AwayFromZero), Math.Round(xpos + xsize, 0, MidpointRounding.AwayFromZero));

                        double ymin = axis_y.ScaleView.ViewMinimum;
                        double ymax = axis_y.ScaleView.ViewMaximum;
                        double ypos = axis_y.PixelPositionToValue(e.Location.Y);
                        double ysize = (ymax - ymin) * ((e.Delta > 0) ? 0.25 : 1);
                        axis_y.ScaleView.Zoom(Math.Round(ypos - ysize, 0, MidpointRounding.AwayFromZero), Math.Round(ypos + ysize, 0, MidpointRounding.AwayFromZero));
                    }
                    catch { }
                };
            }
        }

        private void InitializeChart_Click(object sender, RoutedEventArgs e)
        {
            //InitializeChart();
        }
    }


}
