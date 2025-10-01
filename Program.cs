using ImGuiNET;
using MHbinder.Core.binds;
using MHbinder.Core;
using MHbinder.Core.UI;
using System;
using System.Reflection;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace MHbinder
{
    internal static class Program
    {
        public static class AppInfo
        {
            public const string Version = "1.3.0 DEV";
        }

        static void Main(string[] args)
        {
            AnsiSupport.Enable();
            ConsoleSplash.Show();

            ImGui.CreateContext();

            var renderer = new Renderer();
            var asm = Assembly.GetExecutingAssembly();
            Console.WriteLine("=== Embedded resources ===");
            foreach (var res in asm.GetManifestResourceNames())
                Console.WriteLine(res);
            Console.WriteLine("==========================");

            MHbinder.Core.UI.ThemeManager.LoadFonts();

            ConfigManager.Load();
            HotkeyListener.LoadConfigKey(ConfigManager.OpenMenuKey);
            HotkeyListener.GetBinds = () => BinderManager.Binds;
            FrequentManager.LoadFromConfig(ConfigManager.LoadDictionary());

            renderer.Start();
            while (true)
            {
                HotkeyListener.Poll();
                Thread.Sleep(10); 
            }
        }
    }
}
