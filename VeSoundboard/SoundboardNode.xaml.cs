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
using System.Windows.Controls.Primitives;

namespace VeSoundboard
{
    /// <summary>
    /// Interaction logic for SounboardNode.xaml
    /// </summary>
    public partial class SoundboardNode : UserControl
    {
        protected string filepath;

        public SoundboardNode()
        {
            InitializeComponent();
            KeybindBox.globalKeybind = true;    
        }

        public SoundboardNode(string filepath)
        {
            InitializeComponent();

            this.filepath = filepath;
            ButtonText.Text = System.IO.Path.GetFileNameWithoutExtension(filepath);
            KeybindBox.globalKeybind = true;
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            TopControls.OpacityMask = null;
            BottomControls.OpacityMask = null;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            TopControls.OpacityMask = Brushes.Transparent;
            if (KeybindBox.hotkey.key == Keys.None && !KeybindBox.isSetting)
                BottomControls.OpacityMask = Brushes.Transparent;
        }

        protected void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            double left = Canvas.GetLeft(this) + e.HorizontalChange;
            double top = Canvas.GetTop(this) + e.VerticalChange;

            Canvas.SetLeft(this, left);
            Canvas.SetTop(this, top);
        }

        private void DeleteButton(object sender, RoutedEventArgs e)
        {
            KeybindBox.SetHotkey(new Hotkey());
            KeybindBox.UnregisterGlobalHotkey();
            if (Parent == null) return;
            Panel p = (Panel)Parent;

            p.Children.Remove(this);
        }

        private void SoundButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (filepath != null)
            {
                SoundboardAudio.PlayAudio(filepath);
            }
        }

        private void KeybindBox_GlobalKeybindPressed()
        {
            if (filepath != null)
            {
                SoundboardAudio.PlayAudio(filepath);
            }
        }
    }
}
