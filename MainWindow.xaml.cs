using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Drawing;
using System.Web;
using System.Net;
using System.Text;
using System.IO;
using System.Linq;
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
using MahApps.Metro.Controls.Dialogs;
using ControlzEx.Theming;
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
        private ObservableCollection<NotificationClass> _notifications = new ObservableCollection<NotificationClass>();


        public int CharaImageIndex { get => _charaImageIndex; set => _charaImageIndex = value; }
        public ProjectClass Project { get => _project; set => _project = value; }
        public ObservableCollection<NotificationClass> Notifications { get => _notifications; set => _notifications = value; }
        public string UnopenedNotificationNum;
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

            StartLog_Send();

            LoadNotifications();
            GetNotifications();

            UnopenedNotificationNum = CountUnopenedNotificationNum();
            this.DataContext = new { UnNotificationNum = UnopenedNotificationNum };
        }

        /// <summary>
        /// 2021.08.21 D.Honjyou
        /// 通知情報をローカルファイルから読み込む
        /// </summary>
        private void LoadNotifications()
        {
            string path = "notification.ini";
            if (File.Exists(path))
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                BinaryFormatter f = new BinaryFormatter();
                //読み込んで逆シリアル化する
                _notifications = (ObservableCollection<NotificationClass>)f.Deserialize(fs);
                fs.Close();
            }
        }

        /// <summary>
        /// 2021.08.21 D.Honjyou
        /// 通知情報をローカルファイルに書き出す
        /// </summary>
        private void SaveNotifications()
        {
            FileStream fs = new FileStream("notification.ini", FileMode.Create, FileAccess.Write);
            BinaryFormatter bf = new BinaryFormatter();
            //シリアル化して書き込む
            bf.Serialize(fs, _notifications);
            fs.Close();
        }

        /// <summary>
        /// 2021.08.21 D.Honjyou
        /// 未読通知を数える
        /// </summary>
        private string CountUnopenedNotificationNum()
        {
            int num;
            string strCount;

            num = 0;
            foreach(NotificationClass n in _notifications)
            {
                if (n.IsOpened == false) num++;
            }

            if (num < 1) strCount = "";
            else strCount = num.ToString();

            return (strCount);
        }

        /// <summary>
        /// 2021.08.21 D.Honjyou
        /// 現在取得済みの通知IDの最大値を返す
        /// </summary>
        /// <returns></returns>
        private int GetMaxNotificationID()
        {
            int max = 0;
            Console.WriteLine($"count={_notifications.Count};");
            if (_notifications.Count > 0)
            {
                foreach (NotificationClass n in _notifications)
                {
                    Console.WriteLine($"ID=={n.Id};Title={n.Title}");
                    if(n.Id != "")
                    {
                        if (max < int.Parse(n.Id)) max = int.Parse(n.Id);
                    }
                    
                }
            }
            Console.WriteLine($"MaxID=={max};");
            return max;
        }
        public void test()
        {
            UnopenedNotificationNum = CountUnopenedNotificationNum();
            this.DataContext = new { UnNotificationNum = UnopenedNotificationNum };
        }
        /// <summary>
        /// 2021.08.20 D.Honjyou
        /// インターネットから通知情報を受け取り、Notificationオブジェクトに挿入する。
        /// </summary>
        private void GetNotifications()
        {
            //2021.08.18 通知の取得
            //文字コードを指定する
            Encoding enc = Encoding.GetEncoding("euc-jp");

            //POST送信する文字列を作成
            string postData = "NowID=" + GetMaxNotificationID().ToString();
            //バイト型配列に変換
            byte[] postDataBytes = Encoding.ASCII.GetBytes(postData);

            //WebRequestの作成
            WebRequest req = WebRequest.Create("http://characom.sakuraweb.com/CharacomEX/GetNotifications.php");
            //メソッドにPOSTを指定
            req.Method = "POST";
            //ContentTypeを"application/x-www-form-urlencoded"にする
            req.ContentType = "application/x-www-form-urlencoded";
            //POST送信するデータの長さを指定
            req.ContentLength = postDataBytes.Length;

            //データをPOST送信するためのStreamを取得
            Stream reqStream = req.GetRequestStream();
            //送信するデータを書き込む
            reqStream.Write(postDataBytes, 0, postDataBytes.Length);
            reqStream.Close();

            //サーバーからの応答を受信するためのWebResponseを取得
            WebResponse res = req.GetResponse();
            //応答データを受信するためのStreamを取得
            Stream resStream = res.GetResponseStream();
            //受信して表示
            StreamReader sr = new StreamReader(resStream, enc);

            string str = sr.ReadToEnd();
            Console.WriteLine($"Str==== {str}");
            if (str == "") return;

            var test = new List<string>();
            test.AddRange(str.Split(','));
            if (test[0] == "Not") return;

            //サーバーから受け取った文字列を分割
            var list = new List<string>();
            
            list.AddRange(str.Split('|'));
            foreach (string tmp in list)
            {
                string Title;
                string Message;
                string CreateDate;
                string StartDate;
                string AuthorName;
                string id;
                if (tmp != "")
                {
                    string[] arr = tmp.Split(',');
                    id = arr[0];
                    Title = arr[1];
                    Message = arr[2];
                    AuthorName = "送信者：" + arr[3];
                    CreateDate = "作成日：" + arr[4];
                    StartDate = "発信日：" + arr[5];
                    Console.WriteLine($"temp={tmp}");
                    NotificationClass n = new NotificationClass();
                    n.Id = id;
                    n.Title = Title;
                    n.Message = Message;
                    n.AuthorName = AuthorName;
                    n.CreateDate = CreateDate;
                    n.StartDate = StartDate;
                    n.IsOpened = false;

                    Notifications.Insert(0, n);

                }


            }


            //閉じる
            sr.Close();

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
            //Image img ;
            //Image img2 = new Image();

            iTabItem.Header = _project.MainImages[MainImageIndex].MainImageName;
            Image img = _project.MainImages[MainImageIndex].MainImage;

            img.Stretch = Stretch.Fill;
            

            MainTabItemUserControl mtc = new MainTabItemUserControl();

            mtc.Name = "MainTabItem";
            mtc.MainImage = img;
            iTabItem.Content = mtc;

            //2021.08.22 メインタブの画像に切り出し矩形を作成 再描画変更
            mtc.RedrawRectangle();
            //foreach (CharaImageClass c in _project.MainImages[MainImageIndex].CharaImages)
            //{
            //    inkCanvas_DrawRectangle(mtc.inkCanvas, c.CharaRect);
            //}

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
            ctc.MainIndex = MainImageIndex;
            ctc.CharaIndex = CharaImageIndex;
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
            //System.Diagnostics.Debug.WriteLine("  ttt---Height = " + _project.MainImages[MainImageIndex].CharaImages[tIndex].CharaRect.Height + "  Width = " + _project.MainImages[MainImageIndex].CharaImages[tIndex].CharaRect.Width);
            //System.Diagnostics.Debug.WriteLine($"　CharaImage[tIndex = {tIndex} : Name = {_project.MainImages[MainImageIndex].CharaImages[tIndex].CharaImageName} ]");
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
            foreach (MainImageClass m in _project.MainImages)
            {
                if (m.MainImageName == tabName)
                {
                    mc.MainOrChara = 1;
                    mc.MainIndex = mainIndex;
                    mc.CheckFlag = true;
                }
                charaIndex = 0;
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
                    System.Diagnostics.Debug.WriteLine("CharaTab -- " + mc.Name + ": mainIndex = " + mc.MainIndex + ": charaIndex = " + mc.CharaIndex);

                    _charaImageIndex = mc.CharaIndex;
                    //切り出しパネルのバインディングを変更
                    CharaImages.ItemsSource = _project.MainImages[mc.MainIndex].CharaImages;
                    MainImageIndex = mc.MainIndex;
                    
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
        public byte[] ToBinary(BitmapSource img)
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

        private async void MsgBoxShow(string str)
        {
            await this.ShowMessageAsync("CharacomImager Ex", str);
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
                MsgBoxShow("キャンセルされました。");
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
            MsgBoxShow("JPEGファイルに書き出しました。");
        }

       
        /// <summary>
        /// 2021.08.16 D.Honjyou 
        /// タブを閉じるボタンをクリックした時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseTabItem_Click(object sender, MouseButtonEventArgs e)
        {
            var tabItem = ((TextBlock)sender).FindAncestor<TabItem>();

            tabItem.IsSelected = true;
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
                System.Diagnostics.Debug.WriteLine($"削除するぜ！！------Index={MainImageIndex}-{CharaImageIndex}  --  {_project.MainImages[MainImageIndex].MainImageName},{ImageName}");
                _project.MainImages[MainImageIndex].CharaImages.RemoveAt(CharaImageIndex);
                //メイン画像の矩形を一度クリアする
                foreach (TabItem t in mainTab.Items)
                {
                    System.Diagnostics.Debug.WriteLine($"TabItem:MainImageName === [{t.Header}],[{_project.MainImages[MainImageIndex].MainImageName}]");
                    if (t.Header.ToString() == _project.MainImages[MainImageIndex].MainImageName)
                    {
                        System.Diagnostics.Debug.WriteLine("main！===" + _project.MainImages[MainImageIndex].MainImageName);
                        foreach (UserControl cc in t.FindChildren<UserControl>())
                        {
                            if (cc.Name == "MainTabItem")
                            {
                                MainTabItemUserControl mtc = (MainTabItemUserControl)cc;
                                //↓これはいらないかも2021.08.28 D.Honjyou
                                mtc.Name = "MainTabItem";
                                //矩形の再描画
                                mtc.RedrawRectangle();
                                
                            }

                        }
                    }
                }
                mainTab.Items.RemoveAt(mainTab.SelectedIndex);

            }
            else
            {
                string st = _project.MainImages[MainImageIndex].CharaImages[CharaImageIndex].CharaImageName;
                System.Diagnostics.Debug.WriteLine($"名前が違いました。。。mainWindow:{st} ImageName:{ImageName}...");
            }
        }

        /// <summary>
        /// 2021.08.12 D.Honjyou
        /// グリッド線を表示するかどうかが変更された時のイベント
        /// ChildTabをすべてチェックしてグリッド線を書き直し
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridLineCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (mainTab == null) return;
            
            foreach (TabItem t in mainTab.Items)
            {
                MainOrCharaClass mc = checkTabName(t.Header.ToString());
                if (mc.MainOrChara == 2)
                {
                    ((CharaTabItemUserControl)t.Content).DrawWaku(sender, e);
                }
            }
        }

        /// <summary>
        /// バージョン情報の表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemVersionInfo(object sender, RoutedEventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyName asmName = assembly.GetName();
            Version version = asmName.Version;
            //_ = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly,
            //                                  typeof(AssemblyTitleAttribute));
            AssemblyCopyrightAttribute asmCopyright = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(assembly,
                                                      typeof(AssemblyCopyrightAttribute));

            var build = version.Build;
            var revision = version.Revision;
            var baseDate = new DateTime(2000, 1, 1);

            System.Diagnostics.Debug.WriteLine($"build = {build}, revision = {revision}");
            string strVersion = "Version " + version.ToString();
            string strBuild = "ビルド日時 @" + baseDate.AddDays(build).AddSeconds(revision * 2);
            MsgBoxShow($"バージョン情報\n\n{strVersion}\n{strBuild}\n\n{asmCopyright.Copyright}");
        }

        #region PC名、IPアドレス取得
        string GetIPAddress()
        {
            string ipaddress = Dns.GetHostName();
            IPHostEntry ipentry = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in ipentry.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    ipaddress += "(" + ip.ToString() + ")";
            }

            return ipaddress;
        }
        #endregion
        #region 起動ログ送信
        /// <summary>
        /// 2021.08.28 D.Honjyou
        /// スタートログを送信する
        /// </summary>
        void StartLog_Send()
        {
            const string url = @"http://characom.sakuraweb.com/CharacomEX/insertLog.php";

            ServicePointManager.Expect100Continue = false;
            WebClient wc = new WebClient();
            //NameValueCollectionの作成
            System.Collections.Specialized.NameValueCollection ps =
                new System.Collections.Specialized.NameValueCollection();
            //送信するデータ（フィールド名と値の組み合わせ）を追加
            ps.Add("StartDateTime", DateTime.Now.ToString());
            ps.Add("IPAddr", GetIPAddress());
            /**
            string user_name = "misaki";
            if (File.Exists("user.ini"))
            {
                StreamReader sr = new StreamReader("user.ini");
                user_name = sr.ReadToEnd();
                sr.Close();
            }

            ps.Add("UserID", user_name);
            **/

            //ビルド情報
            Assembly asm = Assembly.GetExecutingAssembly();
            Version ver = asm.GetName().Version;
            DateTime StartDate, StartTime;
            StartDate = DateTime.Parse("2000/01/01");
            StartTime = DateTime.Parse("0:0");
            string lblBuild = StartDate.AddDays(ver.Build).ToShortDateString() + "@" + StartTime.AddSeconds((double)ver.Revision * 2).ToShortTimeString();

            ps.Add("VersionInfo", ver.ToString());
            ps.Add("BuildDate", lblBuild);
            try
            {
                //データを送信し、また受信する
                byte[] resData = wc.UploadValues(url, ps);
                //受信したデータを表示する
                //string resText = System.Text.Encoding.UTF8.GetString(resData);
                //Console.WriteLine($"----StartLog()---{resText}");
            }
            catch
            {
                //Network = false;
            }
            wc.Dispose();

            
        }
        #endregion

        /// <summary>
        /// 通知ウィンドウの表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowNotificationWindow(object sender, RoutedEventArgs e)
        {
            NotificationWindow win = new NotificationWindow();
            win.ShowDialog();
        }

        /// <summary>
        /// 2021.08.21 D.Honjyou
        /// メインウィンドウ終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveNotifications();
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

    public static class VisualTreeHelperWrapper
    {
        /// <summary>
        /// VisualTreeを親側にたどって、
        /// 指定した型の要素を探す
        /// </summary>
        public static T FindAncestor<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            while (depObj != null)
            {
                if (depObj is T target)
                {
                    return target;
                }
                depObj = VisualTreeHelper.GetParent(depObj);
            }
            return null;
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
