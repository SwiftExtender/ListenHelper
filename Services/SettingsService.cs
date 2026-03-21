using System.IO;
using System.Text.Json;

namespace voicio.Services
{
    public class AppSettings()
    {
        public bool IsVoiceTasksEnabled = false;
        public bool IsVoiceSearchFuzzy = false;
        public string RecognitionType = "Vosk";
        public bool IsLoggingEnabled = false;
        public string SearchWord = "find";
        public string ActionWord = "execute";
        public string SetSearchTypeWord = "type";
    }
    public class SettingsService
    {
        private string _settingsPath;
        public AppSettings Load()
        {
            if (!File.Exists(_settingsPath))
            {
                return new AppSettings();
            }
            string settings = File.ReadAllText(_settingsPath);
            return JsonSerializer.Deserialize<AppSettings>(settings) ?? new AppSettings();
        }
        public SettingsService(string path)
        {
            _settingsPath = Path.Join(path, "settings.json");
        }
    }
}
