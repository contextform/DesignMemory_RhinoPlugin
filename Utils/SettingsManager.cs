using System;
using System.IO;
using Newtonsoft.Json;

namespace Contextform.Utils
{
    public class SettingsManager
    {
        private static SettingsManager _instance;
        private readonly string _settingsPath;
        private ContextformSettings _settings;

        public static SettingsManager Instance => _instance ??= new SettingsManager();

        public string ClaudeApiKey 
        { 
            get => _settings?.ClaudeApiKey ?? string.Empty;
            set 
            { 
                if (_settings == null) _settings = new ContextformSettings();
                _settings.ClaudeApiKey = value;
                SaveSettings();
            }
        }

        public string FreeApiEndpoint 
        { 
            get => _settings?.FreeApiEndpoint ?? "https://contextform-api.onrender.com/api/generate";
        }

        public bool UseFreeEndpoint 
        { 
            get => _settings?.UseFreeEndpoint ?? true;
        }

        private SettingsManager()
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Contextform");
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            
            _settingsPath = Path.Combine(appDataPath, "settings.json");
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    _settings = JsonConvert.DeserializeObject<ContextformSettings>(json) ?? new ContextformSettings();
                }
                else
                {
                    _settings = new ContextformSettings();
                }
            }
            catch (Exception)
            {
                _settings = new ContextformSettings();
            }
        }

        private void SaveSettings()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                Rhino.RhinoApp.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
    }

    public class ContextformSettings
    {
        [JsonProperty("claude_api_key")]
        public string ClaudeApiKey { get; set; } = string.Empty;

        [JsonProperty("free_api_endpoint")]
        public string FreeApiEndpoint { get; set; } = "https://contextform-api.onrender.com/api/generate"; // Your free endpoint

        [JsonProperty("use_free_endpoint")]
        public bool UseFreeEndpoint { get; set; } = true; // Default to free for MVP
    }
}