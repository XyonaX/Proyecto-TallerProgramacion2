using System;
using System.IO;
using System.Text.Json;

namespace Proyecto_Taller_2
{
    public static class SettingsService
    {
        private static readonly string Root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Proyecto_Taller_2");
        private static readonly string FilePath = Path.Combine(Root, "settings.json");

        public static AppSettings Current { get; private set; } = new AppSettings();

        public static void Load()
        {
            try
            {
                if (!Directory.Exists(Root)) Directory.CreateDirectory(Root);
                if (!File.Exists(FilePath))
                {
                    Save(new AppSettings());
                }
                var json = File.ReadAllText(FilePath);
                Current = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                Current = new AppSettings();
            }
        }

        public static void Save(AppSettings settings)
        {
            if (!Directory.Exists(Root)) Directory.CreateDirectory(Root);
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
            Current = settings;
        }

        public static string ExportTo(string exportPath)
        {
            File.Copy(FilePath, exportPath, overwrite: true);
            return exportPath;
        }

        public static void ImportFrom(string importPath)
        {
            File.Copy(importPath, FilePath, overwrite: true);
            Load();
        }

        public static void Reset()
        {
            Save(new AppSettings());
        }
    }
}
