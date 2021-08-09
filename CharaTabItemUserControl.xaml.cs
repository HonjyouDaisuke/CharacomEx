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

        private int sIndex;
        /// <summary>
        /// MetroWindow
        /// </summary>
        public MetroWindow Metro { get; set; } = System.Windows.Application.Current.MainWindow as MetroWindow;
        public string CharaImageName { get => _charaImageName; set => _charaImageName = value; }
        public BitmapSource CharaSrcImage { get => charaSrcImage; set => charaSrcImage = value; }

        public CharaTabItemUserControl(BitmapSource img)
        {
            InitializeComponent();

            sIndex = 3;
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
            DrawingAttributes da = new DrawingAttributes();

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

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            //画像処理のテスト・・・
            BitmapSource proc_bmp;
            imageEffect.GetGravityPointDouble((BitmapSource)charaSrcImage);
            proc_bmp = (BitmapSource)charaSrcImage;
            proc_bmp = imageEffect.Normalize(proc_bmp, 38.0);
            proc_bmp = imageEffect.MoveCenter_fromGravity(proc_bmp);
            imgCharaPrc.Source = proc_bmp;
        }

        public void ImageProcessExe()
        {
            BitmapSource proc_bmp;
            imageEffect.GetGravityPointDouble((BitmapSource)charaSrcImage);
            proc_bmp = (BitmapSource)charaSrcImage;
            proc_bmp = imageEffect.Normalize(proc_bmp, 38.0);
            proc_bmp = imageEffect.MoveCenter_fromGravity(proc_bmp);
            imgCharaPrc.Source = proc_bmp;
        }
    }


}
