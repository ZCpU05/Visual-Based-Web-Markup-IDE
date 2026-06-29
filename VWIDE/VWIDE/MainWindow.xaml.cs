using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using LibGit2Sharp;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Diagnostics;
using System.Windows.Media;

namespace VWIDE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        settingsManager settingManager = new settingsManager();

        bool phpEnabled;
        bool darkMode;
        bool projectOpen;
        bool reporistoryEnabled;

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

            phpEnabled = !settingManager.getSetting(1); //line 1 in config.txt
            darkMode = !settingManager.getSetting(2); //line 2 in config.txt
            projectOpen = !settingManager.getSetting(3); //line 3 in config.txt
            reporistoryEnabled = !settingManager.getSetting(4); //line 4 in config.txt

            this.Loaded += mainWindowLoaded;
        }
        private void mainWindowLoaded(object sender, RoutedEventArgs e)
        {
            phpEnable();
            darkModeEnable();
        }

        public async Task<string> runPHP(string phpCode)
        {
            string phpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries", "php", "php.exe");

            if (!File.Exists(phpPath))
            {
                MessageBox.Show("PHP executable not found.");
                return null;
            }

            string tempFilePath = Path.Combine(Path.GetTempPath(), "vwide_preview.php");
            await File.WriteAllTextAsync(tempFilePath, phpCode, Encoding.UTF8);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = phpPath,
                Arguments = $"\"{tempFilePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    process.WaitForExit();

                    if (File.Exists(tempFilePath)) File.Delete(tempFilePath);

                    return string.IsNullOrEmpty(error) ? output : $"PHP Error: {error}";
                }
            }
            catch (Exception ex)
            {
                return $"Process Error: {ex.Message}";
            }
        }

        private TextEditor CurrentTextEditor
        {
            get
            {
                if (filesTabs.SelectedIndex == -1)
                {
                    return null;
                }
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

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (CurrentTextEditor != null)
                    {
                        CurrentTextEditor.TextChanged -= textEditor_TextChanged;

                        CurrentTextEditor.Text = openFiles[currID].content ?? "";

                        CurrentTextEditor.TextChanged += textEditor_TextChanged;
                    }

                    updateWebView();
                }), System.Windows.Threading.DispatcherPriority.Render);
            }
        }

        private void InitializeWebView()
        {
            _ = visualWebTester.EnsureCoreWebView2Async(null);
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
        private void phpEnable_Click(object sender, RoutedEventArgs e) //switches the php flag to enabled and adds activates the php server
        {
            phpEnable();
            if (phpEnabled == false)
            {
                MessageBox.Show("php server Disabled");
            }
            else
            {
                MessageBox.Show("php server Enabled");
            }
        }
        private void phpEnable()
        {
            if (phpEnabled == false)
            {
                phpEnabled = true;
            }
            else
            {
                phpEnabled = false;
            }
        }
        private void darkMode_Click(object sender, RoutedEventArgs e)
        {
            darkModeEnable();
        }
        private void darkModeEnable()
        {
            if (darkMode == false)
            {
                darkMode = true;
                CurrentTextEditor.Background = Brushes.Black;
                CurrentTextEditor.Foreground = Brushes.White;
            }
            else
            {
                darkMode = false;
                CurrentTextEditor.Background = Brushes.White;
                CurrentTextEditor.Foreground = Brushes.Black;
            }
            CurrentTextEditor.TextArea.TextView.Redraw();
        }
        private void getFile() // calls an open file dialogue and gets the selected file
        {
            string fileContent = string.Empty;
            string filePath = string.Empty;

            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.InitialDirectory = "c:\\";
            if (!phpEnabled)
            {
                openFileDialog.Filter = "html files (*.html)|*.html|css files (*.css)|*.css|js files (*.js)|*.js|All files (*.*)|*.*";
            }
            else
            {
                openFileDialog.Filter = "php files (*.php)|*.h|html files (*.html)|*.html|css files (*.css)|*.css|js files (*.js)|*.js|All files (*.*)|*.*";
            }
            openFileDialog.FilterIndex = 1;
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

            if (!phpEnabled)
            {
                saveFileDialog.Filter = "html files (*.html)|*.html|css files (*.css)|*.css|js files (*.js)|*.js|All files (*.*)|*.*";
            }
            else
            {
                saveFileDialog.Filter = "php files (*.php)|*.h|html files (*.html)|*.html|css files (*.css)|*.css|js files (*.js)|*.js|All files (*.*)|*.*";
            }
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FilterIndex = 1;

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
        private async void updateWebView()
        {
            if (visualWebTester != null && visualWebTester.CoreWebView2 != null)
            {
                string content = "";
                if (CurrentTextEditor != null)
                {
                    content = CurrentTextEditor.Text;
                }
                else if (currID >= 0 && currID < openFiles.Count)
                {
                    content = openFiles[currID].content ?? "";
                }

                if (phpEnabled)
                {
                    string htmlResult = await runPHP(content);

                    await visualWebTester.EnsureCoreWebView2Async();
                    visualWebTester.NavigateToString(htmlResult);
                }
                else
                {
                    visualWebTester.CoreWebView2.NavigateToString(content);
                }
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

        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button closeButton && closeButton.Tag is openFileObject fileToRemove)
            {
                e.Handled = true;

                if (openFiles.Count <= 1)
                {
                    MessageBox.Show("Cannot close the last open file.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                bool closingSelectedTab = (filesTabs.SelectedItem == fileToRemove);

                openFiles.Remove(fileToRemove);
                MainWindow.decramentGlobalID();

                filesTabs.ItemsSource = null;
                filesTabs.ItemsSource = openFiles;

                if (closingSelectedTab)
                {
                    currID = openFiles.Count - 1;
                    filesTabs.SelectedIndex = currID;
                }
                else
                {
                    currID = filesTabs.SelectedIndex;
                }
            }
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

    public class settingsManager
    {
        string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");
        public settingsManager()
        {

        }
        
        public bool getSetting(int lineNum)
        {
            try
            {
                string[] lines = File.ReadAllLines(settingsPath);
                return Convert.ToBoolean(lines[lineNum - 1]);
            }
            catch
            {
                MessageBox.Show("Fatal error: Config file missing");
                return false;
            }
        }
        void saveSetting(bool[] settings)
        {

        }
        string getDefaultProjectPath()
        {
            return "";
        }
        int getFontSize()
        {
            return 14;
        }
    }
}