using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VWIDE
{
    class customBinary
    {
        public string binaryPath;
        public string fileExtension;
        public string langName;
        
        public customBinary(string bP, string fE, string lN)
        {
            this.binaryPath = bP;
            this.fileExtension = fE;
            this.langName = lN;
        }
        public async Task<string> execute(string code)
        {
            if (!File.Exists(binaryPath))
            {
                MessageBox.Show("Binary not found.");
                return null;
            }

            string tempFilePath = Path.Combine(Path.GetTempPath(), "vwide_preview" + fileExtension);
            await File.WriteAllTextAsync(tempFilePath, code, Encoding.UTF8);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = binaryPath,
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

                    return string.IsNullOrEmpty(error) ? output : $"{langName} Error: {error}";
                }
            }
            catch (Exception ex)
            {
                return $"Process Error: {ex.Message}";
            }
        }
    }
}
