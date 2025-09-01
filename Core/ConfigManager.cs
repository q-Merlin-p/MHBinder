using MajesticHub.Core.binds;
using MHbinder.Core.binds;
using System;
using System.Collections.Generic;
using System.IO;

namespace MHbinder.Core
{
    internal static class ConfigManager
    {
        private static readonly string ConfigDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configs");
        private static readonly string ConfigPath = Path.Combine(ConfigDir, "default.ini");

        public static string OpenMenuKey { get; private set; } = "RightShift";
        public static List<BinderItem> Binds => BinderManager.Binds;
        public static List<FrequentManager.FrequentItem> FrequentItems => FrequentManager.Items;

        public static void Load()
        {
            var cfg = LoadDictionary();

            BinderManager.LoadFromConfig(cfg);
            FrequentManager.LoadFromConfig(cfg);

            if (cfg.TryGetValue("openMenu", out var key))
                OpenMenuKey = key;
        }

        public static Dictionary<string, string> LoadDictionary()
        {
            var cfg = new Dictionary<string, string>();

            if (!Directory.Exists(ConfigDir))
                Directory.CreateDirectory(ConfigDir);

            if (File.Exists(ConfigPath))
            {
                foreach (var line in File.ReadAllLines(ConfigPath))
                {
                    var parts = line.Split('=');
                    if (parts.Length != 2) continue;
                    cfg[parts[0].Trim()] = parts[1].Trim();
                }
            }

            return cfg;
        }

        public static void Save()
        {
            if (!Directory.Exists(ConfigDir))
                Directory.CreateDirectory(ConfigDir);

            using var writer = new StreamWriter(ConfigPath, false);

            writer.WriteLine($"openMenu = {OpenMenuKey}");

            var bindCfg = BinderManager.SaveToConfig();
            foreach (var kv in bindCfg)
                writer.WriteLine($"{kv.Key} = {kv.Value}");

            var freqCfg = FrequentManager.SaveToConfig();
            foreach (var kv in freqCfg)
                writer.WriteLine($"{kv.Key} = {kv.Value}");
        }

        public static void SaveDictionary(Dictionary<string, string> cfg)
        {
            if (!Directory.Exists(ConfigDir))
                Directory.CreateDirectory(ConfigDir);

            using var writer = new StreamWriter(ConfigPath, false);
            foreach (var kv in cfg)
                writer.WriteLine($"{kv.Key} = {kv.Value}");
        }
    }
}
