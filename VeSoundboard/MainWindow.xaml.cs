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
using System.Threading;

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

    protected const string FILE_EXTENSION = ".ve";

    private static string currentSaveDataLocation;
    private static List<SoundboardPageInfo> loadedData;

    private int pushedKey;

    public MainWindow()
    {
      string[] args = Environment.GetCommandLineArgs();
      Console.WriteLine("Args: " + args);
      if (args.Length > 1)
      {
        if (LoadData(args[1]))
        {
          currentSaveDataLocation = args[1];
        }
      }

      // Creates a default soundboard
      if (loadedData == null)
      {
        if (LoadData("my-soundboard" + FILE_EXTENSION))
        {
          currentSaveDataLocation = "my-soundboard" + FILE_EXTENSION;
        }
        else
        {
          Console.WriteLine("Creating default soundboard");
          loadedData = new List<SoundboardPageInfo>();
          loadedData.Add(new SoundboardPageInfo());
          currentSaveDataLocation = "my-soundboard" + FILE_EXTENSION;
        }
      }

      InitializeComponent();
      Title = Path.GetFileName(currentSaveDataLocation);
      inputSimulator = new InputSimulator();
    }

    public bool LoadData(string location)
    {
      List<SoundboardPageInfo> newData;
      try
      {
        using (Stream stream = File.Open(location, FileMode.Open))
        {
          BinaryFormatter formatter = new BinaryFormatter();
          newData = (List<SoundboardPageInfo>)formatter.Deserialize(stream);
          stream.Close();
          Console.WriteLine("Loading successful.");
          loadedData = newData;
          return true;
        }
      }
      catch (FileNotFoundException e)
      {
        Console.WriteLine("File not found, creating new data.");
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message);
        Console.WriteLine(e.Message);

      }
      return false;

    }

    private void SaveAs()
    {
      Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
      dlg.DefaultExt = FILE_EXTENSION;
      dlg.Filter = "VeSoundboard Files (*" + FILE_EXTENSION + ")|*" + FILE_EXTENSION;

      bool? result = dlg.ShowDialog(this);

      if (result == true)
      {
        currentSaveDataLocation = dlg.FileName;
        Title = Path.GetFileName(currentSaveDataLocation);
        SaveData();
      }
    }

    private bool OpenSoundboard()
    {
      Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
      dlg.DefaultExt = FILE_EXTENSION;
      dlg.Filter = "VeSoundboard Files (*" + FILE_EXTENSION + ")|*" + FILE_EXTENSION;
      bool? result = dlg.ShowDialog(this);
      if (result == true)
      {
        if (LoadData(dlg.FileName))
        {
          currentSaveDataLocation = dlg.FileName;
          Title = Path.GetFileName(currentSaveDataLocation);
          UpdateSoundboardUI();
          return true;
        }
      }
      return false;
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
      UpdateSoundboardUI();


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

      SoundboardAudio.instance.PlaybackStarted += StartPushToTalk;
      SoundboardAudio.instance.PlaybackStopped += StopPushToTalk;
    }

    public void UpdateSoundboardUI()
    {
      tabControl.Items.Clear();
      foreach (SoundboardPageInfo info in loadedData)
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

    private void ChangeAudioDevices()
    {
      SoundboardAudio.instance.InitDevices(primaryDeviceCombo.SelectedIndex, secondaryDeviceCombo.SelectedIndex);
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
      SoundboardAudio.instance.StopAllAudio();
    }

    private void StopKeybindBox_GlobalKeybindPressed()
    {
      SoundboardAudio.instance.StopAllAudio();
    }

    private void StartPushToTalk()
    {
      if (!PTTCheckBox.IsChecked.Value || !pttKeybindBox.GetIsBound()) return;

      // Set window focus
      if (PTTFocusCheckBox.IsChecked.Value && windowCombo.SelectedValue != null)
      {
        Process p = (Process)windowCombo.SelectedValue;
        if (p != null)
        {
          SetForegroundWindow(p.MainWindowHandle);
        }
      }

      // Send key-down
      pushedKey = (int)pttKeybindBox.hotkey.key;
      inputSimulator.Keyboard.KeyDown((VirtualKeyCode)pushedKey);
    }

    private void StopPushToTalk()
    {
      if (!pttKeybindBox.GetIsBound()) return;

      // Send key-up
      inputSimulator.Keyboard.KeyUp((VirtualKeyCode)pushedKey);
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
        using (Stream stream = File.Open(currentSaveDataLocation, FileMode.Create))
        {
          BinaryFormatter formatter = new BinaryFormatter();
          formatter.Serialize(stream, loadedData);
          stream.Close();
        }
      }
      catch (Exception e)
      {
        Console.WriteLine("Problem writing saved data.");
        MessageBox.Show(e.Message);
      }
    }

    private void MainWindowElement_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      SaveData();
      SoundboardAudio.instance.Dispose();
    }

    private void TabCreateClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      Console.WriteLine("Tab area clicked.");
    }

    private void newPageButton_Click(object sender, RoutedEventArgs e)
    {
      SoundboardPageInfo info = new SoundboardPageInfo();
      loadedData.Add(info);

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
          loadedData.Remove(canvas.pageInfo);
        }

        tabControl.Items.Remove(t);

        if (tabControl.Items.Count == 0)
        {
          newPageButton_Click(sender, e);
        }

        SaveData();
      }
    }

    private void exit_Click(object sender, RoutedEventArgs e)
    {
      Close();
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

    private void newSoundboard_Click(object sender, RoutedEventArgs e)
    {
      loadedData = new List<SoundboardPageInfo>();
      loadedData.Add(new SoundboardPageInfo());
      SaveAs();
      UpdateSoundboardUI();
    }

    private void openSoundboard_Click(object sender, RoutedEventArgs e)
    {
      OpenSoundboard();
    }

    private void saveAs_Click(object sender, RoutedEventArgs e)
    {
      SaveAs();
    }
  }
}
