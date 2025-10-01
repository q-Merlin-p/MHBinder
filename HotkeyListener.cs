using MHbinder.Core.binds;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace MHbinder.Core
{
    public static class HotkeyListener
    {
        [DllImport("user32.dll")]
        private static extern int ToUnicode(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr)]
    StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags);

        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        private static string _menuKey = "RightShift";
        private static readonly List<char> _buffer = new();
        private static int _maxBuffer = 50;

        public static Func<List<BinderItem>>? GetBinds;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        public static void LoadConfigKey(string key) => _menuKey = key;

        public static bool IsHotkeyPressed()
        {
            int vk = GetVkCode(_menuKey);
            if (vk == 0) return false;
            return (GetAsyncKeyState(vk) & 0x8000) != 0;
        }

        public static void Poll()
        {
            for (int vk = 0x20; vk <= 0x5A; vk++)
            {
                if ((GetAsyncKeyState(vk) & 0x8000) != 0)
                {
                    char c = VkToChar(vk);
                    if (c != '\0')
                        RegisterChar(c);
                }
            }
        }

        private static void RegisterChar(char c)
        {
            _buffer.Add(c);
            if (_buffer.Count > _maxBuffer)
                _buffer.RemoveAt(0);

            CheckTriggers();
        }

        private static void CheckTriggers()
        {
            if (GetBinds == null) return;
            string current = new string(_buffer.ToArray());

            foreach (var bind in GetBinds())
            {
                if (!string.IsNullOrEmpty(bind.Trigger) &&
                    current.EndsWith(bind.Trigger, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"[KeyBind Triggered] {bind.Trigger} -> {bind.Replace}");

                    ClipboardHelper.SetText(bind.Replace);
                    MHbinder.AlertManager.ShowAlert("Успешно скопированно в буфер обмена");
                    break;
                }
            }
        }


        private static char VkToChar(int vk)
        {
            byte[] keyboardState = new byte[256];
            if (!GetKeyboardState(keyboardState))
                return '\0';

            StringBuilder sb = new StringBuilder(2);
            int result = ToUnicode((uint)vk, 0, keyboardState, sb, sb.Capacity, 0);
            if (result > 0)
                return sb[0];
            return '\0';
        }

        private static int GetVkCode(string key)
        {
            return key.ToUpper() switch
            {
                "RIGHTSHIFT" => 0xA1,
                "LEFTSHIFT" => 0xA0,
                "RIGHTCTRL" => 0xA3,
                "LEFTCTRL" => 0xA2,
                "RIGHTALT" => 0xA5,
                "LEFTALT" => 0xA4,
                "TAB" => 0x09,
                "CAPSLOCK" => 0x14,
                "ESC" or "ESCAPE" => 0x1B,
                "SPACE" => 0x20,
                "ENTER" => 0x0D,
                "F1" => 0x70,
                "F2" => 0x71,
                "F3" => 0x72,
                "F4" => 0x73,
                "F5" => 0x74,
                "F6" => 0x75,
                "F7" => 0x76,
                "F8" => 0x77,
                "F9" => 0x78,
                "F10" => 0x79,
                "F11" => 0x7A,
                "F12" => 0x7B,
                "A" => 0x41,
                "B" => 0x42,
                "C" => 0x43,
                "D" => 0x44,
                "E" => 0x45,
                "F" => 0x46,
                "G" => 0x47,
                "H" => 0x48,
                "I" => 0x49,
                "J" => 0x4A,
                "K" => 0x4B,
                "L" => 0x4C,
                "M" => 0x4D,
                "N" => 0x4E,
                "O" => 0x4F,
                "P" => 0x50,
                "Q" => 0x51,
                "R" => 0x52,
                "S" => 0x53,
                "T" => 0x54,
                "U" => 0x55,
                "V" => 0x56,
                "W" => 0x57,
                "X" => 0x58,
                "Y" => 0x59,
                "Z" => 0x5A,
                _ => 0
            };
        }
    }
}
