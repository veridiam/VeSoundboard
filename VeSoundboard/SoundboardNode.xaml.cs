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
        protected SoundboardItem item;
        protected SoundboardPageInfo pageInfo;
        private bool loaded = false;

        public SoundboardNode(SoundboardItem item, SoundboardPageInfo info)
        {
            InitializeComponent();
            this.item = item;
            ButtonText.Text = item.text;
            KeybindBox.globalKeybind = true;
            pageInfo = info;
        }

        public void SetPosition()
        {
            Canvas.SetLeft(this, item.location.X);
            Canvas.SetTop(this, item.location.Y);
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

            left = Math.Round(left / 5.0d) * 5.0d;
            top = Math.Round(top / 5.0d) * 5.0d;

            Canvas.SetLeft(this, left);
            Canvas.SetTop(this, top);
        }

        private void DeleteButton(object sender, RoutedEventArgs e)
        {
            Destroy();
            MainWindow.SaveData();
        }

        private void SoundButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (item != null)
            {
                SoundboardAudio.instance.PlayAudio(item);
            }
        }

        private void KeybindBox_GlobalKeybindPressed()
        {
            if (item != null)
            {
                SoundboardAudio.instance.PlayAudio(item);
            }
        }

        private void Node_Loaded(object sender, RoutedEventArgs e)
        {
            if (loaded) return;

            loaded = true;
            KeybindBox.SetHotkey(item.hotkey);

            TopControls.OpacityMask = Brushes.Transparent;
            if (KeybindBox.hotkey.key == Keys.None && !KeybindBox.isSetting)
                BottomControls.OpacityMask = Brushes.Transparent;
        }

        private void KeybindBox_KeybindSet()
        {
            item.hotkey = KeybindBox.hotkey;
        }

        private void MoveHandle_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            item.location.X = Canvas.GetLeft(this);
            item.location.Y = Canvas.GetTop(this);

            MainWindow.SaveData();
        }

        public void Destroy()
        {
            KeybindBox.SetHotkey(new Hotkey());
            KeybindBox.UnregisterGlobalHotkey();
            if (Parent == null) return;
            Panel p = (Panel)Parent;

            p.Children.Remove(this);
            pageInfo.items.Remove(item);
        }
    }
}
