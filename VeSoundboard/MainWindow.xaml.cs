using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using NAudio.Wave;
using WindowsInput;
using WindowsInput.Native;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace VeSoundboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private InputSimulator inputSimulator;
        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);

        protected const string dataFilename = "soundboard.dat_storm";
        
        private static List<SoundboardPageInfo> savedData;

        public MainWindow()
        {
            InitializeComponent();
            inputSimulator = new InputSimulator();
        }

        public void LoadData()
        {
            try
            {
                using (Stream stream = File.Open(dataFilename, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    savedData = (List<SoundboardPageInfo>)formatter.Deserialize(stream);
                    stream.Close();
                    Console.WriteLine("Loading successful.");
                }
            } catch (FileNotFoundException e)
            {
                Console.WriteLine("File not found, creating new data.");
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (savedData == null) savedData = new List<SoundboardPageInfo>();

            if (savedData.Count < 1)
            {
                savedData.Add(new SoundboardPageInfo());
            }

            foreach (SoundboardPageInfo info in savedData)
            {
                TabItem t = new TabItem();
                t.Header = info.name;
                tabControl.Items.Add(t);
                SoundboardCanvas canvas = new SoundboardCanvas(info);
                t.Content = canvas;
            }

            tabControl.SelectedIndex = 0;
            SetupPageNameTextBox();
        }

        private void SetupPageNameTextBox()
        {
            TabItem t = (TabItem)tabControl.SelectedItem;

            if (t != null)
            {
                pageNameTextBox.Text = (string)t.Header;
            }
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set up audio device combo boxes
                List<WaveOutCapabilities> devices = new List<WaveOutCapabilities>();
                for (int i = 0; i < WaveOut.DeviceCount; i++)
                    devices.Add(WaveOut.GetCapabilities(i));
                primaryDeviceCombo.ItemsSource = devices;
                primaryDeviceCombo.DisplayMemberPath = "ProductName";
                primaryDeviceCombo.SelectedIndex = Properties.Settings.Default.PrimaryDevice;
                secondaryDeviceCombo.ItemsSource = devices;
                secondaryDeviceCombo.DisplayMemberPath = "ProductName";
                secondaryDeviceCombo.SelectedIndex = Properties.Settings.Default.SecondaryDevice;
                ChangeAudioDevices();
            
                // Set up window focus combo box
                windowCombo.ItemsSource = GetWindowProcesses();
                windowCombo.DisplayMemberPath = "MainWindowTitle";
           
                // Load saved keybinds
                pttKeybindBox.SetHotkey(Properties.Settings.Default.PTTHotkey);
                StopKeybindBox.globalKeybind = true;
                StopKeybindBox.SetHotkey(Properties.Settings.Default.StopHotkey);

                // Setup callbacks
                SoundboardAudio.PlaybackStarted += StartPushToTalk;
                SoundboardAudio.PlaybackStopped += StopPushToTalk;

                // Load saved pages
                LoadData();

            } catch (Exception ex)
            {
                Console.WriteLine("Error loading.");
                Console.WriteLine(ex.Message);
            }
        }

        private void ChangeAudioDevices()
        {
            SoundboardAudio.InitDevices(primaryDeviceCombo.SelectedIndex, secondaryDeviceCombo.SelectedIndex);
        }

        private void DeviceComboChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.PrimaryDevice = primaryDeviceCombo.SelectedIndex;
            Properties.Settings.Default.SecondaryDevice = secondaryDeviceCombo.SelectedIndex;
            Properties.Settings.Default.Save();

            Console.WriteLine("Primary device index: " + Properties.Settings.Default.PrimaryDevice.ToString());
            Console.WriteLine("Secondary device index: " + Properties.Settings.Default.SecondaryDevice.ToString());

            ChangeAudioDevices();
        }

        private void StopSoundButton_Click(object sender, RoutedEventArgs e)
        {
            SoundboardAudio.StopAllAudio();
        }

        private void StopKeybindBox_GlobalKeybindPressed()
        {
            SoundboardAudio.StopAllAudio();
        }

        private void StartPushToTalk()
        {
            if (!PTTCheckBox.IsChecked.Value || !pttKeybindBox.GetIsBound() ) return;
            Console.WriteLine("Starting PTT.");
            
            // Set window focus
            if (PTTFocusCheckBox.IsChecked.Value && windowCombo.SelectedValue != null)
            {
                Process p = (Process)windowCombo.SelectedValue;
                if (p != null)
                {
                    Console.WriteLine("Setting focus to window: " + p.MainWindowTitle);
                    SetForegroundWindow(p.MainWindowHandle);
                }
            }

            // Send key-down
            inputSimulator.Keyboard.KeyDown((VirtualKeyCode)(int)pttKeybindBox.hotkey.key);

        }

        private void StopPushToTalk()
        {
            if (!PTTCheckBox.IsChecked.Value || !pttKeybindBox.GetIsBound()) return;

            Console.WriteLine("Stopping PTT.");
            
            // Send key-up
            inputSimulator.Keyboard.KeyUp((VirtualKeyCode)(int)pttKeybindBox.hotkey.key);
        }

        private List<Process> GetWindowProcesses()
        {
            List<Process> processes = new List<Process>(Process.GetProcesses());

            processes.RemoveAll((Process p) =>
           {
               return (p.MainWindowTitle == null || p.MainWindowTitle == "");
           });

            return processes;
        }

        private void windowCombo_DropDownOpened(object sender, EventArgs e)
        {
            windowCombo.ItemsSource = GetWindowProcesses();
        }

        private void pttKeybindBox_KeybindSet()
        {
            Properties.Settings.Default.PTTHotkey = pttKeybindBox.hotkey;
            Properties.Settings.Default.Save();
        }

        private void StopKeybindBox_KeybindSet()
        {
            Properties.Settings.Default.StopHotkey = StopKeybindBox.hotkey;
            Properties.Settings.Default.Save();
        }

        public static void SaveData()
        {
            Console.WriteLine("Saving data.");
            try
            {
                using (Stream stream = File.Open(dataFilename, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, savedData);
                    stream.Close();
                }
            } catch (Exception ex)
            {
                Console.WriteLine("Problem writing saved data.");
                Console.WriteLine(ex.Message);
            }
        }

        private void MainWindowElement_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveData();
        }

        private void TabCreateClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Console.WriteLine("Tab area clicked.");
        }

        private void newPageButton_Click(object sender, RoutedEventArgs e)
        {
            SoundboardPageInfo info = new SoundboardPageInfo();
            savedData.Add(info);

            SoundboardCanvas canvas = new SoundboardCanvas(info);
            TabItem t = new TabItem();
            t.Content = canvas;
            t.Header = info.name;
            tabControl.Items.Add(t);
            tabControl.SelectedItem = t;

            SaveData();
        }

        private void deletePageButton_Click(object sender, RoutedEventArgs e)
        {
            TabItem t = (TabItem)tabControl.SelectedItem;

            if (t != null)
            {
                if (tabControl.Items.Count > 1)
                {
                    tabControl.SelectedIndex--;
                }
                
                SoundboardCanvas canvas = (SoundboardCanvas)t.Content;
                if (canvas != null)
                {
                    canvas.Destroy();
                    savedData.Remove(canvas.pageInfo);
                }

                tabControl.Items.Remove(t);

                if (tabControl.Items.Count == 0)
                {
                    newPageButton_Click(sender, e);
                }

                SaveData();
            }
        }

        private void SetPageName()
        {
            TabItem t = (TabItem)tabControl.SelectedItem;

            if (t != null)
            {
                SoundboardCanvas canvas = (SoundboardCanvas)t.Content;
                if (canvas != null)
                {
                    canvas.pageInfo.name = pageNameTextBox.Text;

                    t.Header = canvas.pageInfo.name;
                    SaveData();
                }
            }
        }

        private void pageNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SetPageName();
        }

        private void pageNameTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SetPageName();
            }
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetupPageNameTextBox();
        }
    }
}
