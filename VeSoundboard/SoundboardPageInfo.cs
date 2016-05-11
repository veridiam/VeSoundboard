using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeSoundboard
{
    [Serializable]
    public class SoundboardPageInfo
    {
        public string name;
        public List<SoundboardItem> items;

        public SoundboardPageInfo()
        {
            name = "New Page";
            items = new List<SoundboardItem>();
        }
    }
}
