
using ImGuiNET;
using MHbinder;
using System.Numerics;

namespace MHbinder.Core.UI
{
    public enum ThemeType   
    {
        Legacy,
        Dark,
        Light
    }

    public static class ThemeManager
    {
        public static Vector4 Accent { get; private set; }
        public static Vector4 AccentHover { get; private set; }
        public static Vector4 AccentActive { get; private set; }

        public static void ApplyTheme(ThemeType theme)
        {
            switch (theme)
            {
                case ThemeType.Legacy:
                    ApplyLegacyTheme();
                    break;
                case ThemeType.Dark:
                    ApplyDarkTheme();
                    break;
                case ThemeType.Light:
                    ApplyLightTheme();
                    break;
            }
        }

        private static void ApplyLegacyTheme()
        {
            var style = ImGui.GetStyle();
            var colors = style.Colors;

            Accent = new Vector4(0.95f, 0.15f, 0.55f, 1.0f);
            AccentHover = new Vector4(1f, 0.25f, 0.65f, 1.0f);
            AccentActive = new Vector4(0.75f, 0.05f, 0.40f, 1.0f);

            colors[(int)ImGuiCol.Text] = new Vector4(0.95f, 0.95f, 0.97f, 1.0f);
            colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.55f, 0.58f, 0.60f, 1.0f);

            colors[(int)ImGuiCol.WindowBg] = new Vector4(0.08f, 0.08f, 0.10f, 1.0f);
            colors[(int)ImGuiCol.ChildBg] = new Vector4(0.10f, 0.10f, 0.12f, 1.0f);
            colors[(int)ImGuiCol.PopupBg] = new Vector4(0.10f, 0.10f, 0.12f, 1.0f);

            colors[(int)ImGuiCol.Border] = new Vector4(0.28f, 0.28f, 0.33f, 1.0f);

            colors[(int)ImGuiCol.Button] = Accent;
            colors[(int)ImGuiCol.ButtonHovered] = AccentHover;
            colors[(int)ImGuiCol.ButtonActive] = AccentActive;

            colors[(int)ImGuiCol.FrameBg] = new Vector4(0.15f, 0.15f, 0.18f, 1.0f);
            colors[(int)ImGuiCol.FrameBgHovered] = AccentHover * new Vector4(1, 1, 1, 0.6f);
            colors[(int)ImGuiCol.FrameBgActive] = AccentActive;

            colors[(int)ImGuiCol.Header] = new Vector4(0.18f, 0.18f, 0.21f, 1.0f);
            colors[(int)ImGuiCol.HeaderHovered] = AccentHover * new Vector4(1, 1, 1, 0.35f);
            colors[(int)ImGuiCol.HeaderActive] = AccentActive;

            style.WindowRounding = 16f;
            style.FrameRounding = 12f;
            style.GrabRounding = 12f;

            style.FramePadding = new Vector2(12, 6);
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.ItemSpacing = new Vector2(18, 14);
            style.ScrollbarSize = 18f;
            style.WindowTitleAlign = new Vector2(0.5f, 0.5f);

        }

        private static void ApplyDarkTheme()
        {
            var style = ImGui.GetStyle();
            var colors = style.Colors;

            // Основной акцент для кнопок
            Accent = new Vector4(0.25f, 0.5f, 1f, 1f);
            AccentHover = new Vector4(0.35f, 0.6f, 1f, 1f);
            AccentActive = new Vector4(0.15f, 0.4f, 0.9f, 1f);

            // Текст
            colors[(int)ImGuiCol.Text] = new Vector4(0.90f, 0.90f, 0.92f, 1.0f);
            colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.50f, 0.50f, 0.55f, 1.0f);

            // Фон окон и детей
            colors[(int)ImGuiCol.WindowBg] = new Vector4(0.05f, 0.05f, 0.06f, 1.0f);
            colors[(int)ImGuiCol.ChildBg] = new Vector4(0.08f, 0.08f, 0.10f, 1.0f);
            colors[(int)ImGuiCol.PopupBg] = new Vector4(0.08f, 0.08f, 0.10f, 1.0f);

            // Бордеры
            colors[(int)ImGuiCol.Border] = new Vector4(0.22f, 0.22f, 0.25f, 1.0f);

