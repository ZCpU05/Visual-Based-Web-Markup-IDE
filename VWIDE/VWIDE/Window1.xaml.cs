using System;
using System.Windows;
using System.Windows.Controls;

namespace VWIDE
{
    public partial class Window1 : Window //UI and UI Logic for downloading plugins
    {
        string[] binaries = ["python", "nodeJS"];
        string[] exes = ["python.exe", "node.exe"];
        public Window1()
        {
            InitializeComponent();
            installCheck();
            Updater updater = new Updater();
            updater.missingFileHandler();
        }
        void installCheck()
        {
            downloader dl = new downloader();

            for (int i = 0; i < binaries.Length; i++)
            {
                string bin = binaries[i];
                string exe = exes[i];

                bool installed = dl.isInstalled(exe);

                if (FindName($"{bin}IN") is Button installButton && FindName($"{bin}UN") is Button uninstallButton)
                {
                    if (installed)
                    {
                        installButton.Visibility = Visibility.Collapsed;
                        uninstallButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        installButton.Visibility = Visibility.Visible;
                        uninstallButton.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
        private async void download_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                int langTarget = Convert.ToInt32(element.Uid);
                downloader dl = new downloader();
                await dl.download(langTarget);
            }
            installCheck();
        }

        private void uninstall_Click(object sender, RoutedEventArgs e)
        {
            downloader dl = new downloader();
            if (sender is FrameworkElement element)
            {
                int langTarget = Convert.ToInt32(element.Uid);
                dl.uninstall(binaries[langTarget]);
            }
            installCheck();
        }
    }
}
