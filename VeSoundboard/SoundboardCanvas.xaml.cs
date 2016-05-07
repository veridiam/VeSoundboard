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
using System.IO;

namespace VeSoundboard
{
    /// <summary>
    /// Interaction logic for SoundboardCanvas.xaml
    /// </summary>
    public partial class SoundboardCanvas : UserControl
    {
        public SoundboardCanvas()
        {
            InitializeComponent();
        }

        private void BaseCanvas_DragEnter(object sender, DragEventArgs e)
        {
            string filename;
            bool isValid = GetSoundPath(out filename, e);

            if (isValid)
            {
                e.Effects = DragDropEffects.Link;
            } else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void BaseCanvas_DragLeave(object sender, DragEventArgs e)
        {

        }

        private void BaseCanvas_Drop(object sender, DragEventArgs e)
        {

            string filename;
            bool isValid = GetSoundPath(out filename, e);
            if (!isValid) return;

            Console.WriteLine("Added " + filename);

            SoundboardNode node = new SoundboardNode(filename);
            Point location = e.GetPosition(BaseCanvas);
            Canvas.SetLeft(node, location.X);
            Canvas.SetTop(node, location.Y);
            BaseCanvas.Children.Add(node);

            if (dragInstructionLabel.Visibility == Visibility.Visible)
                dragInstructionLabel.Visibility = Visibility.Hidden;
        }

        private bool GetSoundPath(out string filename, DragEventArgs e)
        {
            bool valid = false;
            filename = "";
            string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (FileList.Length > 0)
            {
                filename = FileList[0];
                string ext = Path.GetExtension(filename).ToLower();
                if (ext == ".wav" || ext == ".mp3")
                    valid = true;
            }
            return valid;
        }
    }
}
