using ImGuiNET;
using MajesticHub.Core.binds;
using MHbinder.Core;
using System;
using System.Reflection;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace MajesticHub
{
    internal static class Program
    {
        public static class AppInfo
        {
            public const string Version = "1.2.1 DEV";
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

            renderer.ApplyTheme();

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
