using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MHbinder.Core
{
    internal class GrammarClient
    {
        private static readonly HttpClient _http = new HttpClient();

        // === Yandex Speller ===
        public class YandexResult
        {
            public string word { get; set; }
            public List<string> s { get; set; }
            public int pos { get; set; }
            public int len { get; set; }
        }


        public static async Task<List<YandexResult>> CallYandexSpeller(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return new List<YandexResult>();

            string url = $"https://speller.yandex.net/services/spellservice.json/checkText?text={Uri.EscapeDataString(text)}&lang=ru";
            var res = await _http.GetAsync(url);
            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<YandexResult>>(json);
        }

        public static string ApplyYandexCorrections(string original, List<YandexResult> suggestions)
        {
            if (suggestions == null || suggestions.Count == 0) return original;

            suggestions.Sort((a, b) => a.pos.CompareTo(b.pos));
            int cursor = 0;
            string outStr = "";

            foreach (var s in suggestions)
            {
                outStr += original.Substring(cursor, s.pos - cursor);
                var replacement = (s.s != null && s.s.Count > 0)
                    ? s.s[0]
                    : original.Substring(s.pos, s.len);

                outStr += replacement;
                cursor = s.pos + s.len;
            }

            outStr += original.Substring(cursor);
            return outStr;
        }

        // === LanguageTool ===
        public static async Task<JsonDocument> CallLanguageTool(string text)
        {
            var body = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["text"] = text,
                ["language"] = "ru"
                // ["level"] = "picky"
            });

            var res = await _http.PostAsync("https://api.languagetool.org/v2/check", body);
            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadAsStringAsync();
            return JsonDocument.Parse(json);
        }
    }
}
