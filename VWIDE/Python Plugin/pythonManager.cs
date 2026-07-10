using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using External_Langauage_Manager;

namespace Python_Plugin
{
    internal class pythonManager : IPlugin
    {
        public void OnStartup()
        {
            string pythonFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries", "Compatible Binaries", "python");
            string binaryPath = Path.Combine(pythonFolder, "python.exe");

            if (File.Exists(binaryPath))
            {
                MessageBox.Show("Python is installed");
            }
            else
            {
                MessageBox.Show("you smell and I am homophobic");
            }
        }
    }
}
