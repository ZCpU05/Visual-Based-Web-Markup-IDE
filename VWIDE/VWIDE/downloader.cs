using System;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.IO.Compression;

namespace VWIDE
{
    public class downloader //object that handles downloading external plugins, held in a seperate object again for easier referencing, is only loaded when on the corosponding window. 
    {
        private readonly string[] languages = new string[] { "Python", "nodeJS" };
        public string chosenLanguage { get; private set; }
        public int LangID { get; private set; }

        private static readonly HttpClient httpClient = new HttpClient();

        public async Task download(int id)
        {
            LangID = id;
            chosenLanguage = languages[LangID];

            string jsonContent = await File.ReadAllTextAsync(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VwIDE", "binaryInstallPaths.json"));

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            List<language> allLang = JsonSerializer.Deserialize<List<language>>(jsonContent, options);
            language targetLang = allLang?.FirstOrDefault(x => string.Equals(x.Name, chosenLanguage, StringComparison.OrdinalIgnoreCase));

            string downloadPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                $"{chosenLanguage}.zip"
            );

            byte[] fileBytes = await httpClient.GetByteArrayAsync(targetLang.BinaryLink);
            await File.WriteAllBytesAsync(downloadPath, fileBytes);

            string extractEnd = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries", "Compatible Binaries", chosenLanguage);

            if (!Directory.Exists(extractEnd))
            {
                Directory.CreateDirectory(extractEnd);
            }

            ZipFile.ExtractToDirectory(downloadPath, extractEnd, overwriteFiles: true);

            if (File.Exists(downloadPath))
            {
                File.Delete(downloadPath);
            }

            if (!string.IsNullOrEmpty(targetLang.ScriptLink))
            {
                string dllDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
                string fileName = Path.GetFileName(new Uri(targetLang.ScriptLink).LocalPath);
                string filePath = Path.Combine(dllDir, fileName);

                fileBytes = await httpClient.GetByteArrayAsync(targetLang.ScriptLink);
                await File.WriteAllBytesAsync(filePath, fileBytes);
            }

            MessageBox.Show($"Downloaded {chosenLanguage} Binary, Restart application for installation to take effect", "Success, ");
        }
        public bool isInstalled(string searchTerm)
        {
            string binariesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries", "Compatible Binaries");

            try
            {
                string foundFile = Directory.EnumerateFiles(binariesFolder, searchTerm, SearchOption.AllDirectories)
                    .FirstOrDefault();
                if (foundFile != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch 
            {
                MessageBox.Show("Fatal exception");
                return false;
            }
        }
        public void uninstall(string targetedUninstall)
        {
            if (targetedUninstall == "nodeJS") //Had to add this if statement and the one at line 106 due to a bug i couldn't fix otherwise due to a coding error
            {
                targetedUninstall = "Node";
            }
            string binaryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries", "Compatible Binaries", targetedUninstall);
            string pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", targetedUninstall + ".Plugin.dll");

            string uninstallPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uninstall", targetedUninstall + ".Plugin.dll");
            File.Move(pluginPath, uninstallPath);

            if (targetedUninstall == "Node")
            {
                targetedUninstall = "nodeJS";
                binaryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries", "Compatible Binaries", targetedUninstall);
            }
            Directory.Delete(binaryPath, true);

            MessageBox.Show("Uninstall Succsessful, restart application to take effect");
        }
    }

    public class language //Constructs the object based of the JSON file. 
    {
        public string Name { get; set; }
        public string BinaryLink { get; set; }
        public string ScriptLink { get; set; }
    }
}
