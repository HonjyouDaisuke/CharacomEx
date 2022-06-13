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
using System.Windows.Shapes;

namespace CharacomEx
{
    /// <summary>
    /// ReplaceWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ReplaceWindow : Window
    {
        private Boolean isCancel;
        private string search;
        private string replace;

        public ReplaceWindow()
        {
            InitializeComponent();
        }

        public bool IsCancel { get => isCancel; set => isCancel = value; }
        public string Search { get => search; set => search = value; }
        public string Replace { get => replace; set => replace = value; }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.isCancel = true;
            this.Close();
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            //データチェック
            if(SearchString.Text == "")
            {
                MessageBox.Show("検索文字を入れてください。");
                return;
            }

            search = SearchString.Text;
            replace = ReplaceString.Text;

            this.Close();
        }
    }
}
