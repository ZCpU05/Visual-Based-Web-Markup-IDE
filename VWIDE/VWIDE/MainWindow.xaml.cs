using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Indentation;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;
//using LibGit2Sharp;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Diagnostics;
using System.Windows.Media;
using External_Langauage_Manager;
using System.Diagnostics.Eventing.Reader;
using System.Xml;

/*
---TO DO---
cross file calling support
Comment Over all code

create updater - file healer and plugin updater done!
error handling junk
start debugging and testing the release build (Re-route everything through appdata files rather than internal ones)
*/

namespace VWIDE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        settingsManager settingManager = new settingsManager(); //creates object to handle all settings
        //credentialManager credentialManager = new credentialManager();

        bool phpEnabled; //group of settings that handle the functionality of settings in the program
        bool darkMode;
        bool projectOpen;
        bool reporistoryEnabled;

        bool githubEnabled = false; //unused in v1.0.0

        int fontSize = 0; //varrible for font size to be set to, writes it to the avlonedit to set font size.

        public static int globalIDIndex = 0; //Index for files, this is to handle the internal logic for tracking what file contains what.
        int currID = 0; //The current file the user is on. 

        string currentProjPath = string.Empty; //Set to default on startup allocated later, created here to be accessed globally
        string currentGitProjPath = string.Empty;

        List<openFileObject> openFiles = new List<openFileObject>(); //List holds all the open files as well as there information

        List<string> settingsAsOpened = new List<string>(); //these two lists track the differences between settings on start up and settings as of run time. Used to track certain things
        List<string> settingsUpdated = new List<string>();

        Dictionary<string, IPlugin> plugins = new Dictionary<string, IPlugin>(); //Data structures that hold both offical and custom plugins respectivly
        List<customBinary> customBinarys = new List<customBinary>();

        Dictionary<string, bool> supportedExtensions = new Dictionary<string, bool>(); //A dictionary containing all the supported file extensions and if they support the web view

        public MainWindow() //function that loads things on start up
        {
            InitializeComponent();

            openFileObject openedFile = new openFileObject("", "", "Unamed File"); //creates a deafult file to operate on. 
            openFiles.Add(openedFile);
            filesTabs.SelectedItem = openedFile;
            filesTabs.ItemsSource = openFiles;

            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) //Sets up the webview
            {
                InitializeWebView();
            }
            
            clearUninstalls(); //any files in the uninstall folder such as dlls are deleted 

            Updater updater = new Updater();
            updater.missingFileHandler();
            updater.pluginLinkUpdater();
            updater.programUpdater();

            phpEnabled = settingManager.getSetting(1); //line 1 in config.txt
            darkMode = settingManager.getSetting(2); //line 2 in config.txt
            projectOpen = settingManager.getSetting(3); //line 3 in config.txt
            reporistoryEnabled = settingManager.getSetting(4); //line 4 in config.txt
            fontSize = settingManager.getFontSize();
            //Above is the settings being initalised.

            settingsAsOpened.Add(Convert.ToString(phpEnabled));
            settingsAsOpened.Add(Convert.ToString(darkMode));
            settingsAsOpened.Add(Convert.ToString(projectOpen));
            settingsAsOpened.Add(Convert.ToString(reporistoryEnabled));
            settingsAsOpened.Add(Convert.ToString(fontSize));

            settingsUpdated.Add(Convert.ToString(phpEnabled));
            settingsUpdated.Add(Convert.ToString(darkMode));
            settingsUpdated.Add(Convert.ToString(projectOpen));
            settingsUpdated.Add(Convert.ToString(reporistoryEnabled));
            settingsUpdated.Add(Convert.ToString(fontSize));
            //sets up the settings comparison arrays

            fontSizeBox.Text = fontSize.ToString(); //sets font size

            supportedExtensions.Add(".html", true); //allocates the different types of extensions and if they use the web view or not
            supportedExtensions.Add(".css", false);
            supportedExtensions.Add(".js", false);
            supportedExtensions.Add(".php", true);
            supportedExtensions.Add(".txt", false);

            loadExternalPlugins(); //Loads each type of plugins into memory
            loadCustomBinaries();

            this.Loaded += mainWindowLoaded; //fires an event to signal when the window is fully loaded
        }

        private void mainWindowLoaded(object sender, RoutedEventArgs e) //function that fires upon the main window fully loading, prevents errors due to certain elements not exisiting yet due to loading order. 
        {
            if (phpEnabled) //for some reason the settings only on start up only worked if they were inversed by im going to double check that
            {
                darkMode = !darkMode;
                darkModeEnable();
            }
            else
            {
                darkMode = !darkMode;
                darkModeEnable();
            }

            if (darkMode)
            {
                darkMode = false;
                darkModeEnable();
            }
            else
            {
                darkMode = true;
                darkModeEnable();
            }

            string projPath = settingManager.getDefaultProjectPath(); //sets the default project path
            if (projPath != null && Directory.Exists(projPath)) //if there is a default project path allocate it to the directory viewer, 
            {
                var rootItem = new TreeViewItem
                {
                    Header = Path.GetFileName(projPath),
                    Tag = projPath,
                    IsExpanded = true
                };

                fileDirecotryView.Items.Add(rootItem);

                populateDirectory(projPath, rootItem);
            }

            //Not in v1.0.0
            /*if (credentialManager.getCredentials() != null)
            {
                githubEnabled = true;
                gitButtonManager();

                if (reporistoryEnabled)
                {
                    string gitPath = settingManager.getDefaultGitRepoPath();

                    if (gitPath != null && Directory.Exists(gitPath))
                    {
                        currentGitProjPath = gitPath;

                        var rootItem = new TreeViewItem
                        {
                            Header = Path.GetFileName(gitPath) + " [Git Repo]",
                            Tag = gitPath,
                            IsExpanded = true
                        };

                        gitDirecotryView.Items.Add(rootItem);

                        populateDirectory(gitPath, rootItem);
                    }
                }
            }*/

            CurrentTextEditor.FontSize = fontSize;

            settingsNotif.Visibility = Visibility.Hidden;

            //gitButtonManager();

            Assembly assembly = Assembly.GetExecutingAssembly();//loads syntax highlighting rules from embbeded XHML file
            string editorRules = "VWIDE.bin.editorRules.xshd";
            using (Stream stream = assembly.GetManifestResourceStream(editorRules))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException("Critical error resource could not be opened");
                }
                using (System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader(stream))
                {
                    CurrentTextEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }

            SearchPanel.Install(CurrentTextEditor); //installs find and replace pannel
        }

        public async Task<string> runPHP(string phpCode) //This function runs php code 
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

        private TextEditor CurrentTextEditor //creates a text editor, ties it with the openFiles list allowing its content to be created and destoryed depending on the open file
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

        private void filesTabs_SelectionChanged(object sender, SelectionChangedEventArgs e) //method to handle when a tab is changed on file
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
                    SearchPanel.Install(CurrentTextEditor);
                    defaultLanguage(Path.GetExtension(openFiles[currID].path));
                }), System.Windows.Threading.DispatcherPriority.Render);
            }
        }

        private void InitializeWebView() //Sets up the webview
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

        private void phpEnable() //manages phps state
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
            if (darkMode == false) //Handles enabling and disabling dark mode
            {
                darkMode = true;
                if (CurrentTextEditor != null)
                {
                    CurrentTextEditor.Background = Brushes.Black;
                    CurrentTextEditor.Foreground = Brushes.White;
                }
            }
            else
            {
                darkMode = false;
                if (CurrentTextEditor != null)
                {
                    CurrentTextEditor.Background = Brushes.White;
                    CurrentTextEditor.Foreground = Brushes.Black;
                }
            }
            if (CurrentTextEditor != null)
            {
                CurrentTextEditor.TextArea.TextView.Redraw();
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

        private void createNewile() //Save As function
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

        private void newFile() //Creates a new unanmed file
        {
            openFileObject openedFile = new openFileObject(null, "", "Unamed file.Html");
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
        //These two functions handle the file ID changing it based of open or deleted files
        public static void decramentGlobalID()
        {
            globalIDIndex--;
        }

        private async void updateWebView() //Function updates the webview
        {
            visualWebTester.Visibility = Visibility.Collapsed;
            bool executeFlag = false;
            string extension = Path.GetExtension(openFiles[currID].path);
            string extensionComparison;
            foreach(var (ext, isSupported) in supportedExtensions) //Checks if its a webview supported extension
            {
               if (extension == ext && isSupported)
               {
                    executeFlag = true;
                    visualWebTester.Visibility = Visibility.Visible;
                    break;
               }
            }


            if (visualWebTester != null && visualWebTester.CoreWebView2 != null && executeFlag == true) //Runs the code
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

                string currPath = openFiles[currID].path;
                string currExtension = Path.GetExtension(currPath);

                if (currExtension == ".php" && phpEnabled) //this block runs the code as php
                {
                    string htmlResult = await runPHP(content);
                    await visualWebTester.EnsureCoreWebView2Async();
                    visualWebTester.NavigateToString(htmlResult);
                }
                else if (plugins.ContainsKey(currExtension)) //this block runs the code based of plugins
                {
                    string consoleResult = await plugins[currExtension].execute(content);
                    await visualWebTester.EnsureCoreWebView2Async();
                    visualWebTester.NavigateToString(consoleResult);
                }
                else //fallback block
                {
                    foreach (customBinary cb in customBinarys) //checks if custom install binaries can run the code
                    {
                        if (cb.fileExtension ==  currExtension)
                        {
                            string consoleResult = await cb.execute(content);
                            await visualWebTester.EnsureCoreWebView2Async();
                            visualWebTester.NavigateToString(consoleResult);
                            return;
                        }
                    }
                    visualWebTester.CoreWebView2.NavigateToString(content); //Raw html fallback
                }
            }
        }

        private void textEditor_TextChanged(object sender, EventArgs e)//Fires to update webview when text is typed in avalonedit
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

        private void CloseTab_Click(object sender, RoutedEventArgs e) //Closes file, removes it from openFiles
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

        void selectFolder_click(object sender, RoutedEventArgs e) //Opens dialog to set project directory
        {
            var fileDialog = new OpenFolderDialog
            {
                Title = "Select Project Folder",
                InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
            };

            if (fileDialog.ShowDialog() == true)
            {
                string selectedPath = fileDialog.FolderName;
                currentProjPath = selectedPath;

                fileDirecotryView.Items.Clear();

                var rootItem = new TreeViewItem
                {
                    Header = Path.GetFileName(selectedPath),
                    Tag = selectedPath,
                    IsExpanded = true
                };

                fileDirecotryView.Items.Add(rootItem);

                populateDirectory(selectedPath, rootItem);
            }
        }

        void populateDirectory(string path, TreeViewItem rootItem) //Populates the directory
        {
            try
            {
                foreach (string dir in Directory.GetDirectories(path))
                {
                    if (Path.GetFileName(dir).Equals(".git", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var dirItem = new TreeViewItem
                    {
                        Header = Path.GetFileName(dir),
                        Tag = dir
                    };
                    rootItem.Items.Add(dirItem);

                    populateDirectory(dir, dirItem);
                }
                foreach (string file in Directory.GetFiles(path))
                {
                    var fileItem = new TreeViewItem
                    {
                        Header = Path.GetFileName(file),
                        Tag = file
                    };
                    rootItem.Items.Add(fileItem);
                }
            }
            catch
            {
                //skips protected system folders and or any unforseen issues
            }
        }

        void openFileFromDir_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) //Opens file from file directory
        {
            if (e.NewValue is TreeViewItem selectedItem)
            {
                string fullPath = selectedItem.Tag as string;

                if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                {
                    try
                    {
                        string fileContent = File.ReadAllText(fullPath);
                        string fileName = Path.GetFileName(fullPath);

                        openFileObject openedFile = new openFileObject(fullPath, fileContent, fileName);
                        openFiles.Add(openedFile);

                        filesTabs.ItemsSource = null;
                        filesTabs.ItemsSource = openFiles;
                        filesTabs.SelectedItem = openedFile;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Could not open file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        void checkSetting_Click(object sender, RoutedEventArgs e) //checks for a setting missmatch between the two arrays
        {
            if (sender is FrameworkElement element)
            {
                int settingTarget = Convert.ToInt32(element.Uid);
                bool settingChange = !Convert.ToBoolean(settingsUpdated[settingTarget]);
                settingsUpdated[settingTarget] = Convert.ToString(settingChange);

                if (settingTarget == 0) phpEnabled = settingChange;
                if (settingTarget == 1) darkMode = settingChange;
                if (settingTarget == 2) projectOpen = settingChange;
                if (settingTarget == 3) reporistoryEnabled = settingChange;

                if (settingTarget == 2 && currentProjPath != string.Empty && projectOpen == true)
                {
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "defaultProjDir.txt");
                    File.WriteAllText(path, currentProjPath);
                    MessageBox.Show("Default Project Directory set too" + currentProjPath);
                }
                else if (settingTarget == 2 && currentProjPath == string.Empty)
                {
                    MessageBox.Show("Error: Please set a project directiory before setting a default");
                    projectOpen = false;
                }

                if (settingTarget == 3 && currentGitProjPath != string.Empty && githubEnabled == true)
                {
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "defaultGitRepo.txt");
                    File.WriteAllText(path, currentGitProjPath);
                    MessageBox.Show("Default GitHub Directory set too" + currentGitProjPath);
                }
                else if (settingTarget == 3 && currentGitProjPath == string.Empty)
                {
                    MessageBox.Show("Error: Please set a github repo directiory before setting a default");
                    reporistoryEnabled = false;
                }
            }
            settingMismatchCheck();
        }

        private void fontSizeBox_TextChanged(object sender, TextChangedEventArgs e) //Manages the font size to be saved and rejects bad inputs
        {
            try
            {
                Convert.ToInt32(fontSizeBox.Text);
                settingsUpdated[4] = fontSizeBox.Text;
            }
            catch
            {
                if (settingsAsOpened.Count > 4)
                {
                    settingsUpdated[4] = settingsAsOpened[4];
                    fontSizeBox.Text = settingsAsOpened[4];
                }
            }
            settingMismatchCheck();
        }

        private void saveSettings_Click(object sender, RoutedEventArgs e) //saves the settings to the config file
        {
            settingsUpdated[4] = fontSizeBox.Text;

            settingManager.saveSetting(settingsUpdated);

            settingsAsOpened = new List<string>(settingsUpdated);
            settingMismatchCheck();

            if (int.TryParse(fontSizeBox.Text, out int newSize) && CurrentTextEditor != null)
            {
                CurrentTextEditor.FontSize = newSize;
            }
        }

        void settingMismatchCheck() //notifies the user if the settings are changed 
        {
            if (!settingsUpdated.SequenceEqual(settingsAsOpened))
            {
                settingsNotif.Visibility = Visibility.Visible;
            }
            else
            {
                settingsNotif.Visibility = Visibility.Hidden;
            }
        }

        //not in v1.0.0
        /*public void gitButtonManager()
        {
            if (githubEnabled)
            {
                linkGit.Visibility = Visibility.Hidden;
                openReporisitory.Visibility = Visibility.Visible;
            }
            else
            {
                linkGit.Visibility = Visibility.Visible;
                openReporisitory.Visibility = Visibility.Hidden;
            }
        }*/

        /*void linkGit_Click(object sender, RoutedEventArgs e)
        {
            Window3 window3 = new Window3();
            window3.Owner = this;
            if (window3.ShowDialog() == true)
            {
                gitButtonManager();
            }
        }

        void clearGitHub_Click(object sender, RoutedEventArgs e)
        {
            credentialManager.clearedCredentials();
            githubEnabled = false;
            gitButtonManager();
            gitDirecotryView.Items.Clear();
        }

        void openReporsitory_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select Git Repository Config File",
                InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
                Filter = "Git Config (config)|config|All files (*.*)|*.*",
                FilterIndex = 0,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;
                string selectedFileName = Path.GetFileName(selectedFilePath);
                string parentDirName = Path.GetFileName(Path.GetDirectoryName(selectedFilePath));

                if (selectedFileName.Equals("config", StringComparison.OrdinalIgnoreCase) && parentDirName.Equals(".git", StringComparison.OrdinalIgnoreCase))
                {
                    string repoPath = Path.GetDirectoryName(Path.GetDirectoryName(selectedFilePath));

                    gitDirecotryView.Items.Clear();

                    var rootItem = new TreeViewItem
                    {
                        Header = Path.GetFileName(repoPath) + " [Git Repo]",
                        Tag = repoPath,
                        IsExpanded = true
                    };

                    gitDirecotryView.Items.Add(rootItem);

                    currentGitProjPath = repoPath;

                    populateDirectory(repoPath, rootItem);
                }
                else
                {
                    MessageBox.Show("Error Must select config file in hidden '.git' folder in reporsitory");
                }
            }
        }*/
        private void installBinaries_Click(object sender, RoutedEventArgs e) //takes the user into the offical binary install window
        {
            Window1 window1 = new Window1();
            window1.Owner = this;
            window1.ShowDialog();
        }
        void loadExternalPlugins() //loads installed plugins into memory
        {
            string pluginsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");

            string[] dllFiles = Directory.GetFiles(pluginsFolder, "*.dll");

            foreach (string file in dllFiles)
            {
                try
                {
                    Assembly pluginAssembly = Assembly.LoadFrom(file);

                    foreach (Type type in pluginAssembly.GetTypes())
                    {
                        if(typeof(IPlugin).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                        {
                            IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                            plugin.OnStartup();
                            supportedExtensions = plugin.extensionUpdater(supportedExtensions);
                            plugins.Add(plugin.extension, plugin);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("I am really evil and terrible and I kill rats for fun");
                }
            }
        }
        private void installCustomBinaries_Click(object sender, RoutedEventArgs e) //opens the window for installing and managing custom txt plugins
        {
            Window2 window2 = new Window2();
            window2.Owner = this;
            if((bool)window2.ShowDialog())
            {
                loadCustomBinaries();
            }
        }
        void loadCustomBinaries() //loads custom plugins
        {
            string cPluginFolderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "Custom Plugin");
            string cBinariesFolderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries", "Custom Binaries");

            if (Directory.Exists(cPluginFolderPath))
            {
                string[] txtFiles = Directory.GetFiles(cPluginFolderPath, "*.txt");
                foreach (string file in txtFiles)
                {
                    try
                    {
                        string langName = Path.GetFileNameWithoutExtension(file);

                        string[] lines = File.ReadAllLines(file);
                        if (lines.Length >= 2)
                        {
                            string exeFileName = lines[0].Trim();
                            string extension = "." + lines[1].Trim();
                            bool webViewCompatible = Convert.ToBoolean(lines[2].Trim());

                            string binaryPath = Directory.GetFiles(cBinariesFolderPath, exeFileName, SearchOption.AllDirectories)
                                .FirstOrDefault();

                            if (!string.IsNullOrEmpty(binaryPath) && File.Exists(binaryPath))
                            {
                                customBinary cb = new customBinary(binaryPath, extension, langName, webViewCompatible);
                                customBinarys.Add(cb);
                                supportedExtensions.Add("." + cb.fileExtension, cb.isWebViewCompatible);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("The bee population is in termoil");
                    }
                }
            }
        }
        void clearUninstalls() //clears folders from the uninstall folder
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uninstall");
            string[] files = Directory.GetFiles(filePath);
            foreach (string file in files)
            {
                File.Delete(file);
            }
        }

        private void undo_Click(object sender, RoutedEventArgs e) //General editor function buttons 950-985
        {
            CurrentTextEditor.Undo();
        }
        private void redo_Click(object sender, RoutedEventArgs e)
        {
            CurrentTextEditor.Redo();
        }
        private void cut_Click(object sender, RoutedEventArgs e)
        {
            CurrentTextEditor.Cut();
        }
        private void copy_Click(object sender, RoutedEventArgs e)
        {
            CurrentTextEditor.Copy();
        }
        private void paste_Click(object sender, RoutedEventArgs e)
        {
            CurrentTextEditor.Paste();
        }
        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            visualWebTester.CoreWebView2.Reload();
        }

        private async void ccaRefresh_Click(object sender, RoutedEventArgs e) //This refreshes and clear the web browsers cache, good for stuff style
        {
            await visualWebTester.CoreWebView2.CallDevToolsProtocolMethodAsync("Network.setCacheDisabled", "{\"cacheDisabled\": true}");
            visualWebTester.CoreWebView2.Reload();
            await visualWebTester.CoreWebView2.CallDevToolsProtocolMethodAsync("Network.setCacheDisabled", "{\"cacheDisabled\": false}");
        }
        private void findAndReplace_Click(object sender, RoutedEventArgs e)
        {
            var searchPanel = SearchPanel.Install(CurrentTextEditor);
            searchPanel.Open();
        }
        void defaultLanguage(string ext) //Sets default lanague for syntax highlighting
        {
            switch (ext)
            {
                case ".html":
                    CurrentTextEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("HTML");
                    break;
                case ".css":
                    CurrentTextEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("CSS");
                    break;
                case ".js":
                    CurrentTextEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("JavaScript");
                    break;
                case ".php":
                    CurrentTextEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("PHP");
                    break;

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
        public bool getSetting(int lineNum)
        {
            try
            {
                if (!File.Exists(settingsPath)) return false;
                string[] lines = File.ReadAllLines(settingsPath);
                if (lines.Length >= lineNum)
                {
                    if (bool.TryParse(lines[lineNum - 1], out bool result))
                    {
                        return result;
                    }
                }
                return false;
            }
            catch
            {
                MessageBox.Show("Fatal error: Config file missing");
                return false;
            }
        }

        public void saveSetting(List<string> settings)
        {
            try
            {
                string filePathSettings = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");
                string filePathFontSize = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fontSize.txt");

                File.WriteAllLines(filePathSettings, settings[0..4]);

                if (settings.Count > 4)
                {
                    File.WriteAllText(filePathFontSize, settings[4]);
                }
            }
            catch
            {

            }
        }

        public string getDefaultProjectPath()
        {
            string projectPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "defaultProjDir.txt");
            if (!File.Exists(projectPath)) return null;
            string ProjectDir = File.ReadAllText(projectPath);
            if (ProjectDir == "")
            {
                return null;
            }
            else
            {
                return ProjectDir;
            }
        }
        /*public string getDefaultGitRepoPath()
        {
            string projectPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "defaultGitRepo.txt");
            if (!File.Exists(projectPath)) return null;
            string ProjectDir = File.ReadAllText(projectPath);
            if (ProjectDir == "")
            {
                return null;
            }
            else
            {
                return ProjectDir;
            }
        }*/
        public int getFontSize()
        {
            string fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fontSize.txt");
            if (!File.Exists(fontPath)) return 12;
            string fontSize = File.ReadAllText(fontPath);
            return Convert.ToInt32(fontSize);
        }
    }
}