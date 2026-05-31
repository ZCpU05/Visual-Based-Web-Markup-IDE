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
    /// 
    public partial class MainWindow : Window
    {
        bool phpEnabled = false;
        public static int globalIDIndex = 0;
        int currID = 0; //update to null by default later

        List<openFileObject> openFiles = new List<openFileObject>();

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
            try
            {
                File.WriteAllText(openFiles[currID].path, textEditor.Text);
                System.Windows.MessageBox.Show("file saved!");
            }
            catch 
            {
                System.Windows.MessageBox.Show("unforseen error file failed to save!");
            }
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
            openFileObject openedFile = new openFileObject(filePath);
            openFiles.Add(openedFile);
        }
        public static void incramentGlobalID()
        {
            globalIDIndex++;
        }
        public static void decramentGlobalID()
        {
            globalIDIndex--;
        }
    }
    public class openFileObject
    {
        //public string fileName { get; set; }
        public string path { get; set; }
        int iD;
        public openFileObject(string pth)
        {
            path = pth;
            iD = MainWindow.globalIDIndex;
            MainWindow.incramentGlobalID();
        }
    }
}