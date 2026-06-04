using ICSharpCode.AvalonEdit;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
        int currID = 0;

        List<openFileObject> openFiles = new List<openFileObject>();

        public MainWindow()
        {
            InitializeComponent();
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                InitializeWebView();
            }
            filesTabs.ItemsSource = openFiles;
            openFileObject openedFile = new openFileObject("", "", "Unamed File");
            openFiles.Add(openedFile);
            filesTabs.SelectedItem = openedFile;
        }

        private TextEditor CurrentTextEditor
        {
            get
            {
                if (filesTabs.SelectedIndex == -1) return null;
                var cp = filesTabs.Template.FindName("PART_SelectedContentHost", filesTabs) as ContentPresenter;
                if (cp == null) return null;
                return FindVisualChild<TextEditor>(cp);
            }
        }

        private void filesTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source == filesTabs && filesTabs.SelectedIndex >= 0 && filesTabs.SelectedIndex < openFiles.Count)
            {
                currID = filesTabs.SelectedIndex;

                if (visualWebTester != null && visualWebTester.CoreWebView2 != null)
                {
                    visualWebTester.CoreWebView2.NavigateToString(openFiles[currID].content ?? "");
                }

                if (CurrentTextEditor != null)
                {
                    CurrentTextEditor.TextChanged -= textEditor_TextChanged;

                    CurrentTextEditor.Text = openFiles[currID].content ?? "";

                    CurrentTextEditor.TextChanged += textEditor_TextChanged;
                }
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
        private void saveAsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            createNewile();
        }
        private void saveMenuItem_Click(object sender, RoutedEventArgs e) //runs when save button is clicked from file
        {
            if (CurrentTextEditor == null) return;

            try
            {
                openFiles[currID].content = CurrentTextEditor.Text;
                File.WriteAllText(openFiles[currID].path, openFiles[currID].content);
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
                openFileObject openedFile = new openFileObject(filePath, fileContent, openFileDialog.SafeFileName);
                openFiles.Add(openedFile);

                currID = openFiles.Count - 1;

                filesTabs.ItemsSource = null;
                filesTabs.ItemsSource = openFiles;
                filesTabs.SelectedItem = openedFile;
            }
        }
        private void createNewile()
        {
            if (CurrentTextEditor == null) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "html files (*.html)|*.html|All files (*.*)|*.*";
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FilterIndex = 2;

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    openFiles[currID].content = CurrentTextEditor.Text;
                    openFiles[currID].path = saveFileDialog.FileName;
                    openFiles[currID].fileName = Path.GetFileName(saveFileDialog.FileName);

                    File.WriteAllText(openFiles[currID].path, openFiles[currID].content);

                    filesTabs.ItemsSource = null;
                    filesTabs.ItemsSource = openFiles;
                    filesTabs.SelectedIndex = currID;

                    System.Windows.MessageBox.Show("File saved successfully!");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Failed to save: " + ex.Message);
                }
            }
        }
        private void newFile()
        {
            openFileObject openedFile = new openFileObject(null, "", "unamed file");
            openFiles.Add(openedFile);

            filesTabs.ItemsSource = null;
            filesTabs.ItemsSource = openFiles;
            filesTabs.SelectedItem = openedFile;
            currID = openFiles.Count - 1;
        }
        private void newMenuItem_Click(object sender, RoutedEventArgs e)
        {
            newFile();
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
            if (visualWebTester != null && visualWebTester.CoreWebView2 != null && CurrentTextEditor != null)
            {
                string content = CurrentTextEditor.Text;

                visualWebTester.CoreWebView2.NavigateToString(content);
            }
        }
        private void textEditor_TextChanged(object sender, EventArgs e)
        {
            if (sender is TextEditor editor)
            {
                if (currID >= 0 && currID < openFiles.Count)
                {
                    openFiles[currID].content = editor.Text;
                }
            }
            updateWebView();
        }

        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T t) return t;
                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null) return childOfChild;
            }
            return null;
        }
    }
    public class openFileObject
    {
        public string fileName { get; set; }
        public string path { get; set; }
        int iD;
        public string content { get; set; }
        public openFileObject(string pth, string cont, string fName)
        {
            this.path = pth;
            iD = MainWindow.globalIDIndex;
            this.content = cont;
            this.fileName = fName;
            MainWindow.incramentGlobalID();
        }
    }
}