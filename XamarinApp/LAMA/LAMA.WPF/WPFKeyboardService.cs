using LAMA.Services;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Windows.Input;

[assembly: Dependency(typeof(LAMA.WPF.WPFKeyboardService))]
namespace LAMA.WPF
{
    internal class WPFKeyboardService : IKeyboardService
    {
        public event KeyDown OnKeyDown;
        public event KeyUp OnKeyUp;

        HashSet<Key> keys;

        public WPFKeyboardService()
        {
            keys = new HashSet<Key>();
            App.Current.MainWindow.KeyDown += (object sender, KeyEventArgs e) =>
            {
                if (keys.Contains(e.Key)) return;

                OnKeyDown(e.Key.ToString());
                keys.Add(e.Key);
            };

            App.Current.MainWindow.KeyUp += (object sender, KeyEventArgs e) =>
            {
                if (!keys.Contains(e.Key))
                    return;

                OnKeyUp(e.Key.ToString());
                keys.Remove(e.Key);
            };
        }

        public bool IsKeyPressed(string key)
        {
            return Enum.TryParse(key, out Key eKey) && keys.Contains(eKey);
        }
    }
}
