using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace VeSoundboard
{
    /// <summary>
    /// Interaction logic for KeyBinderBox.xaml
    /// </summary>
    public partial class KeyBinderBox : UserControl
    {
        public bool globalKeybind = false;
        public ModifierKeys modifiers;
        public Keys key;
        
        public bool isSetting { get; private set; } = false;

        // Events
        public delegate void OnGlobalKeybindPressed();
        public event OnGlobalKeybindPressed GlobalKeybindPressed;
        public delegate void OnKeybindSet();
        public event OnKeybindSet KeybindSet;

        // Global Hotkey
        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        private HwndSource source;
        private int HOTKEY_ID;

        public KeyBinderBox()
        {
            InitializeComponent();
        }

        public bool GetIsBound()
        {
            return (key != Keys.None);
        }

        private void baseTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SetKeysetMode(true);
        }

        private void baseTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SetKeysetMode(false);
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (isSetting)
            {
                switch(e.Key)
                {
                    case Key.RightShift:
                    case Key.LeftShift:
                    case Key.LeftCtrl:
                    case Key.RightCtrl:
                    case Key.LeftAlt:
                    case Key.RightAlt:
                        return;
                    case Key.LWin:
                    case Key.RWin:
                        SetKeysetMode(false);
                        break;
                    case Key.Back:
                    case Key.Escape:
                        SetHotkey(ModifierKeys.None, Keys.None);
                        break;
                    default:
                        SetHotkey(Keyboard.Modifiers, (Keys)KeyInterop.VirtualKeyFromKey(e.Key));
                        break;
                }
            }
        }

        public void SetHotkey(ModifierKeys mods, Keys key)
        {
            // Set keybinding
            this.modifiers = mods;
            this.key = key;

            Console.WriteLine("Set keybind to " + modifiers.ToString() + " " + key.ToString());
            
            string keyString = "<";
            if (key == Keys.None)
            {
                if (globalKeybind)
                    UnregisterGlobalHotkey();
                keyString += "No Key";
            }
            else
            {
                if (globalKeybind)
                    RegisterGlobalHotkey();
                if (modifiers != ModifierKeys.None)
                    keyString += modifiers.ToString() + " ";

                keyString += key.ToString();
            }
            keyString += ">";
            baseTextBox.Text = keyString;

            SetKeysetMode(false);

            // Call event
            if (KeybindSet != null)
                KeybindSet.Invoke();
        }

        protected void SetKeysetMode(bool enabled)
        {
            if (enabled)
            {
                isSetting = true;
                baseTextBox.Background = SystemColors.InfoBrush;
                baseTextBox.Select(0, 0);
            } else
            {
                isSetting = false;
                baseTextBox.Background = Brushes.White;
            }
        }

        private void RegisterGlobalHotkey()
        {
            UnregisterGlobalHotkey();
            var helper = new WindowInteropHelper(Window.GetWindow(this));
            RegisterHotKey(helper.Handle, HOTKEY_ID, (uint)modifiers, (uint)key);
        }

        public void UnregisterGlobalHotkey()
        {
            var helper = new WindowInteropHelper(Window.GetWindow(this));
            UnregisterHotKey(helper.Handle, HOTKEY_ID);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    if (wParam.ToInt32() == HOTKEY_ID)
                    {
                        if (GlobalKeybindPressed != null)
                        {
                            GlobalKeybindPressed.Invoke();
                        }
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void baseKeyBinderBox_Loaded(object sender, RoutedEventArgs e)
        {
            HOTKEY_ID = GetHashCode();
            var helper = new WindowInteropHelper(Window.GetWindow(this));
            source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(HwndHook);
        }
    }
}
