using System.Text.RegularExpressions;

namespace MHbinder.Core
{
    public class LogResult
    {
        public string Id { get; set; } = "";
        public string Punish { get; set; } = "";
        public int? Time { get; set; }
        public List<string> Names { get; set; } = new();
    }

    public static class LogStacker
    {
        private static readonly Dictionary<string, int?> LIMITS = new()
        {
            ["/ajail"] = 720,
            ["/mute"] = 720,
            ["/ban"] = 9999,
            ["/hardban"] = 9999,
            ["/gunban"] = null
        };

        private static readonly string[] ORDER = { "ajail", "ban", "hardban", "gunban", "mute", "warn" };

        public static (List<LogResult> Results, List<string> Errors) Process(string input)
        {
            var errors = new List<string>();
            var results = new List<LogResult>();
            if (string.IsNullOrWhiteSpace(input))
            {
                errors.Add("Пустой ввод");
                return (results, errors);
            }

            var lines = input.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || Regex.IsMatch(line, "^-+$")) continue;

                var idMatch = Regex.Match(line, @"ID:([^;]*);", RegexOptions.IgnoreCase);
                var punishMatch = Regex.Match(line, @"PUNISH:([^;]*);", RegexOptions.IgnoreCase);
                var timeMatch = Regex.Match(line, @"TIME:([^;]*);", RegexOptions.IgnoreCase);
                var nameMatch = Regex.Match(line, @"NAME:([^;]*);", RegexOptions.IgnoreCase);

                if (!idMatch.Success) errors.Add($"Ошибка в строке {i + 1}: отсутствует поле ID\nСтрока: \"{line}\"");
                if (!punishMatch.Success) errors.Add($"Ошибка в строке {i + 1}: отсутствует поле PUNISH\nСтрока: \"{line}\"");
                if (!nameMatch.Success) errors.Add($"Ошибка в строке {i + 1}: отсутствует поле NAME\nСтрока: \"{line}\"");

                string id = idMatch.Groups[1].Value.Trim();
                string punish = punishMatch.Groups[1].Value.Trim();
                string name = nameMatch.Groups[1].Value.Trim();
                int? time = null;

                if (string.IsNullOrEmpty(id)) errors.Add($"Ошибка в строке {i + 1}: пустое значение ID\nСтрока: \"{line}\"");
                if (string.IsNullOrEmpty(punish)) errors.Add($"Ошибка в строке {i + 1}: пустое значение PUNISH\nСтрока: \"{line}\"");
                if (string.IsNullOrEmpty(name)) errors.Add($"Ошибка в строке {i + 1}: пустое значение NAME\nСтрока: \"{line}\"");

                punish = punish.StartsWith("/") ? punish.ToLower() : "/" + punish.ToLower();
                if (punish != "/warn" && punish != "/gunban")
                {
                    var tStr = timeMatch.Groups[1].Value.Trim();
                    if (string.IsNullOrEmpty(tStr))
                    {
                        errors.Add($"Ошибка в строке {i + 1}: отсутствует TIME для наказания {punish}\nСтрока: \"{line}\"");
                    }
                    else if (int.TryParse(tStr, out int tNum))
                    {
                        time = Math.Max(1, tNum);
                    }
                    else
                    {
                        errors.Add($"Ошибка в строке {i + 1}: некорректное значение TIME\nСтрока: \"{line}\"");
                    }
                }

                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(punish) && !string.IsNullOrEmpty(name) && errors.Count == 0)
                {
                    var existing = results.FirstOrDefault(r => r.Id == id && r.Punish == punish);
                    if (existing != null)
                    {
                        if (time.HasValue)
                        {
                            var limit = LIMITS[punish];
                            existing.Time = Math.Min((existing.Time ?? 0) + time.Value, limit ?? (existing.Time ?? 0) + time.Value);
                        }
                        existing.Names.Add(name);
                    }
                    else
                    {
                        var limit = LIMITS[punish];
                        var finalTime = (limit.HasValue && time.HasValue) ? Math.Min(time.Value, limit.Value) : time;
                        results.Add(new LogResult { Id = id, Punish = punish, Time = finalTime, Names = new List<string> { name } });
                    }
                }
            }

            if (errors.Count > 0) return (new List<LogResult>(), errors);

            results.ForEach(r => r.Names = r.Names.Distinct().ToList());
            results.Sort((a, b) =>
            {
                string aK = a.Punish.TrimStart('/');
                string bK = b.Punish.TrimStart('/');
                if (aK == "ajail" && bK == "ajail")
                    return (b.Time ?? 0) - (a.Time ?? 0);
                return Array.IndexOf(ORDER, aK) - Array.IndexOf(ORDER, bK);
            });

            return (results, errors);
        }
    }
}
