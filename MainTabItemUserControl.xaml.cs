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

        public MainTabItemUserControl()
        {
            InitializeComponent();
        }
        
        public void ClearRectView()
        {
            inkCanvas.Children.Clear();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = new { MainImage = MainImage.Source };
        }
    }
}
