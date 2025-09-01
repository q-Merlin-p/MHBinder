using System;
using static MajesticHub.Program;

namespace MajesticHub
{
    public static class ConsoleSplash
    {
        public static void Show()
        {
            string[] logo = {
"___  ___      _           _   _        _   _       _  ",
"|  \\/  |     (_)         | | (_)      | | | |     | | ",
"| .  . | __ _ _  ___  ___| |_ _  ___  | |_| |_   _| |__ ",
"| |\\/| |/ _` | |/ _ \\/ __| __| |/ __| |  _  | | | | '_ \\  ",
"| |  | | (_| | |  __/\\__ \\ |_| | (__  | | | | |_| | |_) |  ",
"\\_|  |_/\\__,_| |\\___||___/\\__|_|\\___| \\_| |_/\\__,_|_.__/  ",
"            _/ |   ",
"           |__/ ",
"        "
};
            (int R, int G, int B) start = (255, 0, 128); 
            (int R, int G, int B) end = (0, 128, 255); 

            for (int i = 0; i < logo.Length; i++)
            {
                double t = (double)i / (logo.Length - 1); 
                int r = (int)(start.R + (end.R - start.R) * t);
                int g = (int)(start.G + (end.G - start.G) * t);
                int b = (int)(start.B + (end.B - start.B) * t);

                Console.WriteLine($"\u001b[38;2;{r};{g};{b}m{logo[i]}\u001b[0m");
            }

            Console.WriteLine($"MajesticHub v{AppInfo.Version} by Merlin @devxzcd");
            Console.WriteLine("Repo: https://github.com/Majestic-HUB-Dev/MHBinder");
            Console.WriteLine($"Press {MHbinder.Core.ConfigManager.OpenMenuKey} to open/close overlay\n");
        }
    }
}
