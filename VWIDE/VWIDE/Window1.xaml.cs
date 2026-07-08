using System;
using System.Windows;

namespace VWIDE
{
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        private async void download_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                int langTarget = Convert.ToInt32(element.Uid);
                downloader dl = new downloader();
                await dl.download(langTarget);
            }
        }
    }
}
