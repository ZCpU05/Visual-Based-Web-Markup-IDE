using External_Langauage_Manager;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace VWIDE
{
    public class Updater
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly HttpClient client = new HttpClient();
        public async Task pluginLinkUpdater() //Detects if the plugins are not the most recent version and notifies if an update is needed. 
        {
            try
            {
                string gitUrl = "https://api.github.com/repos/ZCpU05/VwIDE-External-Binary-Scripts/releases/latest";

                client.DefaultRequestHeaders.UserAgent.Clear();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("VwIDE-Updater");

                string apiResponse = await client.GetStringAsync(gitUrl);

                var latestRelease = JsonSerializer.Deserialize<gitHubRelease>(apiResponse);

                if (latestRelease == null || string.IsNullOrEmpty(latestRelease.TagName))
                {
                    return;
                }

                string remoteVersionStr = latestRelease.TagName.TrimStart('v', 'V');
                Version remoteVersion = new Version(remoteVersionStr);

                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string jsonPath = Path.Combine(appDataPath, "VwIDE", "binaryInstallPaths.json");

                if (!File.Exists(jsonPath)) return;

                string jsonString = await File.ReadAllTextAsync(jsonPath);
                var plugins = JsonSerializer.Deserialize<List<jsonStructure>>(jsonString);

                bool needsUpdate = false;

                if (plugins != null)
                {
                    foreach (var plugin in plugins)
                    {
                        Version localVersion = new Version(plugin.version);

                        if (remoteVersion > localVersion)
                        {
                            plugin.scriptLink = $"https://github.com/ZCpU05/VwIDE-External-Binary-Scripts/releases/download/{latestRelease.TagName}/{GetPluginDllName(plugin.name)}";
                            plugin.version = remoteVersionStr;

                            needsUpdate = true;
                        }
                    }
                }

                if (needsUpdate)
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string updatedJson = JsonSerializer.Serialize(plugins, options);
                    await File.WriteAllTextAsync(jsonPath, updatedJson);
                    MessageBox.Show("New Version of Plugin Detected, If any plugins are installed, uninstall and reinstall them to update them");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error has occurred: " + ex.Message);
            }
        }

        public void missingFileHandler() //Function to heal any missing files in the event that is needed
        {
            string VwIDEDataPath = Path.Combine(appDataPath, "VWIDE");
            if (!Directory.Exists(VwIDEDataPath))
            {
                Directory.CreateDirectory(VwIDEDataPath);
            }
            if (!File.Exists(Path.Combine(VwIDEDataPath, "config.txt")))
            {
                string[] lines = { "False", "False", "False", "False" };

                using (StreamWriter streamWriter = new StreamWriter(Path.Combine(VwIDEDataPath, "config.txt")))
                {
                    foreach (string line in lines)
                    {
                        streamWriter.WriteLine(line);
                    }
                }
            }
            if (!File.Exists(Path.Combine(VwIDEDataPath, "defaultGitRepo.txt")))
            {
                File.Create(Path.Combine(VwIDEDataPath, "defaultGitRepo.txt"));
            }
            if (!File.Exists(Path.Combine(VwIDEDataPath, "defaultProjDir.txt")))
            {
                File.Create(Path.Combine(VwIDEDataPath, "defaultProjDir.txt"));
            }
            if (!File.Exists(Path.Combine(VwIDEDataPath, "fontSize.txt")))
            {

                using (StreamWriter streamWriter = new StreamWriter(Path.Combine(VwIDEDataPath, "fontSize.txt")))
                {
                    streamWriter.WriteLine("12");
                }
            }
            if (!File.Exists(Path.Combine(VwIDEDataPath, "binaryInstallPaths.json")))
            {
                var pluginStrcture = new List<jsonStructure>
                {
                    new jsonStructure
                    {
                        name = "python",
                        binaryLink = "https://www.python.org/ftp/python/3.13.14/python-3.13.14-embed-amd64.zip",
                        scriptLink = "",
                        version = "0.0.1"
                    },
                    new jsonStructure
                    {
                        name = "nodeJS",
                        binaryLink = "https://nodejs.org/dist/v24.18.0/node-v24.18.0-win-x64.zip",
                        scriptLink = "",
                        version = "0.0.1"
                    }
                };
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(pluginStrcture, options);
                File.WriteAllText(Path.Combine(VwIDEDataPath, "binaryInstallPaths.json"), jsonString);
            }
            if(!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins")))
            {
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins"));
            }
            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "Custom Plugins")))
            {
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "Custom Plugins"));
            }
            if(!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries")))
            {
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries"));
            }
            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries", "Compatible Binaries")))
            {
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries", "Compatible Binaries"));
            }
            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries", "Custom Binaries")))
            {
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries", "Custom Binaries"));
            }
            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries", "php")))
            {
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries", "php"));
            }
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries", "php", "php.exe")))
            {
                phpRestore();
            }
        }
        static string GetPluginDllName(string pluginName)
        {
            return pluginName.ToLower() switch
            {
                "python" => "Python.Plugin.dll",
                "nodejs" => "Node.Plugin.dll",
                _ => $"{pluginName}.Plugin.dll"
            };
        }
        async Task phpRestore()
        {
            HttpClient httpClient = new HttpClient();
            string downloadPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                $"php.zip"
            );

            byte[] fileBytes = await httpClient.GetByteArrayAsync("https://downloads.php.net/~windows/releases/archives/php-8.5.9-nts-Win32-vs17-x64.zip");
            await File.WriteAllBytesAsync(downloadPath, fileBytes);

            string extractEnd = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries", "php");

            if (!Directory.Exists(extractEnd))
            {
                Directory.CreateDirectory(extractEnd);
            }

            ZipFile.ExtractToDirectory(downloadPath, extractEnd, overwriteFiles: true);

            if (File.Exists(downloadPath))
            {
                File.Delete(downloadPath);
            }
        }
    }
    internal class jsonStructure
    {
        public string name { get; set; }
        public string binaryLink { get; set; }
        public string scriptLink { get; set; }
        public string version { get; set; }
    }
    internal class gitHubRelease
    {
        [System.Text.Json.Serialization.JsonPropertyName("tag_name")]
        public string TagName { get; set; }
    }
}
