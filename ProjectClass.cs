using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections;
using System.Collections.ObjectModel;

namespace CharacomEx
{
    /// <summary>
    /// ProjectClass
    /// 2021.07.24 D.Honjyou
    /// プロジェクト全体を管理するクラス
    /// </summary>
    [Serializable()]
    public sealed class ProjectClass
    {
        private string _projectTitle;
        private string _projectName;
        private string _projectFileName;
        private ObservableCollection<MainImageClass> mainImages = new ObservableCollection<MainImageClass>();


        public string ProjectTitle { get => _projectTitle; set => _projectTitle = value; }
        public string ProjectName { get => _projectName; set => _projectName = value; }
        public string ProjectFileName { get => _projectFileName; set => _projectFileName = value; }
        internal ObservableCollection<MainImageClass> MainImages { get => mainImages; set => mainImages = value; }
    }

    /// <summary>
    /// MainImageFlass
    /// 2021.07.24 D.Honjyou
    /// 資料画像１枚を管理するクラス
    /// </summary>
    [Serializable()]
    class MainImageClass
    {
        private string _mainImageTitle;
        private string _mainImageName;
        private byte[] img;
        [NonSerialized()]
        private Image _mainImage = new Image();
        private ObservableCollection<CharaImageClass> charaImages = new ObservableCollection<CharaImageClass>();

        public string MainImageTitle { get => _mainImageTitle; set => _mainImageTitle = value; }
        public string MainImageName { get => _mainImageName; set => _mainImageName = value; }
        public Image MainImage { get => _mainImage; set => _mainImage = value; }
        public byte[] Img { get => img; set => img = value; }
        internal ObservableCollection<CharaImageClass> CharaImages { get => charaImages; set => charaImages = value; }
    }

    /// <summary>
    /// CharaImageClass
    /// 2021.07.24 D.Honjyou
    /// 切り出し矩形画像を管理するクラス
    /// </summary>
    [Serializable()]
    class CharaImageClass
    {
        private string _charaImageTitle;
        private string _charaImageName;
        private Rect _charaRect = new Rect();
        private byte[] img;
        [NonSerialized()]
        private Image _charaImage = new Image();
        
        public string CharaImageTitle { get => _charaImageTitle; set => _charaImageTitle = value; }
        public string CharaImageName { get => _charaImageName; set => _charaImageName = value; }
        public Image CharaImage { get => _charaImage; set => _charaImage = value; }
        public Rect CharaRect { get => _charaRect; set => _charaRect = value; }
        public byte[] Img { get => img; set => img = value; }
    }

    class MainOrCharaClass
    {
        private bool _checkFlag;
        private string _name;
        private int _mainOrChara;
        private int _mainIndex;
        private int _charaIndex;

        public bool CheckFlag { get => _checkFlag; set => _checkFlag = value; }
        public string Name { get => _name; set => _name = value; }
        /// <summary>
        /// MainOrChara 1->メイン画像　2->個別文字画像
        /// </summary>
        public int MainOrChara { get => _mainOrChara; set => _mainOrChara = value; }
        public int MainIndex { get => _mainIndex; set => _mainIndex = value; }
        public int CharaIndex { get => _charaIndex; set => _charaIndex = value; }
    }

    /// <summary>
    /// 2021.08.20
    /// 通知のためのクラス
    /// </summary>
    [Serializable()]
    public class NotificationClass
    {
        private string _title;
        private string _authorName;
        private string _message;
        private string _createDate;
        private string _startDate;
        private string _id;
        private bool _isOpened;

        public string Title { get => _title; set => _title = value; }
        public string Message { get => _message; set => _message = value; }
        public string Id { get => _id; set => _id = value; }
        public bool IsOpened { get => _isOpened; set => _isOpened = value; }
        public string AuthorName { get => _authorName; set => _authorName = value; }
        public string CreateDate { get => _createDate; set => _createDate = value; }
        public string StartDate { get => _startDate; set => _startDate = value; }
    }
}
