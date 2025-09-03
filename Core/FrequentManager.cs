using MHbinder.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class FrequentManager
{
    public static List<FrequentItem> Items { get; private set; } = new();

    public class FrequentItem
    {
        public string Phrase { get; set; } = "";

        public FrequentItem() { }
        public FrequentItem(string phrase)
        {
            Phrase = phrase;
        }
    }

    public static void LoadFromConfig(Dictionary<string, string> cfg)
    {
        Items.Clear();

        foreach (var kvp in cfg)
        {
            if (!kvp.Key.StartsWith("freq")) continue;

            string value = kvp.Value.Trim();
            if (value.StartsWith("phrase:", StringComparison.OrdinalIgnoreCase))
            {
                string phrase = value.Substring("phrase:".Length).Trim();
                if (!string.IsNullOrEmpty(phrase))
                    Items.Add(new FrequentItem(phrase));
            }
        }
    }

    public static Dictionary<string, string> SaveToConfig()
    {
        var cfg = new Dictionary<string, string>();
        int i = 0;
        foreach (var item in Items)
        {
            cfg[$"freq{i}"] = $"phrase:{item.Phrase}";
            i++;
        }
        return cfg;
    }

    public static void Save()
    {
        var cfg = ConfigManager.LoadDictionary();

        // удаляем все старые freq ключи
        var keysToRemove = cfg.Keys.Where(k => k.StartsWith("freq")).ToList();
        foreach (var key in keysToRemove)
            cfg.Remove(key);

        // добавляем актуальные
        foreach (var kvp in SaveToConfig())
            cfg[kvp.Key] = kvp.Value;

        ConfigManager.SaveDictionary(cfg);
    }


    public static void AddItem(string phrase)
    {
        Items.Add(new FrequentItem(phrase));
    }

    public static void RemoveItem(FrequentItem item)
    {
        Items.Remove(item);
    }
}
