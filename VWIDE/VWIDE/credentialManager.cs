using Meziantou.Framework.Win32;
using System;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
namespace VWIDE
{
    public class credentialManager
    {
        const string appName = "VWIDE";

        public void saveCredentials(string username, string password)
        {
            CredentialManager.WriteCredential(
                applicationName: appName,
                userName: username,
                secret: password,
                persistence: CredentialPersistence.Session);
        }
        public (string Username, string Token)? getCredentials()
        {
            var credentials = CredentialManager.ReadCredential(appName);

            if (credentials == null)
            {
                return null;
            }
            return (credentials.UserName, credentials.Password);
        }
        public void clearedCredentials()
        {
            CredentialManager.DeleteCredential(appName);
        }
    }
}
