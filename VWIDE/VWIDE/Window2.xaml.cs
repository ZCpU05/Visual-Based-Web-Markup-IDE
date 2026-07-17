using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VWIDE
{
    public partial class Window2 : Window
    {
        int globalY = 50;
        int customBinaryID = 0;
        string customBinaryPaths = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries", "Custom Binaries");
        Grid binaryContainer = new Grid();

        public Window2()
        {
            InitializeComponent();
            loadPrexistingCustomBinaries();
            this.Closing += Window2_Closing;
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        void incramentGlobalY()
        {
            globalY += 100;
            customBinaryID++;
        }

        private void newBinary_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                binaryContainer.Height = 90;
                binaryContainer.Margin = new Thickness(0, 0, 0, 10);
                binaryContainer.Tag = customBinaryID;

                Rectangle cbHolder = new Rectangle();
                cbHolder.Fill = Brushes.LightGray;
                cbHolder.RadiusX = 5;
                cbHolder.RadiusY = 5;
                cbHolder.Tag = customBinaryID;

                ComboBox exes = new ComboBox();
                exes.VerticalAlignment = VerticalAlignment.Center;
                exes.HorizontalAlignment = HorizontalAlignment.Left;
                exes.Width = 131;
                exes.Margin = new Thickness(20, 0, 0, 0);
                exes.Tag = "Exe_" + customBinaryID;

                if (Directory.Exists(customBinaryPaths))
                {
                    string[] exeFiles = Directory.GetFiles(customBinaryPaths, "*.exe", SearchOption.AllDirectories);
                    foreach (string exeFile in exeFiles)
                    {
                        exes.Items.Add(System.IO.Path.GetFileName(exeFile));
                    }
                }

                TextBox fileExtensionInput = new TextBox();
                fileExtensionInput.Width = 65;
                fileExtensionInput.HorizontalAlignment = HorizontalAlignment.Left;
                fileExtensionInput.VerticalAlignment = VerticalAlignment.Center;
                fileExtensionInput.Margin = new Thickness(152, 0, 0, 0);
                fileExtensionInput.Tag = "Ext_" + customBinaryID;

                Label extensionNotif = new Label();
                extensionNotif.Width = 73;
                extensionNotif.HorizontalAlignment = HorizontalAlignment.Left;
                extensionNotif.VerticalAlignment = VerticalAlignment.Center;
                extensionNotif.Margin = new Thickness(149, 0, 0, 35);
                extensionNotif.Content = "File extension";
                extensionNotif.Tag = customBinaryID;

                TextBox nameInput = new TextBox();
                nameInput.Width = 65;
                nameInput.HorizontalAlignment = HorizontalAlignment.Left;
                nameInput.VerticalAlignment = VerticalAlignment.Center;
                nameInput.Margin = new Thickness(232, 0, 0, 0);
                nameInput.Tag = "Name_" + customBinaryID;

                Label nameNotif = new Label();
                nameNotif.Width = 73;
                nameNotif.HorizontalAlignment = HorizontalAlignment.Left;
                nameNotif.VerticalAlignment = VerticalAlignment.Center;
                nameNotif.Margin = new Thickness(232, 0, 0, 35);
                nameNotif.Content = "Name";
                nameNotif.Tag = customBinaryID;

                Button saveCustomInstall = new Button();
                saveCustomInstall.Width = 50;
                saveCustomInstall.HorizontalAlignment = HorizontalAlignment.Left;
                saveCustomInstall.VerticalAlignment = VerticalAlignment.Center;
                saveCustomInstall.Margin = new Thickness(300, 0, 0, 0);
                saveCustomInstall.Content = "Save";
                saveCustomInstall.Click += save_Click;
                saveCustomInstall.Tag = customBinaryID;

                binaryContainer.Children.Add(cbHolder);
                binaryContainer.Children.Add(exes);
                binaryContainer.Children.Add(fileExtensionInput);
                binaryContainer.Children.Add(extensionNotif);
                binaryContainer.Children.Add(nameInput);
                binaryContainer.Children.Add(nameNotif);
                binaryContainer.Children.Add(saveCustomInstall);

                cBLayout.Children.Add(binaryContainer);
                incramentGlobalY();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not find new custom binary, place custom binary exe along with its other asociated files within the 'custom binaries' folder");
            }
        }

        void loadPrexistingCustomBinaries()
        {
            binaryContainer.Children.Clear();
            string folderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "Custom Plugin");
            if (Directory.Exists(folderPath))
            {
                string[] customPlugins = Directory.GetFiles(folderPath, "*.txt", SearchOption.AllDirectories);
                foreach (string plugin in customPlugins)
                {
                    if (File.Exists(plugin))
                    {
                        string[] lines = File.ReadAllLines(plugin);

                        try
                        {
                            binaryContainer.Height = 90;
                            binaryContainer.Margin = new Thickness(0, 0, 0, 10);
                            binaryContainer.Tag = customBinaryID;

                            Rectangle cbHolder = new Rectangle();
                            cbHolder.Fill = Brushes.LightGray;
                            cbHolder.RadiusX = 5;
                            cbHolder.RadiusY = 5;
                            cbHolder.Tag = customBinaryID;

                            ComboBox exes = new ComboBox();
                            exes.VerticalAlignment = VerticalAlignment.Center;
                            exes.HorizontalAlignment = HorizontalAlignment.Left;
                            exes.Width = 131;
                            exes.Margin = new Thickness(20, 0, 0, 0);
                            exes.Tag = "Exe_" + customBinaryID;
                            if (Directory.Exists(customBinaryPaths))
                            {
                                int i = 0;
                                string[] exeFiles = Directory.GetFiles(customBinaryPaths, "*.exe", SearchOption.AllDirectories);
                                foreach (string exeFile in exeFiles)
                                {
                                    exes.Items.Add(System.IO.Path.GetFileName(exeFile));
                                    if (lines.Length > 0)
                                    {
                                        string targetExe = lines[0].Trim();
                                        exes.SelectedItem = targetExe;
                                    }
                                }
                            }

                            TextBox fileExtensionInput = new TextBox();
                            fileExtensionInput.Width = 65;
                            fileExtensionInput.HorizontalAlignment = HorizontalAlignment.Left;
                            fileExtensionInput.VerticalAlignment = VerticalAlignment.Center;
                            fileExtensionInput.Margin = new Thickness(152, 0, 0, 0);
                            fileExtensionInput.Tag = "Ext_" + customBinaryID;
                            fileExtensionInput.Text = lines[0];

                            Label extensionNotif = new Label();
                            extensionNotif.Width = 73;
                            extensionNotif.HorizontalAlignment = HorizontalAlignment.Left;
                            extensionNotif.VerticalAlignment = VerticalAlignment.Center;
                            extensionNotif.Margin = new Thickness(149, 0, 0, 35);
                            extensionNotif.Content = "File extension";
                            extensionNotif.Tag = customBinaryID;

                            TextBox nameInput = new TextBox();
                            nameInput.Width = 65;
                            nameInput.HorizontalAlignment = HorizontalAlignment.Left;
                            nameInput.VerticalAlignment = VerticalAlignment.Center;
                            nameInput.Margin = new Thickness(232, 0, 0, 0);
                            nameInput.Tag = "Name_" + customBinaryID;
                            nameInput.Text = System.IO.Path.GetFileNameWithoutExtension(plugin);

                            Label nameNotif = new Label();
                            nameNotif.Width = 73;
                            nameNotif.HorizontalAlignment = HorizontalAlignment.Left;
                            nameNotif.VerticalAlignment = VerticalAlignment.Center;
                            nameNotif.Margin = new Thickness(232, 0, 0, 35);
                            nameNotif.Content = "Name";
                            nameNotif.Tag = customBinaryID;

                            Button saveCustomInstall = new Button();
                            saveCustomInstall.Width = 50;
                            saveCustomInstall.HorizontalAlignment = HorizontalAlignment.Left;
                            saveCustomInstall.VerticalAlignment = VerticalAlignment.Center;
                            saveCustomInstall.Margin = new Thickness(300, 0, 0, 30);
                            saveCustomInstall.Content = "Save";
                            saveCustomInstall.Click += save_Click;
                            saveCustomInstall.Tag = customBinaryID;

                            Button deleteCustomInstall = new Button();
                            deleteCustomInstall.Width = 50;
                            deleteCustomInstall.HorizontalAlignment = HorizontalAlignment.Left;
                            deleteCustomInstall.VerticalAlignment = VerticalAlignment.Center;
                            deleteCustomInstall.Margin = new Thickness(300, 30, 0, 0);
                            deleteCustomInstall.Content = "Delete";
                            deleteCustomInstall.Click += delete_Click;
                            deleteCustomInstall.Tag = plugin;

                            binaryContainer.Children.Add(cbHolder);
                            binaryContainer.Children.Add(exes);
                            binaryContainer.Children.Add(fileExtensionInput);
                            binaryContainer.Children.Add(extensionNotif);
                            binaryContainer.Children.Add(nameInput);
                            binaryContainer.Children.Add(nameNotif);
                            binaryContainer.Children.Add(saveCustomInstall);
                            binaryContainer.Children.Add(deleteCustomInstall);

                            cBLayout.Children.Add(binaryContainer);
                            incramentGlobalY();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Could not find new custom binary, place custom binary exe along with its other asociated files within the 'custom binaries' folder'");
                        }
                    }
                }
                incramentGlobalY();
            }
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button clickedButton = sender as Button;
                if (clickedButton == null)
                {
                    return;
                }
                Grid container = clickedButton.Parent as Grid;
                if (container == null)
                {
                    return;
                }
                string id = clickedButton.Tag.ToString();

                string selectedExe = "";
                string fileExtension = "";
                string binaryName = "";

                foreach (UIElement child in container.Children)
                {
                    if (child is ComboBox comboBox && comboBox.Tag?.ToString() == "Exe_" + id)
                    {
                        selectedExe = comboBox.SelectedItem?.ToString() ?? "";
                    }
                    else if (child is TextBox textBox)
                    {
                        if (textBox.Tag?.ToString() == "Ext_" + id)
                        {
                            fileExtension = textBox.Text.Trim().Replace(".", "");
                        }
                        else if (textBox.Tag?.ToString() == "Name_" + id)
                        {
                            binaryName = textBox.Text.Trim();
                        }
                    }
                }

                string fullFilename = $"{binaryName}.txt";
                string folderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "Custom Plugin");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string filePath = System.IO.Path.Combine(folderPath, fullFilename);
                string contentToWrite = $"{selectedExe}\n{fileExtension}";
                File.WriteAllText(filePath, contentToWrite);

                MessageBox.Show($"File successfully saved to {fullFilename}!");
                loadPrexistingCustomBinaries();
            }
            catch
            {
                MessageBox.Show("Im going to kill you with my army of evil rats");
            }
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button clickedButton = sender as Button;
                if (clickedButton == null)
                {
                    return;
                }
                Grid container = clickedButton.Parent as Grid;
                if (container == null)
                {
                    return;
                }

                string[] lines = File.ReadAllLines(clickedButton.Tag.ToString());

                string initialFilePath = clickedButton.Tag.ToString();
                if (!File.Exists(initialFilePath))
                {
                    return;
                }

                string uninstallDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uninstall");
                if (!Directory.Exists(uninstallDirectory))
                {
                    Directory.CreateDirectory(uninstallDirectory);
                }

                string fileName = System.IO.Path.GetFileName(initialFilePath);
                string destinationFilePath = System.IO.Path.Combine(uninstallDirectory, fileName);

                File.Move(initialFilePath, destinationFilePath);
                loadPrexistingCustomBinaries();

                string searchLocation = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries", "Custom Binaries");
                string[] binary = Directory.GetFiles(searchLocation, lines[0], SearchOption.AllDirectories);
                string binaryTarget = binary[0];

                DirectoryInfo currentDir = new DirectoryInfo(System.IO.Path.GetDirectoryName(binaryTarget));
                DirectoryInfo targetToDelete = null;

                while (currentDir != null && currentDir.Parent != null)
                {

                    if (string.Equals(currentDir.Parent.FullName.TrimEnd('\\'), searchLocation.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
                    {
                        targetToDelete = currentDir;
                        break;
                    }
                    currentDir = currentDir.Parent;
                }

                if (targetToDelete != null && targetToDelete.Exists)
                {
                    targetToDelete.Delete(true);
                }
                MessageBox.Show("Uninstallation Succsessful");
            }
            catch
            {
                MessageBox.Show("Im going to kill you with my army of neutral rats");
            }
        }
        private void Window2_Closing(object sender, CancelEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}