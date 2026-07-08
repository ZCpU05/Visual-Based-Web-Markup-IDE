using System;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace VWIDE
{
    public class downloader
    {
        string[] languages = new string[] { "python", "nodeJS" };
        public string chosenLanguage;
        public int langID;

        private static readonly HttpClient httpClient = new HttpClient();

        public async Task download(int id)
        {
            langID = id;
            chosenLanguage = languages[langID];

            string jsonContent = await File.ReadAllTextAsync("binaryInstallPaths.json");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            List<language> allLang = JsonSerializer.Deserialize<List<language>>(jsonContent, options);

            language targetLang = allLang?.FirstOrDefault(x => string.Equals(x.name, chosenLanguage, StringComparison.OrdinalIgnoreCase));

            string downloadPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                $"{chosenLanguage}.zip"
            );

            byte[] fileBytes = await httpClient.GetByteArrayAsync(targetLang.binaryLink);
            await File.WriteAllBytesAsync(downloadPath, fileBytes);

            MessageBox.Show($"Downloaded {chosenLanguage}.zip", "Success");
        }
    }
        public class language
        {
            public string name { get; set; }
            public string binaryLink { get; set; }
            public string scriptLink { get; set; }
        }
}
