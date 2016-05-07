using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudio.Wave;
using WindowsInput;
using WindowsInput.Native;
using System.Diagnostics;
using System.Runtime.InteropServices;

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

        public MainWindow()
        {
            InitializeComponent();
            inputSimulator = new InputSimulator();
            
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
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

            windowCombo.ItemsSource = GetWindowProcesses();
            windowCombo.DisplayMemberPath = "MainWindowTitle";

            pttKeybindBox.SetHotkey(Properties.Settings.Default.PTTHotkey);
            StopKeybindBox.globalKeybind = true;
            StopKeybindBox.SetHotkey(Properties.Settings.Default.StopHotkey);

            SoundboardAudio.PlaybackStarted += StartPushToTalk;
            SoundboardAudio.PlaybackStopped += StopPushToTalk;
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
    }
}
