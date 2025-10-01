using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MHbinder
{
    public static class ForumStacker
    {
        private static readonly Dictionary<string, int> PunishLimits = new(StringComparer.OrdinalIgnoreCase)
        {
            ["/ajail"] = 720,
            ["/mute"] = 720,
            ["/ban"] = 9999,
            ["/hardban"] = 9999,
            ["/gunban"] = 9999,
        };

        public static (string output, List<string> errors) Process(string input)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(input))
            {
                errors.Add("Пожалуйста, введите данные");
                return ("", errors);
            }

            var lines = input.Replace("\r", "").Split('\n');
            var map = new Dictionary<string, (string id, string punish, int? time, HashSet<string> names)>();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || Regex.IsMatch(line, @"^-+$")) continue;

                var idM = Regex.Match(line, @"ID:([^;]*);", RegexOptions.IgnoreCase);
                var punM = Regex.Match(line, @"PUNISH:([^;]*);", RegexOptions.IgnoreCase);
                var timM = Regex.Match(line, @"TIME:([^;]*);", RegexOptions.IgnoreCase);
                var namM = Regex.Match(line, @"NAME:([^;]*);", RegexOptions.IgnoreCase);

                if (!idM.Success) errors.Add($"Ошибка в строке {i + 1}: отсутствует поле ID\nСтрока: \"{line}\"");
                if (!punM.Success) errors.Add($"Ошибка в строке {i + 1}: отсутствует поле PUNISH\nСтрока: \"{line}\"");
                if (!namM.Success) errors.Add($"Ошибка в строке {i + 1}: отсутствует поле NAME\nСтрока: \"{line}\"");

                var id = idM.Success ? idM.Groups[1].Value.Trim() : "";
                var punish = punM.Success ? punM.Groups[1].Value.Trim() : "";
                var timeStr = timM.Success ? timM.Groups[1].Value.Trim() : "";
                var name = namM.Success ? namM.Groups[1].Value.Trim() : "";

                if (idM.Success && id.Length == 0) errors.Add($"Ошибка в строке {i + 1}: пустое значение ID\nСтрока: \"{line}\"");
                if (punM.Success && punish.Length == 0) errors.Add($"Ошибка в строке {i + 1}: пустое значение PUNISH\nСтрока: \"{line}\"");
                if (namM.Success && name.Length == 0) errors.Add($"Ошибка в строке {i + 1}: пустое значение NAME\nСтрока: \"{line}\"");

                bool isWarn = punish.Equals("warn", StringComparison.OrdinalIgnoreCase) ||
                              punish.Equals("/warn", StringComparison.OrdinalIgnoreCase);

                int? tVal = null;
                if (!isWarn)
                {
                    if (string.IsNullOrWhiteSpace(timeStr))
                    {
                        errors.Add($"Ошибка в строке {i + 1}: отсутствует или пустое значение TIME для наказания {punish}\nСтрока: \"{line}\"");
                    }
                    else if (int.TryParse(timeStr, out var t))
                    {
                        tVal = t;
                    }
                    else
                    {
                        errors.Add($"Ошибка в строке {i + 1}: TIME должен быть числом, получено \"{timeStr}\"\nСтрока: \"{line}\"");
                    }
                }

                if (!idM.Success || !punM.Success || !namM.Success) continue; 

                var key = $"{id}|{punish}";
                if (!map.TryGetValue(key, out var rec))
                    rec = (id, punish, isWarn ? (int?)null : 0, new HashSet<string>(StringComparer.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(name))
                    rec.names.Add(name);

                if (!isWarn && tVal.HasValue)
                {
                    int limit = PunishLimits.TryGetValue(punish, out var lim) ? lim : int.MaxValue;
                    int current = rec.time ?? 0;
                    int sum = current + tVal.Value;
                    rec.time = Math.Min(sum, limit);
                }

                map[key] = rec;
            }

            if (errors.Count > 0) return ("", errors);

            var order = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["/ajail"] = 0,
                ["/ban"] = 1,
                ["/hardban"] = 2,
                ["/gunban"] = 3,
                ["/mute"] = 4,
                ["/warn"] = 5,
                ["warn"] = 5
            };

            var list = map.Values
                .Select(v => new { v.id, v.punish, v.time, names = v.names.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList() })
                .OrderBy(v => order.TryGetValue(v.punish, out var o) ? o : 99)
                .ThenByDescending(v => v.punish.Equals("/ajail", StringComparison.OrdinalIgnoreCase) ? (v.time ?? 0) : 0)
                .ThenBy(v => v.id, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var sb = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                var r = list[i];
                if (i > 0) sb.AppendLine();
                if (r.punish.Equals("warn", StringComparison.OrdinalIgnoreCase) || r.punish.Equals("/warn", StringComparison.OrdinalIgnoreCase))
                    sb.Append($"{r.punish} {r.id} Жалобы {string.Join(", ", r.names)}");
                else
                    sb.Append($"{r.punish} {r.id} {r.time ?? 0} Жалобы {string.Join(", ", r.names)}");
            }

            return (sb.ToString(), errors);
        }
    }
}
