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
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;


namespace CharacomEx
{
    /// <summary>
    /// NotificationInfoWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class NotificationInfoWindow : MetroWindow
    {
        NotificationClass _notification;
       
        public NotificationInfoWindow()
        {
            InitializeComponent();
        }

        public NotificationClass Notification { get => _notification; set => _notification = value; }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string title = _notification.Title;
            string message = _notification.Message;
            this.DataContext = _notification;
            //this.Content = new {_notification.Title, _notification.Message, _notification.AuthorName, _notification.StartDate, _notification.CreateDate };
            Console.WriteLine($"title===={_notification.Title}");
        }

        private void btnOK_Clicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
