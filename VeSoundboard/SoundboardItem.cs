using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeSoundboard
{
    [Serializable]
    public class SoundboardItem
    {
        public string filename;
        public Hotkey hotkey;
        public Point location;

        public SoundboardItem(string filename, Point location)
        {
            this.filename = filename;
            hotkey = new Hotkey();
            this.location = location;
        }
    }
}
