using System.Numerics;
using ImGuiNET;

namespace MHbinder.Core.UI
{
    public static class Style
    {
        public static bool PrimaryButton(string text, Vector2? size = null)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, ThemeManager.Accent);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ThemeManager.AccentHover);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, ThemeManager.AccentActive);
            bool pressed = ImGui.Button(text, size ?? new Vector2(120, 35));
            ImGui.PopStyleColor(3);
            return pressed;
        }

        public static bool GhostButton(string text, Vector2? size = null)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f);
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ThemeManager.Accent * new Vector4(1, 1, 1, 0.2f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, ThemeManager.Accent * new Vector4(1, 1, 1, 0.3f));
            ImGui.PushStyleColor(ImGuiCol.Border, ThemeManager.Accent * new Vector4(1, 1, 1, 0.6f));
            bool pressed = ImGui.Button(text, size ?? new Vector2(120, 35));
            ImGui.PopStyleColor(4);
            ImGui.PopStyleVar();
            return pressed;
        }

        public static bool DangerGhostButton(string text, Vector2? size = null)
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

        public static bool InfoGhostButton(string text, Vector2? size = null)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f);
            var info = new Vector4(0.25f, 0.55f, 0.95f, 1f);
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, info * new Vector4(1, 1, 1, 0.15f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, info * new Vector4(1, 1, 1, 0.25f));
            ImGui.PushStyleColor(ImGuiCol.Border, info);
            bool pressed = ImGui.Button(text, size ?? new Vector2(120, 35));
            ImGui.PopStyleColor(4);
            ImGui.PopStyleVar();
            return pressed;
        }

        public static bool AccentGhostButton(string text, Vector2? size = null)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f);
            var accent = ThemeManager.Accent;
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, accent * new Vector4(1, 1, 1, 0.15f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, accent * new Vector4(1, 1, 1, 0.25f));
            ImGui.PushStyleColor(ImGuiCol.Border, accent);
            bool pressed = ImGui.Button(text, size ?? new Vector2(120, 35));
            ImGui.PopStyleColor(4);
            ImGui.PopStyleVar();
            return pressed;
        }
    }
}
