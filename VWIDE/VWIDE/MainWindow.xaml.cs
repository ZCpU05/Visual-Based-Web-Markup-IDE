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
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                InitializeWebView();
            }
        }

        private async void InitializeWebView()
        {
            await visualWebTester.EnsureCoreWebView2Async(null);
            updateWebView();
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
        private void getFile() // calls an open file dialogue and gets the selected file
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
                openFileObject openedFile = new openFileObject(filePath);
                openFiles.Add(openedFile);

                currID = openFiles.Count - 1;

                textEditor.Text = fileContent;
            }
        }
        public static void incramentGlobalID()
        {
            globalIDIndex++;
        }
        public static void decramentGlobalID()
        {
            globalIDIndex--;
        }
        private void updateWebView()
        {
            if(visualWebTester != null && visualWebTester.CoreWebView2 != null)
            {
                string content = textEditor.Text;

                visualWebTester.CoreWebView2.NavigateToString(content);
            }
        }
        private void textEditor_TextChanged(object sender, EventArgs e)
        {
            updateWebView();
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