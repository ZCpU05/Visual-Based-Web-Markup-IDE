using ICSharpCode.AvalonEdit;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VWIDE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool phpEnabled = false;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void openMenuItem_Click(object sender, RoutedEventArgs e) //runs when open button is clicked from file
        {
            getFile();
        }
        private void saveMenuItem_Click(object sender, RoutedEventArgs e) //runs when save button is clicked from file
        {
            System.Windows.MessageBox.Show("Saving file");
        }
        private void getFile() //calls an open file dialouge and gets the selected file
        {
            string fileContent = string.Empty;
            string filePath = string.Empty;

            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.InitialDirectory = "c:\\";
            if (!phpEnabled)
            {
                openFileDialog.Filter = "html files (*.html)|*.html|All files (*.*)|*.*";
            }
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;

                using (Stream stream = openFileDialog.OpenFile())
                using (StreamReader reader = new StreamReader(stream))
                {
                    fileContent = reader.ReadToEnd();
                }
            }

            textEditor.Text = fileContent; //Writes the file contents to the text editor
        }
    }
}