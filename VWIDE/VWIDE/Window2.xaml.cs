using System;
using System.Collections.Generic;
using System.IO;
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
using System.Text.Json;
using System.Net.Http;
using System.IO.Compression;

namespace VWIDE //Window for installing custom binaries
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        int globalY = 50;
        int customBinaryID = 0;
        string customBinaryPaths = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries", "Custom Binaries");
        List<customBinary> customBinaries = new List<customBinary>();
        public Window2()
        {
            InitializeComponent();
            loadPrexistingCustomBinaries();
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
                Grid binaryContainer = new Grid();
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
                MessageBox.Show("Could not find new custom binary, place custom binary exe along with its other asociated files within the 'custom binaries' folder'");
            }
        }
        void loadPrexistingCustomBinaries()
        {
            string folderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "Custom Plugin");
            if (Directory.Exists(folderPath))
            {
                //Code for editing and manging installed plugins please finish!
                //string[] customPlugins

                //incramentGlobalY();
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
    }
}