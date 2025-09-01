using ClickableTransparentOverlay;
using ImGuiNET;
using MajesticHub.Core.binds;
using MHbinder.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MajesticHub
{
    public class Renderer : Overlay
    {


        public void ApplyTheme()
        {
            ApplyMajesticTheme();
            _themeApplied = true;
        }

        static string ExtractResourceToFile(string resourceName, string fileName)
        {
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Resource {resourceName} not found");
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            File.WriteAllBytes(fileName, ms.ToArray());
            return fileName;
        }



        private bool _menuOpen;
        private bool _altTabHidden;
        private bool _themeApplied;
        private nint _hwnd;

        // === Custom Tabs ===
        private enum Tab { Grammar, Rules, Forum, Logs, Binder, Other, Frequent }
        private Tab _activeTab = Tab.Grammar;

        // accent
        private Vector4 _accent = new(0.95f, 0.15f, 0.55f, 1.00f);
        private Vector4 _accentHover = new(1.00f, 0.25f, 0.65f, 1.00f);
        private Vector4 _accentActive = new(0.75f, 0.05f, 0.40f, 1.00f);

        // === Grammar ===
        private string _grammarText = "";
        private string _correctedText = "";
        private List<GrammarClient.YandexResult> _yandexResults = new();
        private JsonDocument? _langToolJson;

        // === Rules Search ===
        private string _rulesQuery = "";
        private string _rulesResult = "";

        // === Forum Stacker ===
        private string _forumInput = "";
        private string _forumOutput = "";
        private List<string> _forumErrors = new();

        // === Logs Stacker ===
        private string _logsInput = "";
        private string _logsOutput = "";
        private List<string> _logsErrors = new();


        // alletr popup
        private Vector2 _alertPopupPos = new Vector2(400, 200); 
        private bool _alertPopupVisible = false;
        private double _alertPopupTime = 0;
        private string _alertPopupText = "";

        public Renderer() : base(
            "Overlay",
            GetSystemMetrics(0),
            GetSystemMetrics(1))
        { }

        protected override void Render()
        {
            if (!_altTabHidden)
            {
                _hwnd = WindowUtils.TryGetOverlayHwnd(this, "Overlay");
                if (_hwnd != 0)
                {
                    WindowUtils.HideFromAltTab(_hwnd);
                    _altTabHidden = true;
                }
            }

            if (!_themeApplied)
            {
                ApplyMajesticTheme();
                _themeApplied = true;
            }


            if (HotkeyListener.IsHotkeyPressed())
            {
                _menuOpen = !_menuOpen;
                Thread.Sleep(180);
            }



            AlertManager.DrawAlertPopup();
            if (!_menuOpen) return;

            ImGui.SetNextWindowPos(new Vector2(150, 80), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(new Vector2(980, 650), ImGuiCond.FirstUseEver);
            ImGui.Begin("MajesticHub",
                ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoScrollbar
               );

            DrawBackdrop();
            DrawTabbar();

            ImGui.Dummy(new Vector2(1, 6));
            ImGui.Dummy(new Vector2(1, 6));

            switch (_activeTab)
            {
                case Tab.Grammar: DrawGrammar(); break;
                case Tab.Rules: DrawRules(); break;
                case Tab.Forum: DrawForum(); break;
                case Tab.Logs: DrawLogs(); break;
                case Tab.Binder: DrawBinder(); break;
                case Tab.Other: DrawOther(); break;
                case Tab.Frequent: DrawFrequent(); break;
            }

            ImGui.End();
        }


        // ============================ UI Blocks ============================

        private void DrawTabbar()
        {
            ImGui.PushID("tabbar");
            var tabs = new (Tab id, string label, int badge)[]
            {
                (Tab.Grammar, "Граммер", _yandexResults.Count + ((_langToolJson != null && _langToolJson.RootElement.TryGetProperty("matches", out var m)) ? m.GetArrayLength():0)),
                (Tab.Rules,   "Поиск правила", string.IsNullOrWhiteSpace(_rulesResult) ? 0 : 1),
                (Tab.Forum,   "Форум стакер",   _forumErrors.Count),
                (Tab.Logs,    "Лог стакер",    _logsErrors.Count),
                (Tab.Binder,  "Биндер",  0),
                (Tab.Other, "Другое", 0),
                (Tab.Frequent,"Часто", FrequentManager.Items.Count) 
            };

            float padX = 12f;
            float padY = 6f;
            float radius = 10f;

            var dl = ImGui.GetWindowDrawList();
            var startCursor = ImGui.GetCursorScreenPos();
            float barHeight = ImGui.GetTextLineHeight() + padY * 4f;

            var avail = ImGui.GetContentRegionAvail();
            uint strip = ImGui.ColorConvertFloat4ToU32(new Vector4(0.10f, 0.10f, 0.12f, 1f));

            var barMin = startCursor;
            var barMax = new Vector2(startCursor.X + avail.X, startCursor.Y + barHeight);
            dl.AddRectFilled(barMin, barMax, strip, 10f);
            ImGui.SetCursorScreenPos(new Vector2(startCursor.X + 8, startCursor.Y + padY));

            float x = ImGui.GetCursorScreenPos().X;
            float y = ImGui.GetCursorScreenPos().Y;

            foreach (var (id, label, badge) in tabs)
            {
                ImGui.PushID(label);
                var labelSize = ImGui.CalcTextSize(label);
                float w = labelSize.X + padX * 2f + (badge > 0 ? 22f : 0f);
                float h = labelSize.Y + padY * 2f;

                var rectMin = new Vector2(x, y);
                var rectMax = new Vector2(x + w, y + h);

                bool hovered = ImGui.IsMouseHoveringRect(rectMin, rectMax);
                bool clicked = hovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left);

                Vector4 fill = _activeTab == id ? _accentActive * new Vector4(1, 1, 1, 0.55f)
                              : hovered ? _accentHover * new Vector4(1, 1, 1, 0.25f)
                              : new Vector4(0.18f, 0.18f, 0.21f, 1f);
                uint cFill = ImGui.ColorConvertFloat4ToU32(fill);
                dl.AddRectFilled(rectMin, rectMax, cFill, radius);

                if (_activeTab == id)
                {
                    uint cUnder = ImGui.ColorConvertFloat4ToU32(_accent);
                    dl.AddRectFilled(new Vector2(rectMin.X, rectMax.Y - 3), new Vector2(rectMax.X, rectMax.Y), cUnder, 2f);
                }

                uint cText = ImGui.ColorConvertFloat4ToU32(new Vector4(0.96f, 0.96f, 0.98f, 1));
                dl.AddText(new Vector2(rectMin.X + padX, rectMin.Y + padY - 1), cText, label);

                if (badge > 0)
                {
                    string b = badge > 99 ? "99+" : badge.ToString();
                    var bSize = ImGui.CalcTextSize(b);
                    float bx = rectMax.X - bSize.X - 10;
                    float by = rectMin.Y + (h - bSize.Y) * 0.5f;
                    uint cBadgeBg = ImGui.ColorConvertFloat4ToU32(new Vector4(0.95f, 0.25f, 0.35f, 1f));
                    dl.AddRectFilled(new Vector2(bx - 6, by - 3), new Vector2(bx + bSize.X + 6, by + bSize.Y + 3), cBadgeBg, 8f);
                    uint cBadgeTx = ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1));
                    dl.AddText(new Vector2(bx, by), cBadgeTx, b);
                }

                if (clicked) _activeTab = id;
                x += w + 8f;
                ImGui.PopID();
            }
            ImGui.PopID();
        }

        private void DrawGrammar()
        {
            ImGui.Text("Введите текст для проверки грамматических ошибок");
            ImGui.Separator();

            ImGui.Columns(2, "grammar_columns", true);

            ImGui.BeginChild("grammar_input", new Vector2(0, 300));
            ImGui.Text("Ваш текст:");
            ImGui.InputTextMultiline("##grammarText", ref _grammarText, 5000, new Vector2(-1, -1));
            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild("grammar_output", new Vector2(0, 300));
            ImGui.Text("Исправленный текст:");
            ImGui.InputTextMultiline("##corrected", ref _correctedText, 5000, new Vector2(-1, -1));
            ImGui.EndChild();

            ImGui.Columns(1);

            if (GhostButton("Проверить", new Vector2(140, 36)))
            {
                Task.Run(async () =>
                {
                    var yandex = await GrammarClient.CallYandexSpeller(_grammarText);
                    var corrected = GrammarClient.ApplyYandexCorrections(_grammarText, yandex);
                    var lt = await GrammarClient.CallLanguageTool(corrected);

                    _correctedText = corrected;
                    _yandexResults = yandex;
                    _langToolJson = lt;
                });
            }
            ImGui.SameLine();
            if (GhostButton("Скопировать", new Vector2(140, 36)) && !string.IsNullOrEmpty(_correctedText))
                ImGui.SetClipboardText(_correctedText);
            ImGui.SameLine();
            if (DangerGhostButton("Очистить", new Vector2(140, 36)))
            {
                _grammarText = "";
                _correctedText = "";
                _yandexResults.Clear();
                _langToolJson = null;
            }

            if (_yandexResults.Count > 0)
            {
                ImGui.Spacing();
                ImGui.TextColored(new Vector4(1f, 0.6f, 0.6f, 1f), "Ошибки:");
                foreach (var res in _yandexResults)
                {
                    PushCard(new Vector4(0.9f, 0.2f, 0.2f, 0.12f), new Vector4(1f, 0.3f, 0.3f, 0.9f));
                    ImGui.BeginChild($"gerr_{Guid.NewGuid()}",
                        new Vector2(-20, 40), 
                        ImGuiChildFlags.None,
                        ImGuiWindowFlags.NoScrollbar);
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10); 
                    ImGui.TextWrapped($"{res.word} → {string.Join(", ", res.s)}");

                    ImGui.EndChild();
                    PopCard();
                    ImGui.Spacing();
                }
            }

            if (_langToolJson != null && _langToolJson.RootElement.TryGetProperty("matches", out var matches))
            {
                foreach (var m in matches.EnumerateArray())
                {
                    string msg = m.GetProperty("message").GetString() ?? "";
                    string context = m.TryGetProperty("context", out var ctx) && ctx.TryGetProperty("text", out var ctxText)
                        ? ctxText.GetString() ?? ""
                        : "";

                    PushCard(new Vector4(0.2f, 0.4f, 0.9f, 0.12f), new Vector4(0.3f, 0.6f, 1f, 0.9f));
                    ImGui.BeginChild($"lterr_{Guid.NewGuid()}",
                        new Vector2(-20, 40),
                        ImGuiChildFlags.None,
                        ImGuiWindowFlags.NoScrollbar);
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);

                    ImGui.TextWrapped($"{context} → {msg}");

                    ImGui.EndChild();
                    PopCard();
                    ImGui.Spacing();
                }
            }
        }

        private void DrawRules()
        {
            ImGui.Text("Поиск среди всех правил сервера");
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.InputTextWithHint("##rulesQuery", "Введите ключевое слово...", ref _rulesQuery, 500);
            ImGui.SameLine();
            if (GhostButton("Найти"))
                _rulesResult = RuleSearch.FindRule(_rulesQuery);
            ImGui.SameLine();
            if (DangerGhostButton("Очистить") && (!string.IsNullOrEmpty(_rulesQuery) || !string.IsNullOrEmpty(_rulesResult)))
            {
                _rulesQuery = "";
                _rulesResult = "";
            }

            ImGui.Spacing();

            ImGui.BeginChild("rules_output", new Vector2(0, 400), ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar);
            ImGui.InputTextMultiline("##rulesResult", ref _rulesResult, 10000, new Vector2(-1, -1), ImGuiInputTextFlags.ReadOnly);
            ImGui.EndChild();
        }

        private void DrawForum()
        {
            ImGui.Text("Обработка данных наказаний из форума");
            ImGui.Separator();

            ImGui.Columns(2, "forum_columns", true);

            ImGui.BeginChild("forum_input", new Vector2(0, 300), ImGuiChildFlags.None, ImGuiWindowFlags.None);
            ImGui.Text("Ввод:");
            ImGui.InputTextMultiline("##forumInput", ref _forumInput, 10000, new Vector2(-1, -1));
            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild("forum_output", new Vector2(0, 300), ImGuiChildFlags.None, ImGuiWindowFlags.None);
            ImGui.Text("Результат:");
            ImGui.InputTextMultiline("##forumOutput", ref _forumOutput, 10000, new Vector2(-1, -1), ImGuiInputTextFlags.ReadOnly);
            ImGui.EndChild();

            ImGui.Columns(1);
            ImGui.Spacing();

            if (_forumErrors.Count > 0)
            {
                ImGui.TextColored(new Vector4(1f, 0.4f, 0.4f, 1f), $"Ошибки ({_forumErrors.Count}):");
                ImGui.Spacing();

                foreach (var err in _forumErrors)
                {
                    PushCard(new Vector4(0.9f, 0.2f, 0.2f, 0.12f), new Vector4(1f, 0.3f, 0.3f, 0.9f));
                    ImGui.BeginChild($"ferr_{Guid.NewGuid()}",
                        new Vector2(-1, 50),
                        ImGuiChildFlags.None,
                        ImGuiWindowFlags.NoScrollbar);

                    ImGui.TextWrapped(err);

                    ImGui.EndChild();
                    PopCard();
                    ImGui.Spacing();
                }
            }

            ImGui.Spacing();

            if (GhostButton("Обработать", new Vector2(140, 36)))
            {
                try
                {
                    (_forumOutput, _forumErrors) = ForumStacker.Process(_forumInput);
                }
                catch (Exception ex)
                {
                    _forumErrors.Clear();
                    _forumErrors.Add("Ошибка обработки: " + ex.Message);
                }
            }

            ImGui.SameLine();
            if (GhostButton("Копировать", new Vector2(140, 36)) && !string.IsNullOrEmpty(_forumOutput))
                ImGui.SetClipboardText(_forumOutput);

            ImGui.SameLine();
            if (DangerGhostButton("Очистить", new Vector2(140, 36)))
            {
                _forumInput = "";
                _forumOutput = "";
                _forumErrors.Clear();
            }
        }

        private void DrawLogs()
        {
            ImGui.Text("Обработка данных наказаний из логов");
            ImGui.Separator();

            ImGui.Columns(2, "logs_columns", true);

            ImGui.BeginChild("logs_input", new Vector2(0, 300), ImGuiChildFlags.None, ImGuiWindowFlags.None);
            ImGui.Text("Ввод:");
            ImGui.InputTextMultiline("##logsInput", ref _logsInput, 5000, new Vector2(-1, -1));
            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild("logs_output", new Vector2(0, 300), ImGuiChildFlags.None, ImGuiWindowFlags.None);
            ImGui.Text("Результат:");
            if (_logsErrors.Count > 0)
            {
                foreach (var err in _logsErrors)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 0.3f, 0.3f, 1f));
                    ImGui.TextWrapped(err);
                    ImGui.PopStyleColor();
                    ImGui.Separator();
                }
            }
            else
            {
                ImGui.InputTextMultiline("##logsOutput", ref _logsOutput, 10000, new Vector2(-1, -1), ImGuiInputTextFlags.ReadOnly);
            }
            ImGui.EndChild();

            ImGui.Columns(1);
            ImGui.Spacing();

            if (GhostButton("Обработать", new Vector2(140, 36)))
            {
                var (results, errors) = LogStacker.Process(_logsInput);
                if (errors.Count > 0)
                {
                    _logsErrors = errors;
                    _logsOutput = "";
                }
                else
                {
                    _logsErrors.Clear();
                    _logsOutput = string.Join("\n", results.Select(r =>
                    {
                        string names = string.Join(", ", r.Names);
                        return $"{r.Punish} {r.Id} {r.Time} {names}";
                    }));
                }
            }

            ImGui.SameLine();
            if (GhostButton("Копировать", new Vector2(140, 36)) && !string.IsNullOrEmpty(_logsOutput))
                ImGui.SetClipboardText(_logsOutput);

            ImGui.SameLine();
            if (DangerGhostButton("Очистить", new Vector2(140, 36)))
            {
                _logsInput = "";
                _logsOutput = "";
                _logsErrors.Clear();
            }
        }

        private void DrawBinder()
        {
            ImGui.Text("Список биндов");
            ImGui.Separator();

            ImGui.BeginChild("binder_scroll", new Vector2(0, 420), ImGuiChildFlags.None, ImGuiWindowFlags.AlwaysVerticalScrollbar);

            int index = 0;
            foreach (var bind in BinderManager.Binds.ToList())
            {
                ImGui.PushID(index);

                ImGui.Columns(2, $"bind_cols_{index}", false);

                string trigger = bind.Trigger ?? "";
                if (ImGui.InputText($"Триггер##{index}", ref trigger, 64))
                {
                    bind.Trigger = trigger;
                    ConfigManager.Save();
                }

                ImGui.NextColumn();

                string replace = bind.Replace ?? "";
                if (ImGui.InputText($"Текст##{index}", ref replace, 256))
                {
                    bind.Replace = replace;
                    ConfigManager.Save();
                }

                ImGui.Columns(1);

                if (DangerGhostButton("Удалить", new Vector2(100, 28)))
                {
                    BinderManager.RemoveBind(bind);
                    ImGui.PopID();
                    break;
                }

                ImGui.Spacing();
                ImGui.PopID();
                index++;
            }

            ImGui.EndChild();

            ImGui.Spacing();

            if (PrimaryButton("Добавить бинд"))
            {
                BinderManager.AddBind("", "");
                ConfigManager.Save();
            }
        }

        private void DrawOther()
        {
            ImGui.Text("Важное и не очень:");
            ImGui.Separator();
            ImGui.Spacing();

            string discordLink = "https://discord.gg/J5qUHAqw";

            ImGui.InputText("##discordLink", ref discordLink, 256, ImGuiInputTextFlags.ReadOnly);

            if (ImGui.Button("Скопировать ссылку", new Vector2(200, 36)))
            {
                ImGui.SetClipboardText(discordLink);
                AlertManager.ShowAlert("Успешно скопированно в буфер обмена");
            }


        }

        private void DrawFrequent()
        {
            ImGui.Text("Частые фразы");
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.BeginChild("frequent_scroll", new Vector2(0, 500), ImGuiChildFlags.None, ImGuiWindowFlags.AlwaysVerticalScrollbar);

            int index = 0;
            foreach (var item in FrequentManager.Items.ToList())
            {
                ImGui.PushID(index);

                string phrase = item.Phrase;
                if (ImGui.InputText($"##phrase{index}", ref phrase, 1024))
                {
                    item.Phrase = phrase; 
                    FrequentManager.Save(); 
                }

                ImGui.SameLine();

                if (ImGui.Button("Скопировать", new Vector2(120, 28)))
                {
                    ImGui.SetClipboardText(item.Phrase);
                    AlertManager.ShowAlert("Фраза скопирована в буфер обмена!");
                }

                ImGui.SameLine();

                if (DangerGhostButton("Удалить", new Vector2(100, 28)))
                {
                    FrequentManager.RemoveItem(item);
                    ImGui.PopID();
                    break;
                }

                ImGui.Spacing();
                ImGui.PopID();
                index++;
            }

            ImGui.EndChild();

            if (PrimaryButton("Добавить фразу"))
                FrequentManager.AddItem("");
        }



        public void DrawAllertPopup()
        {
            ImGui.OpenPopup("CopiedPopup");
        }


        // ============================ Helpers ============================

        private void DrawBackdrop()
        {
            var dl = ImGui.GetWindowDrawList();
            var pos = ImGui.GetWindowPos();
            var size = ImGui.GetWindowSize();

            uint col = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.1f, 0.5f, 0.18f)); 
            float thickness = 3.0f;

            Vector2 start = new Vector2(pos.X, pos.Y + size.Y * 0.05f);

            // волны
            Vector2 c1 = new Vector2(pos.X + size.X * 0.25f, pos.Y + size.Y * 0.20f);
            Vector2 c2 = new Vector2(pos.X + size.X * 0.50f, pos.Y + size.Y * 0.60f);
            Vector2 mid = new Vector2(pos.X + size.X * 0.5f, pos.Y + size.Y * 0.45f);

            Vector2 c3 = new Vector2(pos.X + size.X * 0.75f, pos.Y + size.Y * 0.30f);
            Vector2 c4 = new Vector2(pos.X + size.X * 0.90f, pos.Y + size.Y * 0.80f);
            Vector2 end = new Vector2(pos.X + size.X, pos.Y + size.Y * 0.70f);

            dl.PathClear();
            dl.PathLineTo(start);
            dl.PathBezierCubicCurveTo(c1, c2, mid);
            dl.PathBezierCubicCurveTo(c3, c4, end);

            dl.PathStroke(col, ImDrawFlags.None, thickness);
        }



        private static void PushCard(Vector4 bg, Vector4 border)
        {
            ImGui.PushStyleColor(ImGuiCol.ChildBg, bg);
            ImGui.PushStyleColor(ImGuiCol.Border, border);
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 8f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(15, 8));
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(6, 4));
        }

        private static void PopCard()
        {
            ImGui.PopStyleVar(3);
            ImGui.PopStyleColor(2);
        }

        private bool PrimaryButton(string text, Vector2? size = null)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, _accent);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, _accentHover);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, _accentActive);
            bool pressed = ImGui.Button(text, size ?? new Vector2(120, 35));
            ImGui.PopStyleColor(3);
            return pressed;
        }

        private bool GhostButton(string text, Vector2? size = null)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f);
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, _accent * new Vector4(1, 1, 1, 0.2f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, _accent * new Vector4(1, 1, 1, 0.3f));
            ImGui.PushStyleColor(ImGuiCol.Border, _accent * new Vector4(1, 1, 1, 0.6f));
            bool pressed = ImGui.Button(text, size ?? new Vector2(120, 35));
            ImGui.PopStyleColor(4);
            ImGui.PopStyleVar();
            return pressed;
        }

        private bool DangerGhostButton(string text, Vector2? size = null)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f);
            var danger = new Vector4(0.95f, 0.25f, 0.35f, 1f);
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, danger * new Vector4(1, 1, 1, 0.15f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, danger * new Vector4(1, 1, 1, 0.25f));
            ImGui.PushStyleColor(ImGuiCol.Border, danger);
            bool pressed = ImGui.Button(text, size ?? new Vector2(120, 35));
            ImGui.PopStyleColor(4);
            ImGui.PopStyleVar();
            return pressed;
        }

        private bool PillButton(string text)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 100f);
            bool p = GhostButton(text, new Vector2(160, 28));
            ImGui.PopStyleVar();
            return p;
        }

        private void ApplyMajesticTheme()
        {
            var style = ImGui.GetStyle();
            var colors = style.Colors;

            _accent = new Vector4(0.95f, 0.15f, 0.55f, 1.0f);
            _accentHover = new Vector4(1f, 0.25f, 0.65f, 1.0f);
            _accentActive = new Vector4(0.75f, 0.05f, 0.40f, 1.0f);

            colors[(int)ImGuiCol.Text] = new Vector4(0.95f, 0.95f, 0.97f, 1.0f);
            colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.55f, 0.58f, 0.60f, 1.0f);

            colors[(int)ImGuiCol.WindowBg] = new Vector4(0.08f, 0.08f, 0.10f, 1.0f);
            colors[(int)ImGuiCol.ChildBg] = new Vector4(0.10f, 0.10f, 0.12f, 1.0f);
            colors[(int)ImGuiCol.PopupBg] = new Vector4(0.10f, 0.10f, 0.12f, 1.0f);

            colors[(int)ImGuiCol.Border] = new Vector4(0.28f, 0.28f, 0.33f, 1.0f);

            colors[(int)ImGuiCol.Button] = _accent;
            colors[(int)ImGuiCol.ButtonHovered] = _accentHover;
            colors[(int)ImGuiCol.ButtonActive] = _accentActive;

            colors[(int)ImGuiCol.FrameBg] = new Vector4(0.15f, 0.15f, 0.18f, 1.0f);
            colors[(int)ImGuiCol.FrameBgHovered] = _accentHover * new Vector4(1, 1, 1, 0.6f);
            colors[(int)ImGuiCol.FrameBgActive] = _accentActive;

            colors[(int)ImGuiCol.Header] = new Vector4(0.18f, 0.18f, 0.21f, 1.0f);
            colors[(int)ImGuiCol.HeaderHovered] = _accentHover * new Vector4(1, 1, 1, 0.35f);
            colors[(int)ImGuiCol.HeaderActive] = _accentActive;

            style.WindowRounding = 16f;
            style.FrameRounding = 12f;
            style.GrabRounding = 12f;

            style.FramePadding = new Vector2(14, 8);
            style.ItemSpacing = new Vector2(18, 14);
            style.ScrollbarSize = 18f;

            style.WindowTitleAlign = new Vector2(0.5f, 0.5f);
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);

            // === Грузим шрифт ===
            var io = ImGui.GetIO();
            io.Fonts.Clear();

            string interPath = ExtractResourceToFile(
                "MHbinder.assets.fonts.Inter-VariableFont_opsz,wght.ttf",
                "Inter.ttf");

            io.Fonts.AddFontFromFileTTF(interPath, 18.0f, null, io.Fonts.GetGlyphRangesCyrillic());
            io.Fonts.Build();
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);
    }

    public static class AlertManager
    {
        private static Vector2 _alertPopupPos = new Vector2(400, 200);
        private static bool _alertPopupVisible = false;
        private static double _alertPopupTime = 0;
        private static string _alertPopupText = "";

        public static void ShowAlert(string text)
        {
            _alertPopupText = text;
            _alertPopupVisible = true;
            _alertPopupTime = ImGui.GetTime();
        }

        public static void DrawAlertPopup()
        {
            if (!_alertPopupVisible)
                return;

            ImGui.SetNextWindowPos(_alertPopupPos, ImGuiCond.Appearing);
            if (ImGui.BeginPopup("AlertPopup", ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.TextColored(new Vector4(0.5f, 1f, 0.5f, 1f), _alertPopupText);
                _alertPopupPos = ImGui.GetWindowPos();
                ImGui.EndPopup();
            }
            else { ImGui.OpenPopup("AlertPopup");}

            if (ImGui.GetTime() - _alertPopupTime > 2.0)
                _alertPopupVisible = false;
        }
    }

}
