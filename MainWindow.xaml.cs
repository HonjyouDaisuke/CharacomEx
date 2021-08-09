using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MSAPI = Microsoft.WindowsAPICodePack;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Win32;
using MahApps.Metro.Controls;
using ControlzEx.Theming;
using ui = ModernWpf.Controls;
using Image = System.Windows.Controls.Image;
using Pen = System.Windows.Media.Pen;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace CharacomEx
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private ImageEffect imgEffect = new ImageEffect();
        private ProjectClass _project = new ProjectClass();
        public int MainImageIndex;
        private int _charaImageIndex;

        public int CharaImageIndex { get => _charaImageIndex; set => _charaImageIndex = value; }
        public ProjectClass Project { get => _project; set => _project = value; }



        public MainWindow()
        {
            InitializeComponent();
            CTreeView.ItemsSource = _project.MainImages;
            ProjectInfo.DataContext = _project;

            //this.DataContext = _project;
            //CharaImages.ItemsSource = _projectObj;

            _project.ProjectTitle = "First Project";

            MainImageIndex = 0;
            _charaImageIndex = 0;
        }

        private void ShowFlyout(object sender, RoutedEventArgs e)
        {
            this.flyout.IsOpen = true;
        }

        private void MenuItemDarkMode_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.Current.ChangeTheme(this, "Dark.Blue");
        }

        private void MenuItemLightMode_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.Current.ChangeTheme(this, "Light.Blue");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _project.ProjectTitle = "Fiest Project";
            _project.ProjectName = "TestProject";

            MainImageClass testMain = new MainImageClass();
            CharaImageClass TestChara1 = new CharaImageClass();
            CharaImageClass TestChara2 = new CharaImageClass();

            TestChara1.CharaImageTitle = "aaa";
            TestChara2.CharaImageTitle = "bbb";

            testMain.MainImageTitle = "maintitle";
            testMain.CharaImages.Add(TestChara1);
            testMain.CharaImages.Add(TestChara2);
            _project.MainImages.Add(testMain);
            MainImageIndex = _project.MainImages.Count - 1;


            foreach (MainImageClass mm in _project.MainImages)
            {
                System.Diagnostics.Debug.WriteLine(mm.MainImageTitle);

                foreach (CharaImageClass cc in mm.CharaImages)
                {
                    System.Diagnostics.Debug.WriteLine("   " + cc.CharaImageTitle);
                }
            }

        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            CharaImageClass TestChara1 = new CharaImageClass();
            CharaImageClass TestChara2 = new CharaImageClass();

            TestChara1.CharaImageTitle = "dd";
            TestChara2.CharaImageTitle = "eee";


            ((MainImageClass)_project.MainImages[MainImageIndex]).CharaImages.Add(TestChara1);
            ((MainImageClass)_project.MainImages[MainImageIndex]).CharaImages.Add(TestChara2);

            foreach (MainImageClass mm in _project.MainImages)
            {
                System.Diagnostics.Debug.WriteLine(mm.MainImageTitle);

                foreach (CharaImageClass cc in mm.CharaImages)
                {
                    System.Diagnostics.Debug.WriteLine("   " + cc.CharaImageTitle);
                }
            }
        }

        /// <summary>
        /// MenuItemOpenMeainImage_Click
        /// 資料画像を開く（メニューから）
        /// 2021.07.14 D.Honjyou
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemOpenMainImage_Click(object sender, RoutedEventArgs e)
        {
            // ダイアログのインスタンスを生成
            var dialog = new OpenFileDialog();

            // ファイルの種類を設定
            //dialog.Filter = "テキストファイル (*.txt)|*.txt|全てのファイル (*.*)|*.*";

            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {
                BitmapImage bitmap = new BitmapImage(); // デコードされたビットマップイメージのインスタンスを作る。
                try
                {
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(dialog.FileName); // ビットマップイメージのソースにファイルを指定する。
                    bitmap.EndInit();
                    //ImageDoc1.Source = bitmap; // Imageコントロールにバインディングする。

                    //2021.07.24 D.Honjyou
                    // Projectにメイン画像を追加
                    MainImageClass mainImage = new MainImageClass();
                    mainImage.MainImageName = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
                    mainImage.MainImageTitle = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
                    mainImage.MainImage.Width = bitmap.PixelWidth;
                    mainImage.MainImage.Height = bitmap.PixelHeight;
                    mainImage.MainImage.Source = bitmap;

                    _project.MainImages.Add(mainImage);
                    MainImageIndex = _project.MainImages.Count - 1;
                    //切り出しパネルのバインディングを変更
                    CharaImages.ItemsSource = _project.MainImages[MainImageIndex].CharaImages;

                    //2021.07.25 D.Honjyou
                    //新しいメインタブを作成
                    MakeNewMainImageTab();

                    foreach (MainImageClass mm in _project.MainImages)
                    {
                        System.Diagnostics.Debug.WriteLine(mm.MainImageTitle);

                        foreach (CharaImageClass cc in mm.CharaImages)
                        {
                            System.Diagnostics.Debug.WriteLine("   " + cc.CharaImageTitle);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// 2021.07.29 D.Honjyou
        /// 赤い矩形を描く
        /// </summary>
        /// <param name="inkCanvas">InkCanvas</param>
        /// <param name="rect">矩形データ</param>
        private void inkCanvas_DrawRectangle(object inkCanvas, Rect rect)
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
            ((InkCanvas)inkCanvas).Children.Add(myLine1);

            Line myLine2 = new Line();
            myLine2.Stroke = Brushes.Red;
            myLine2.X1 = rect.X;
            myLine2.X2 = rect.X + rect.Width;
            myLine2.Y1 = rect.Y + rect.Height;
            myLine2.Y2 = rect.Y + rect.Height;
            myLine2.StrokeThickness = 3;
            myLine2.StrokeDashArray = new DoubleCollection { 2, 2 };
            ((InkCanvas)inkCanvas).Children.Add(myLine2);

            Line myLine3 = new Line();
            myLine3.Stroke = Brushes.Red;
            myLine3.X1 = rect.X;
            myLine3.X2 = rect.X;
            myLine3.Y1 = rect.Y;
            myLine3.Y2 = rect.Y + rect.Height;
            myLine3.StrokeThickness = 3;
            myLine3.StrokeDashArray = new DoubleCollection { 2, 2 };
            ((InkCanvas)inkCanvas).Children.Add(myLine3);

            Line myLine4 = new Line();
            myLine4.Stroke = Brushes.Red;
            myLine4.X1 = rect.X + rect.Width;
            myLine4.X2 = rect.X + rect.Width;
            myLine4.Y1 = rect.Y;
            myLine4.Y2 = rect.Y + rect.Height;
            myLine4.StrokeThickness = 3;
            myLine4.StrokeDashArray = new DoubleCollection { 2, 2 };
            ((InkCanvas)inkCanvas).Children.Add(myLine4);
        }

        

        /// <summary>
        /// メイン画像のinkCanvasでストロークが終わった時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void inkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            //-----------------------メイン画像かどうかのフラグが必要-------------------

            if (_project.MainImages[MainImageIndex].MainImage.Source == null)
            {
                return;
            }
            // 元になる画像を画面から読み込む
            BitmapSource bmp = (BitmapSource)_project.MainImages[MainImageIndex].MainImage.Source;
            // 原画像を２値化する
            BitmapSource bmp2 = imgEffect.TwoColorProc(bmp);
            var image = new RenderTargetBitmap((int)_project.MainImages[MainImageIndex].MainImage.ActualWidth, (int)_project.MainImages[MainImageIndex].MainImage.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            image.Render(_project.MainImages[MainImageIndex].MainImage);

            System.Windows.Point min = new System.Windows.Point(9999.0, 9999.0);
            System.Windows.Point max = new System.Windows.Point(0.0, 0.0);
            System.Windows.Point bp = new System.Windows.Point(0, 0);

            // 元画像に色々書き込む
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext drawContent = dv.RenderOpen())
            {

                foreach (System.Windows.Point p in ((InkCanvas)sender).Strokes[0].StylusPoints)
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

            var croppedBitmap = new CroppedBitmap(image, new Int32Rect((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height));
            var croppedBitmap2 = new CroppedBitmap(bmp2, new Int32Rect((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height));
            var croppedBitmap3 = new CroppedBitmap(bmp3, new Int32Rect((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height));
            bmp = imgEffect.ExtractionProc(croppedBitmap, croppedBitmap2, croppedBitmap3);

            ((InkCanvas)sender).Strokes.Clear();

            inkCanvas_DrawRectangle(sender, rect);

            
            //切り出した矩形をメイン画像に追加
            //勝手に表示されるように作成
            CharaImageClass charaImage = new CharaImageClass();
            charaImage.CharaImageTitle = ((MainImageClass)_project.MainImages[MainImageIndex]).MainImageTitle + "-" + ((MainImageClass)_project.MainImages[MainImageIndex]).CharaImages.Count.ToString();
            charaImage.CharaImageName = charaImage.CharaImageTitle;
            charaImage.CharaRect = rect;
            charaImage.CharaImage.Source = bmp;

            System.Diagnostics.Debug.WriteLine(" ??? Height = " + charaImage.CharaRect.Height + "  Width = " + charaImage.CharaRect.Width);

            ((MainImageClass)_project.MainImages[MainImageIndex]).CharaImages.Add(charaImage);

        }

        /// <summary>
        /// 2021.07.26 D.Honjyou
        /// タブのタイトルから存在確認
        /// </summary>
        /// <param name="TabTitle"></param>
        /// <returns>iRet 9999=見つかりませんでした。 0-9998 タブのインデックス</returns>
        private Boolean IsExistTabFromName(string TabTitle)
        {
            Boolean bRet;

            bRet = false;
            foreach (TabItem t in mainTab.Items)
            {
                if (t.Header.ToString() == TabTitle)
                {
                    bRet = true;
                }
            }

            return bRet;
        }

        /// <summary>
        /// 2021.07.31 D.Honjyou
        /// タブのタイトルからIndexを取得
        /// </summary>
        /// <param name="TabTitle"></param>
        /// <returns>int TabIndex 存在しないときは-1</returns>
        private int getTabIndexFromName(string TabTitle)
        {
            int iRet;

            iRet = -1;
            foreach (TabItem t in mainTab.Items)
            {
                if (t.Header.ToString() == TabTitle)
                {
                    iRet = t.TabIndex;
                }
            }

            return iRet;
        }

        /// <summary>
        /// 2021.07.24 ProjectTreeSelect
        /// Flyoutからプロジェクトの中のメイン画像を選択
        /// D.Honjyou
        /// </summary>
        /// <param name="objSender"></param>
        /// <param name="e"></param>
        private void ProjectTreeSelect(object objSender, EventArgs e)
        {
            int tIndex;

            tIndex = CTreeView.SelectedIndex;
            if (!IsExistTabFromName(_project.MainImages[tIndex].MainImageName))
            {
                MainImageIndex = tIndex;
                MakeNewMainImageTab();
            }
            SelectTabItem(_project.MainImages[tIndex].MainImageName);
            CharaImages.ItemsSource = _project.MainImages[tIndex].CharaImages;
            System.Diagnostics.Debug.Write("メイン画像のインデックス=" + MainImageIndex.ToString());
            this.flyout.IsOpen = false;
        }

        /// <summary>
        /// SelectTabItem
        /// 2021.07.25 D.Honjyou
        /// タブ名を検索して、見つかったタブを選択状態にする。
        /// </summary>
        /// <param name="TabName"></param>
        private void SelectTabItem(string TabName)
        {
            foreach (TabItem ti in mainTab.Items)
            {
                if (ti.Header != null)
                {
                    if (ti.Header.ToString() == TabName)
                    {
                        ti.IsSelected = true;
                    }
                }

            }
        }

        /// <summary>
        /// 2021.07.25 D.Honjyou
        /// メインイメージタブを作成する
        /// </summary>
        private void MakeNewMainImageTab()
        {
            TabItem iTabItem = new TabItem();
            Grid g = new Grid();
            ScrollViewer sv = new ScrollViewer();
            CustomInkCanvas cic = new CustomInkCanvas();
            Image img = new Image();

            iTabItem.Header = _project.MainImages[MainImageIndex].MainImageName;
            img = _project.MainImages[MainImageIndex].MainImage;

            img.Stretch = Stretch.Fill;

            Binding imgBindWidth = new Binding("Source.PixelWidth");
            imgBindWidth.RelativeSource = new RelativeSource(RelativeSourceMode.Self);
            Binding imgBindHeight = new Binding("Source.PixelHeight");
            imgBindHeight.RelativeSource = new RelativeSource(RelativeSourceMode.Self);

            img.SetBinding(Image.WidthProperty, imgBindWidth);
            img.SetBinding(Image.HeightProperty, imgBindHeight);

            MainTabItemUserControl mtc = new MainTabItemUserControl();

            mtc.Name = "MainTabItem";
            mtc.inkCanvas.StrokeCollected += new InkCanvasStrokeCollectedEventHandler(inkCanvas_StrokeCollected);
            mtc.inkCanvas.Children.Add(img);
            iTabItem.Content = mtc;

            //2021.07.31 メインタブの画像に切り出し矩形を作成
            foreach (CharaImageClass c in _project.MainImages[MainImageIndex].CharaImages)
            {
                inkCanvas_DrawRectangle(mtc.inkCanvas, c.CharaRect);
            }

            mainTab.Items.Add(iTabItem);

            SelectTabItem(_project.MainImages[MainImageIndex].MainImageName);
        }


        
        /// <summary>
        /// 2021.07.25 D.Honjyou
        /// メインイメージタブを作成する
        /// </summary>
        private void MakeNewCharaImageTab(string Header, Image img, Rect inkRect)
        {
            TabItem iTabItem = new TabItem();
            Grid g = new Grid();
            //Image img = new Image();

            //2021.07.31 D.Honjyou
            //タブがすでに存在していたら、フォーカスを当てて終了
            if (IsExistTabFromName(Header))
            {
                SelectTabItem(Header);
                return;
            }

            //2021.08.01 D.Honjyou
            // CharaTabItemをユーザーコントロールとして追加
            CharaTabItemUserControl ctc = new CharaTabItemUserControl((BitmapSource)img.Source);

            //2021.08.08 D.Honjyou
            //CachedBitmapになってしまったのを回避するためいったんバイナリに変換
            byte[] tmp = ToBinary((BitmapSource)img.Source);
            BitmapSource srcBmp = ToBitmapSource(tmp);

            ImageBrush imageBrush = new ImageBrush();
            imageBrush.Stretch = Stretch.None;
            imageBrush.ImageSource = (BitmapImage)srcBmp;

            iTabItem.Header = Header;
            ctc.CharaImageName = Header;
            ctc.CharaSrcImage = (BitmapSource)img.Source;
            ctc.charaInkCanvas.Background = imageBrush;
            ctc.charaInkCanvas.Height = srcBmp.PixelHeight;
            ctc.charaInkCanvas.Width = srcBmp.PixelWidth;
            iTabItem.Content = ctc;

            ctc.ImageProcessExe();

            mainTab.Items.Add(iTabItem);
            //subtab.Items.Add(bTab);


            SelectTabItem(Header);
        }

        /// <summary>
        /// 2021.07.24 D.Honjyou
        /// 切り出しパネルから文字を選んだ時、メインタブに画像を表示
        /// </summary>
        /// <param name="objSender"></param>
        /// <param name="e"></param>
        private void CharaListSelect(object objSender, EventArgs e)
        {
            int tIndex;
            string sHeader;
            Image img = new Image();
            
            //選択中の切り出し矩形番号を取得
            tIndex = CharaImages.SelectedIndex;

            sHeader = _project.MainImages[MainImageIndex].CharaImages[tIndex].CharaImageName;
            img = _project.MainImages[MainImageIndex].CharaImages[tIndex].CharaImage;
            System.Diagnostics.Debug.WriteLine("  ttt---Height = " + _project.MainImages[MainImageIndex].CharaImages[tIndex].CharaRect.Height + "  Width = " + _project.MainImages[MainImageIndex].CharaImages[tIndex].CharaRect.Width);
            System.Diagnostics.Debug.WriteLine($"　CharaImage[tIndex = {tIndex} : Name = {_project.MainImages[MainImageIndex].CharaImages[tIndex].CharaImageName} ]");
            //System.Diagnostics.Debug.WriteLine($"　CharaImage type= { img.Sourc }" );
            MakeNewCharaImageTab(sHeader, _project.MainImages[MainImageIndex].CharaImages[tIndex].CharaImage, _project.MainImages[MainImageIndex].CharaImages[tIndex].CharaRect);

            SelectTabItem(_project.MainImages[MainImageIndex].CharaImages[tIndex].CharaImageName);
            CharaImageIndex = tIndex;
            System.Diagnostics.Debug.WriteLine("インデックスを設定しました。Main = " + MainImageIndex.ToString() + " , chara = " + CharaImageIndex.ToString());
        }

        /// <summary>
        /// 2021.08.05 メイン画像か個別文字化の判定
        /// </summary>
        /// <param name="tabName"></param>
        /// <returns></returns>
        private MainOrCharaClass checkTabName(string tabName)
        {
            int mainIndex, charaIndex;
            MainOrCharaClass mc = new MainOrCharaClass();
            mc.CheckFlag = false;
            mc.Name = tabName;

            mainIndex = 0;
            charaIndex = 0;
            foreach (MainImageClass m in _project.MainImages)
            {
                if (m.MainImageName == tabName)
                {
                    mc.MainOrChara = 1;
                    mc.MainIndex = mainIndex;
                    mc.CheckFlag = true;
                }
                foreach (CharaImageClass c in m.CharaImages)
                {
                    if (c.CharaImageName == tabName)
                    {
                        mc.MainOrChara = 2;
                        mc.MainIndex = mainIndex;
                        mc.CharaIndex = charaIndex;
                        mc.CheckFlag = true;
                    }
                    charaIndex++;
                }
                mainIndex++;
            }

            return (mc);
        }
        /// <summary>
        /// mainTabSlectionChange
        /// 2021.07.25 D.Honjyou
        /// メインタブを選択したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (mainTab.SelectedItem == null) return;

            int selectwdIndex = ((TabControl)sender).SelectedIndex;
            string tabName = ((TabItem)mainTab.SelectedItem).Header.ToString();
            string ret;

            System.Diagnostics.Debug.WriteLine("tabName = " + tabName);

            MainOrCharaClass mc = new MainOrCharaClass();

            mc = checkTabName(tabName);
            if (mc.CheckFlag)
            {
                if (mc.MainOrChara == 1)
                {
                    System.Diagnostics.Debug.WriteLine("MainTab -- " + mc.Name + ": mainIndex = " + mc.MainIndex);
                    //切り出しパネルのバインディングを変更
                    CharaImages.ItemsSource = _project.MainImages[mc.MainIndex].CharaImages;
                    MainImageIndex = mc.MainIndex;

                }
                else
                {
                    _charaImageIndex = mc.CharaIndex;
                    System.Diagnostics.Debug.WriteLine("CharaTab -- " + mc.Name + ": mainIndex = " + mc.MainIndex + ": charaIndex = " + mc.CharaIndex);
                }

            }
        }

        /// <summary>
        /// 2021.07.25 D.Honjyou
        /// メニュー：終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        /// <summary>
        /// 2021.07.25 D.Honjyou
        /// メニュー：名前を付けて保存（プロジェクト）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemProjectOpen_Click(object sender, RoutedEventArgs e)
        {
            // ダイアログのインスタンスを生成
            var dialog = new OpenFileDialog();

            // ファイルの種類を設定
            dialog.Filter = "CharacomExファイル (*.cex)|*.cex|全てのファイル (*.*)|*.*";

            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {
                FileStream fs = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read);
                BinaryFormatter f = new BinaryFormatter();
                //読み込んで逆シリアル化する
                _project = (ProjectClass)f.Deserialize(fs);
                fs.Close();

                //2021.08.03 D.Honjyou
                //プロジェクトのファイル名をセット
                _project.ProjectFileName = dialog.FileName;

                // Removes the selected tab:  
                mainTab.Items.Remove(e.Source);
                // Removes all the tabs:  
                mainTab.Items.Clear();

                foreach (MainImageClass m in _project.MainImages)
                {
                    System.Diagnostics.Debug.WriteLine("main image name =" + m.MainImageTitle);
                    BitmapSource bbb;

                    //bbb = LoadImage(m.Img);
                    bbb = ToBitmapSource(m.Img);
                    m.MainImage = new Image();
                    m.MainImage.Source = bbb;
                    //m.MainImage.Source = LoadImage(m.Img);
                    foreach (CharaImageClass c in m.CharaImages)
                    {
                        System.Diagnostics.Debug.WriteLine("chara image name =" + c.CharaImageTitle);
                        BitmapSource ccc;

                        //bbb = LoadImage(m.Img);
                        ccc = ToBitmapSource(c.Img);
                        c.CharaImage = new Image();
                        c.CharaImage.Source = ccc;
                        c.CharaImage.Width = ccc.Width;
                        c.CharaImage.Height = ccc.Height;
                        System.Diagnostics.Debug.WriteLine(ccc.Width + "--->" + c.CharaImage.Width);
                        //m.MainImage.Source = LoadImage(m.Img);
                    }
                }
                CTreeView.ItemsSource = _project.MainImages;
                ProjectInfo.DataContext = _project;

                //上書き保存をONにする
                if (_project.ProjectFileName != "")
                {
                    MenuSaveAll.IsEnabled = true;
                }
                System.Diagnostics.Debug.WriteLine("Project title=" + _project.ProjectTitle);
                System.Diagnostics.Debug.WriteLine("Project title=" + _project.ProjectFileName);

                //ProjectInfo.DataContext = _project;

                //2021.07.29 D.Honjyou
                //プロジェクトにメイン画像があったら１番目をタブ表示
                if (_project.MainImages.Count > 0)
                {
                    MainImageIndex = 0;
                    if (!IsExistTabFromName(_project.MainImages[MainImageIndex].MainImageName))
                    {
                        MakeNewMainImageTab();
                    }
                    SelectTabItem(_project.MainImages[MainImageIndex].MainImageName);
                    CharaImages.ItemsSource = _project.MainImages[MainImageIndex].CharaImages;

                }
            }
        }


        /// <summary>
        /// BitmapSourceをbyte[]型へ変換
        /// </summary>
        /// <param name="bs"></param>
        /// <returns></returns>
        public static byte[] ToBinary(BitmapSource img)
        {
            if (img == null) return null;

            //BitmapSource bmp = (BitmapSource)img;
            //img.Source = bmp;

            // BitmapSourceの派生クラス「RenderTargetBitmap」で、画像を取ってくる
            //System.Diagnostics.Debug.WriteLine("->with = " + bmp.PixelWidth.ToString() + ",Height = " + bmp.PixelHeight.ToString());
            System.Diagnostics.Debug.WriteLine("+>with = " + img.Width.ToString() + ",Height = " + img.Height.ToString());
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
        public static BitmapSource ToBitmapSource(byte[] byteArray)
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
        /// 2021.07.25 D.Honjyou
        /// メニュー：名前を付けて保存（プロジェクト）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemProjectSaveAs_Click(object sender, RoutedEventArgs e)
        {
            // ダイアログのインスタンスを生成
            var dialog = new SaveFileDialog();

            // ファイルの種類を設定
            dialog.Filter = "CharacomExファイル (*.cex)|*.cex|全てのファイル (*.*)|*.*";
            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {
                FileStream fs = new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write);
                foreach (MainImageClass m in _project.MainImages)
                {
                    BitmapSource mbmp = (BitmapSource)m.MainImage.Source;
                    m.Img = ToBinary(mbmp);
                    //System.Diagnostics.Debug.WriteLine(m.Img.Length.ToString());
                    foreach (CharaImageClass c in m.CharaImages)
                    {
                        BitmapSource cbmp = (BitmapSource)c.CharaImage.Source;

                        c.Img = ToBinary(cbmp);
                        //System.Diagnostics.Debug.WriteLine("size------>" + c.Img.Length.ToString());
                    }
                }
                BinaryFormatter bf = new BinaryFormatter();
                _project.ProjectFileName = dialog.FileName;
                //シリアル化して書き込む
                bf.Serialize(fs, _project);
                fs.Close();
            }
        }

        /// <summary>
        /// 2021.08.03 D.Honjyou
        /// プロジェクトファイルの上書き保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemProjectSaveAll_Click(object sender, RoutedEventArgs e)
        {
            //プロジェクトのパスが空なら終了
            if (_project.ProjectFileName == "")
            {
                return;
            }

            FileStream fs = new FileStream(_project.ProjectFileName, FileMode.Create, FileAccess.Write);

            foreach (MainImageClass m in _project.MainImages)
            {
                BitmapSource mbmp = (BitmapSource)m.MainImage.Source;
                m.Img = ToBinary(mbmp);
                //System.Diagnostics.Debug.WriteLine(m.Img.Length.ToString());
                foreach (CharaImageClass c in m.CharaImages)
                {
                    BitmapSource cbmp = (BitmapSource)c.CharaImage.Source;

                    c.Img = ToBinary(cbmp);
                    //System.Diagnostics.Debug.WriteLine("size------>" + c.Img.Length.ToString()
                }
            }
            BinaryFormatter bf = new BinaryFormatter();
            _project.ProjectFileName = _project.ProjectFileName;
            //シリアル化して書き込む
            bf.Serialize(fs, _project);
            fs.Close();
        }
        /// <summary>
        /// 2021.08.03 D.Honjyou
        /// 保存する拡張子によってエンコーダーを作成
        /// </summary>
        /// <param name="ext">拡張子（ドットはいらない)</param>
        /// <returns>BitmapEncoder</returns>
        private BitmapEncoder GetEncoder(string ext)
        {
            BitmapEncoder encoder = null;

            if (ext == "jpeg") encoder = new JpegBitmapEncoder();
            else if (ext == "jpg") encoder = new JpegBitmapEncoder();
            else if (ext == "gif") encoder = new GifBitmapEncoder();
            else if (ext == "png") encoder = new PngBitmapEncoder();
            else if (ext == "bmp") encoder = new BmpBitmapEncoder();
            else encoder = new BmpBitmapEncoder();

            return (encoder);
        }

        /// <summary>
        /// 2021.08.03 D.Honjyou
        /// フォルダを検索して、存在しなければ作成
        /// </summary>
        /// <param name="path">フォルダへのパス</param>
        private void MakeDirectory(string path)
        {
            //フォルダがなければ作成
            if (Directory.Exists(path))
            {
                System.Diagnostics.Debug.WriteLine("ProjectFolder あり");
            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(path);
                di.Create();

            }
        }

        /// <summary>
        /// 画像をすべて書き出しメニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemAllImageOutput_Click(object sender, RoutedEventArgs e)
        {
            //string path = @"C:\Users\honjy\OneDrive\ドキュメント\MyProjects\CharacomImagerPro\鑑定書サンプル";
            string ext = "jpeg";

            //path += @"\" + Project.ProjectTitle;
            var dlg = new MSAPI::Dialogs.CommonOpenFileDialog();

            // フォルダ選択ダイアログ（falseにするとファイル選択ダイアログ）
            dlg.IsFolderPicker = true;
            // タイトル
            dlg.Title = "出力するフォルダを選択してください";
            // 初期ディレクトリ
            if (_project.ProjectFileName != "")
            {
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(_project.ProjectFileName);
            }


            if (dlg.ShowDialog() == MSAPI::Dialogs.CommonFileDialogResult.Cancel)
            {
                //キャンセルが押された。
                return;
                //MessageBox.Show($"{dlg.FileName}が選択されました。");
            }
            MakeDirectory(dlg.FileName);

            string fileName = "";
            BitmapEncoder encoder;
            BitmapSource bs;
            FileStream stream;

            //イメージをJpegにして保存
            foreach (MainImageClass m in Project.MainImages)
            {
                //MainImageを保存
                fileName = dlg.FileName + "\\" + m.MainImageTitle + "." + ext;
                stream = new FileStream(fileName, FileMode.Create);
                encoder = GetEncoder(ext);
                bs = (BitmapSource)m.MainImage.Source;
                encoder.Frames.Add(BitmapFrame.Create(bs));
                encoder.Save(stream);
                stream.Close();

                MakeDirectory(dlg.FileName + "\\" + m.MainImageTitle);
                //CharaImage
                foreach (CharaImageClass c in m.CharaImages)
                {
                    //CharaImageを保存
                    fileName = dlg.FileName + "\\" + m.MainImageTitle + "\\" + c.CharaImageTitle + "." + ext;
                    stream = new FileStream(fileName, FileMode.Create);
                    encoder = GetEncoder(ext);
                    bs = (BitmapSource)c.CharaImage.Source;
                    //BitmapSource bb = (BitmapSource)c.CharaImage.Source;
                    //bi = (BitmapImage)bb;
                    encoder.Frames.Add(BitmapFrame.Create(bs));
                    encoder.Save(stream);
                    stream.Close();
                }
            }
        }


        /// <summary>
        /// 2021.07.31 タブを閉じるボタンをクリックした時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseTabItem_Click(object sender, MouseButtonEventArgs e)
        {
            mainTab.Items.RemoveAt(mainTab.SelectedIndex);

        }

        /// <summary>
        /// 2021.08.06 D.Honjyou
        /// 現在表示している矩形を削除する
        /// </summary>
        /// <param name="ImageName">文字矩形の名前</param>
        public void DeleteCurrentRect(string ImageName)
        {
            //現在の矩形のチェック（名前が同じかどうか)
            if (_project.MainImages[MainImageIndex].CharaImages[CharaImageIndex].CharaImageName == ImageName)
            {
                System.Diagnostics.Debug.WriteLine("削除するぜ！！" + ImageName);
                _project.MainImages[MainImageIndex].CharaImages.RemoveAt(CharaImageIndex);
                mainTab.Items.RemoveAt(mainTab.SelectedIndex);
                //メイン画像の矩形を一度クリアする
                int tMainImageIndex;
                foreach (TabItem t in mainTab.Items)
                {
                    if (t.Header.ToString() == _project.MainImages[MainImageIndex].MainImageName)
                    {
                        System.Diagnostics.Debug.WriteLine("main！===" + _project.MainImages[MainImageIndex].MainImageName);
                        foreach (UserControl cc in t.FindChildren<UserControl>())
                        {
                            if (cc.Name == "MainTabItem")
                            {
                                MainTabItemUserControl mtc = (MainTabItemUserControl)cc;
                                //矩形のクリア
                                mtc.ClearRectView();
                                mtc.Name = "MainTabItem";
                                mtc.inkCanvas.StrokeCollected += new InkCanvasStrokeCollectedEventHandler(inkCanvas_StrokeCollected);
                                //画像の再描画
                                mtc.inkCanvas.Children.Add(_project.MainImages[MainImageIndex].MainImage);
                                //矩形の再描画
                                //2021.07.31 メインタブの画像に切り出し矩形を作成
                                foreach (CharaImageClass c in _project.MainImages[MainImageIndex].CharaImages)
                                {
                                    inkCanvas_DrawRectangle(mtc.inkCanvas, c.CharaRect);
                                }

                            }

                        }
                    }
                }
            }
            else
            {
                string st = _project.MainImages[MainImageIndex].CharaImages[CharaImageIndex].CharaImageName;
                System.Diagnostics.Debug.WriteLine($"名前が違いました。。。mainWindow:{st} ImageName:{ImageName}...");
            }
        }

    }


    public class CustomInkCanvas : InkCanvas
    {
        protected override void OnStrokeCollected(InkCanvasStrokeCollectedEventArgs e)
        {
            this.Strokes.Remove(e.Stroke);
            var newStroke = new FillStroke(e.Stroke.StylusPoints, e.Stroke.DrawingAttributes);
            this.Strokes.Add(newStroke);

            base.OnStrokeCollected(new InkCanvasStrokeCollectedEventArgs(newStroke));
        }
    }

    public class FillStroke : Stroke
    {
        public FillStroke(StylusPointCollection stylusPoints) : base(stylusPoints)
        {
        }
        public FillStroke(StylusPointCollection stylusPoints, DrawingAttributes drawingAttributes) : base(stylusPoints, drawingAttributes)
        {
        }

        protected override void DrawCore(DrawingContext drawingContext, DrawingAttributes drawingAttributes)
        {
            if (drawingContext == null)
            {
                throw new ArgumentNullException(nameof(drawingContext));
            }
            if (null == drawingAttributes)
            {
                throw new ArgumentNullException(nameof(drawingAttributes));
            }

            if (this.StylusPoints.Count < 3) return;

            var stroke = new Pen(Brushes.Blue, 2);
            stroke.Freeze();
            var fill = new SolidColorBrush(Color.FromArgb(0x80, 0x00, 0xff, 0xff));
            fill.Freeze();

            var streamGeometry = new StreamGeometry();
            using (var geometryContext = streamGeometry.Open())
            {
                geometryContext.BeginFigure(this.StylusPoints[0].ToPoint(), true, false);
                var points = new PointCollection();
                for (var i = 1; i < this.StylusPoints.Count; i++)
                {
                    points.Add(this.StylusPoints[i].ToPoint());
                    // デバッグ出力
                    //System.Diagnostics.Debug.WriteLine(points[i-1].X.ToString() + "," + points[i-1].Y.ToString());
                }
                geometryContext.PolyLineTo(points, true, false);
            }

            drawingContext.DrawGeometry(fill, stroke, streamGeometry);
        }

    }
}
