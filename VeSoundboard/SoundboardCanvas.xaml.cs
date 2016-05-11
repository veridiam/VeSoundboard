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
        public SoundboardPageInfo pageInfo;
        private bool loaded = false;

        public SoundboardCanvas(SoundboardPageInfo info)
        {
            InitializeComponent();
            pageInfo = info;
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
            try
            {
                string filename;
                bool isValid = GetSoundPath(out filename, e);
                if (!isValid) throw new Exception("Invalid file type.");

                Console.WriteLine("Added " + filename);

                SoundboardItem item = new SoundboardItem(filename, e.GetPosition(BaseCanvas));
                pageInfo.items.Add(item);
                SoundboardNode node = new SoundboardNode(item, pageInfo);
                BaseCanvas.Children.Add(node);
                node.SetPosition();

                MainWindow.SaveData();

                if (dragInstructionLabel.Visibility == Visibility.Visible)
                    dragInstructionLabel.Visibility = Visibility.Hidden;
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private bool GetSoundPath(out string filename, DragEventArgs e)
        {
            bool valid = false;
            filename = "";
            string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (FileList.Length > 0)
            {
                filename = FileList[0];
                // TODO: Use relative paths
                //Uri u = new Uri(@filename);
                //u = u.MakeRelativeUri(new Uri(@Directory.GetCurrentDirectory()));
                //filename = u.ToString();

                string ext = Path.GetExtension(filename).ToLower();
                if (ext == ".wav" || ext == ".mp3")
                    valid = true;
            }
            return valid;
        }

        public void LoadNodes()
        {
            if (loaded) return;

            if (pageInfo.items.Count > 0) dragInstructionLabel.Visibility = Visibility.Hidden;
            foreach(SoundboardItem i in pageInfo.items)
            {
                Console.WriteLine("Adding node " + i.filename);
                SoundboardNode node = new SoundboardNode(i, pageInfo);
                BaseCanvas.Children.Add(node);
                node.SetPosition();
            }
        }

        public void Destroy()
        {
            for(int i = 0; i < BaseCanvas.Children.Count; i++)
                {
                    try
                    {
                        SoundboardNode n = (SoundboardNode)BaseCanvas.Children[i];
                        n.Destroy();
                    }
                    catch (Exception ex)
                    {
                        // Do nothing
                    }
                }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
            LoadNodes();
            loaded = true;
        }
    }
}
