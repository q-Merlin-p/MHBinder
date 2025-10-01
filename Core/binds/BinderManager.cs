using ImGuiNET;
using MHbinder.Core;
using MHbinder.Core.binds;
using System.Collections.Generic;
using System.Linq;

namespace MHbinder.Core.binds
{
    public static class BinderManager
    {
        public static List<BinderItem> Binds { get; private set; } = new();

        public static void AddBind(string trigger, string replace)
        {
            Binds.Add(new BinderItem(trigger, replace));
            ConfigManager.Save();
            //HotkeyListener.CheckTriggers();
        }

        public static void RemoveBind(BinderItem bind)
        {
            Binds.Remove(bind);
            ConfigManager.Save();
        }

        public static void LoadFromConfig(Dictionary<string, string> cfg)
        {
            Binds.Clear();

            foreach (var kvp in cfg)
            {
                if (!kvp.Key.StartsWith("bind")) continue;
                int index = kvp.Value.IndexOf(',');
                if (index == -1) continue;

                string triggerPart = kvp.Value.Substring(0, index).Trim();
                string replacePart = kvp.Value.Substring(index + 1).Trim();
                string trigger = "", replace = "";

                if (triggerPart.StartsWith("trigger:", StringComparison.OrdinalIgnoreCase))
                    trigger = triggerPart.Substring("trigger:".Length).Trim();

                if (replacePart.StartsWith("replace:", StringComparison.OrdinalIgnoreCase))
                    replace = replacePart.Substring("replace:".Length).Trim(); 

                if (!string.IsNullOrEmpty(trigger) && !string.IsNullOrEmpty(replace))
                    Binds.Add(new BinderItem(trigger, replace));
            }
        }


        public static Dictionary<string, string> SaveToConfig()
        {
            var dict = new Dictionary<string, string>();
            int i = 1;
            foreach (var b in Binds)
            {
                dict[$"bind{i}"] = $"trigger:{b.Trigger}, replace:{b.Replace}";
                i++;
            }
            return dict;
        }
    }
}
