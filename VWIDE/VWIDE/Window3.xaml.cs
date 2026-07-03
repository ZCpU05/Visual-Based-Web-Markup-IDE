using System;
using System.ComponentModel;
using System.Windows;
using Meziantou.Framework.Win32;


namespace VWIDE //Window for linking github
{
    /// <summary>
    /// Interaction logic for Window3.xaml
    /// </summary>
    public partial class Window3 : Window
    {
        readonly credentialManager credentialManager = new credentialManager();
        private bool _isSuccessfullySaved = false;

        public Window3()
        {
            InitializeComponent();
            this.Closing += Window3_Closing;
        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            string username = usernameField.Text.Trim();
            string password = passwordField.Text.Trim();

            if(string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter github account details");
            }
            else
            {
                try
                {
                    credentialManager.saveCredentials(username, password);

                    _isSuccessfullySaved = true;
                    this.DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unforeseen error: {ex.Message}\n\nStack Trace: {ex.StackTrace}");
                }
            }

        }
        private void Window3_Closing(object sender, CancelEventArgs e)
        {
            if (!_isSuccessfullySaved)
            {
                MessageBox.Show("To exit please submit or press cancel");
                e.Cancel = true;
            }
        }
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            _isSuccessfullySaved = true;
            this.Close();
        }
    }
}
