using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;


namespace CharacomEx
{
    /// <summary>
    /// NotificationWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class NotificationWindow : MetroWindow
    {
        //private ObservableCollection<NotificationClass> notifications = new ObservableCollection<NotificationClass>();
        //internal ObservableCollection<NotificationClass> Notifications { get => notifications; set => notifications = value; }

        public NotificationWindow()
        {
            InitializeComponent();
        }

        

        private void MessageDoubleClicked(object sender, MouseButtonEventArgs e)
        {

            NotificationInfoWindow win = new NotificationInfoWindow();
            var Oya = (MainWindow)Application.Current.MainWindow;

            win.Notification = Oya.Notifications[NotificationsList.SelectedIndex];
            Oya.Notifications[NotificationsList.SelectedIndex].IsOpened = true;
            NotificationsList.ItemsSource = Oya.Notifications;
            DoEvents();
            win.ShowDialog();
        }

        private void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            var callback = new DispatcherOperationCallback(obj =>
            {
                ((DispatcherFrame)obj).Continue = false;
                return null;
            });
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, callback, frame);
            Dispatcher.PushFrame(frame);
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("--showshow");
            /**
            foreach (NotificationClass n in notifications)
            {
                Console.WriteLine(n.Title);
            }
            **/

            var Oya = (MainWindow)Application.Current.MainWindow;
            
            NotificationsList.ItemsSource = Oya.Notifications;
        }

        private void btnOK_Clicked(object sender, RoutedEventArgs e)
        {
            var Oya = (MainWindow)Application.Current.MainWindow;
            Oya.test();
            this.Close();
        }
    }
}
