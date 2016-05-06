using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
namespace VeSoundboard
{
    [Serializable]
    public class Hotkey
    {
        public ModifierKeys modifiers;
        public Keys key;

        public Hotkey()
        {
            modifiers = ModifierKeys.None;
            key = Keys.None;
        }

        public Hotkey(ModifierKeys mods, Keys k)
        {
            modifiers = mods;
            key = k;
        }
    }
}