            // Кнопки
            colors[(int)ImGuiCol.Button] = Accent;
            colors[(int)ImGuiCol.ButtonHovered] = AccentHover;
            colors[(int)ImGuiCol.ButtonActive] = AccentActive;

            // Фреймы
            colors[(int)ImGuiCol.FrameBg] = new Vector4(0.12f, 0.12f, 0.15f, 1.0f);
            colors[(int)ImGuiCol.FrameBgHovered] = AccentHover * new Vector4(1, 1, 1, 0.6f);
            colors[(int)ImGuiCol.FrameBgActive] = AccentActive;

            // Заголовки (Header)
            colors[(int)ImGuiCol.Header] = new Vector4(0.15f, 0.15f, 0.18f, 1.0f);
            colors[(int)ImGuiCol.HeaderHovered] = AccentHover * new Vector4(1, 1, 1, 0.35f);
            colors[(int)ImGuiCol.HeaderActive] = AccentActive;

            // Scrollbars, Rounding и отступы оставляем как в Legacy
            style.WindowRounding = 16f;
            style.FrameRounding = 12f;
            style.GrabRounding = 12f;

            style.FramePadding = new Vector2(12, 6);
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.ItemSpacing = new Vector2(18, 14);
            style.ScrollbarSize = 18f;
            style.WindowTitleAlign = new Vector2(0.5f, 0.5f);
        }

        private static void ApplyLightTheme()
        {
            var style = ImGui.GetStyle();
            var colors = style.Colors;

            // Основной акцент для кнопок
            Accent = new Vector4(0.2f, 0.4f, 0.8f, 1.0f);
            AccentHover = new Vector4(0.3f, 0.5f, 0.9f, 1.0f);
            AccentActive = new Vector4(0.1f, 0.3f, 0.7f, 1.0f);

            // Текст
            colors[(int)ImGuiCol.Text] = new Vector4(0.05f, 0.05f, 0.05f, 1.0f);
            colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.4f, 0.4f, 0.4f, 1.0f);

            // Фон окон и детей
            colors[(int)ImGuiCol.WindowBg] = new Vector4(0.95f, 0.95f, 0.97f, 1.0f);
            colors[(int)ImGuiCol.ChildBg] = new Vector4(0.9f, 0.9f, 0.92f, 1.0f);
            colors[(int)ImGuiCol.PopupBg] = new Vector4(0.9f, 0.9f, 0.92f, 1.0f);

            // Бордеры
            colors[(int)ImGuiCol.Border] = new Vector4(0.7f, 0.7f, 0.75f, 1.0f);

            // Кнопки
            colors[(int)ImGuiCol.Button] = Accent;
            colors[(int)ImGuiCol.ButtonHovered] = AccentHover;
            colors[(int)ImGuiCol.ButtonActive] = AccentActive;

            // Фреймы
            colors[(int)ImGuiCol.FrameBg] = new Vector4(0.85f, 0.85f, 0.88f, 1.0f);
            colors[(int)ImGuiCol.FrameBgHovered] = AccentHover * new Vector4(1, 1, 1, 0.6f);
            colors[(int)ImGuiCol.FrameBgActive] = AccentActive;

            // Заголовки (Header)
            colors[(int)ImGuiCol.Header] = new Vector4(0.88f, 0.88f, 0.91f, 1.0f);
            colors[(int)ImGuiCol.HeaderHovered] = AccentHover * new Vector4(1, 1, 1, 0.35f);
            colors[(int)ImGuiCol.HeaderActive] = AccentActive;

            // Scrollbars, Rounding и отступы оставляем как в Legacy
            style.WindowRounding = 16f;
            style.FrameRounding = 12f;
            style.GrabRounding = 12f;

            style.FramePadding = new Vector2(12, 6);
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.ItemSpacing = new Vector2(18, 14);
            style.ScrollbarSize = 18f;
            style.WindowTitleAlign = new Vector2(0.5f, 0.5f);
        }

        public static void LoadFonts()
        {
            var io = ImGui.GetIO();
            io.Fonts.Clear();

            string interPath = Renderer.ExtractResourceToFile(
                "MHbinder.assets.fonts.Inter-VariableFont_opsz,wght.ttf",
                "Inter.ttf");

            io.Fonts.AddFontFromFileTTF(interPath, 18.0f, null, io.Fonts.GetGlyphRangesCyrillic());
            io.Fonts.Build();
        }
    }
}

