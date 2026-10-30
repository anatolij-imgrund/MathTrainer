using System;
using System.IO;

namespace MathTrainer.Persistence
{
    public class PasswordService
    {
        private readonly string _filePath;
        private const string DefaultPassword = "0000";

        public PasswordService(string fileName = "parent_config.txt")
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appSpecificFolder = Path.Combine(appDataPath, "MathTrainer");

            if (!Directory.Exists(appSpecificFolder))
            {
                Directory.CreateDirectory(appSpecificFolder);
            }

            _filePath = Path.Combine(appSpecificFolder, fileName);
        }

        public string GetPassword()
        {
            if (!File.Exists(_filePath) || string.IsNullOrWhiteSpace(File.ReadAllText(_filePath)))
            {
                SetPassword(DefaultPassword);
                return DefaultPassword;
            }

            try
            {
                return File.ReadAllText(_filePath).Trim();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Fehler beim Passwortlesen: {ex.Message}");
                SetPassword(DefaultPassword);
                return DefaultPassword;
            }
        }

        public bool CheckPassword(string inputPassword)
        {
            string storedPassword = GetPassword();
            return storedPassword == inputPassword;
        }

        public void SetPassword(string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                System.Console.WriteLine("Versuch leeres Password zu stellen. Es wird Default-Passwort gestellt.");
                newPassword = DefaultPassword;
            }

            try
            {
                File.WriteAllText(_filePath, newPassword.Trim());
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Fehler neim Passwortspeichern: {ex.Message}");
            }
        }
    }
}