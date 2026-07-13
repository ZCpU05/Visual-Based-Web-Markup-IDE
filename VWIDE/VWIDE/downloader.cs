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
    public class downloader
    {
        private readonly string[] languages = new string[] { "python", "nodeJS" };
        public string chosenLanguage { get; private set; }
        public int LangID { get; private set; }

        private static readonly HttpClient httpClient = new HttpClient();

        public async Task download(int id)
        {
            LangID = id;
            chosenLanguage = languages[LangID];

            string jsonContent = await File.ReadAllTextAsync("binaryInstallPaths.json");

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
            string binaryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries", "Compatible Binaries", targetedUninstall);
            string pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", targetedUninstall + " Plugin.dll");

            Directory.Delete(binaryPath, true);
            //File.Delete(pluginPath);
            //Need to add special case for uninstalling plugins due to nature of dlls

            MessageBox.Show("Uninstall Succsessful");
        }
    }

    public class language
    {
        public string Name { get; set; }
        public string BinaryLink { get; set; }
        public string ScriptLink { get; set; }
    }
}
