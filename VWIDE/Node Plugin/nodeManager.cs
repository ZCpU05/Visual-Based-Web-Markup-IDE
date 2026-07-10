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

namespace Node_Plugin
{
    internal class nodeManager : IPlugin
    {
        public void OnStartup()
        {
            string pythonFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries", "Compatible Binaries", "nodeJS", "node-v24.18.0-win-x64");
            string binaryPath = Path.Combine(pythonFolder, "node.exe");

            if (File.Exists(binaryPath))
            {
                MessageBox.Show("Node.JS is installed");
            }
            else
            {
                MessageBox.Show("you smell and I am homophobic");
            }
        }
    }
}
