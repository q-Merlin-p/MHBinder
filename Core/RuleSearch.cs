using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Collections.Generic;

public class Rule
{
    public string number { get; set; }   
    public string text { get; set; }
    public string punishment { get; set; }
    public List<string> keywords { get; set; }
}


public static class RuleSearch
{
    public static string FindRule(string query)
    {
        string[] ruleFiles = {
            "assets.rules.OPP_rules.json",
            "assets.rules.SomeOther_rules.json",
            "assets.rules.Extra_rules.json"
        };

        if (string.IsNullOrWhiteSpace(query))
            return "Введите запрос для поиска.";

        query = query.Trim().ToLower();

        var assembly = Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames();

        foreach (var r in resourceNames)
            Console.WriteLine(r); // ПРИ РЕЛИЗЕ УБРАТЬ ЛОГ

        foreach (var resourceName in resourceNames)
        {
            
            try
            {
                using Stream? stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null) continue;

                using var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();

                var rules = JsonSerializer.Deserialize<List<Rule>>(json);

                var found = rules?.FirstOrDefault(r =>
                    r.keywords.Any(k => k.Equals(query, StringComparison.OrdinalIgnoreCase)));

                if (found != null)
                    return $"{found.number} — {found.text}\n\n💡 Наказание: {found.punishment}";
            }
            catch (Exception ex)
            {
                return $"Ошибка при загрузке правил ({resourceName}): {ex.Message}";
            }
        }

        return "Такого правила нет в базе.";
    }


}