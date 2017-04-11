using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace VeSoundboard
{
  [Serializable]
  public class SoundboardItem
  {
    public string text;
    public Hotkey hotkey;
    public Point location;

    public float[] audioData { get; protected set; }
    public WaveFormat waveFormat
    {
      get { return new WaveFormat(sampleRate, channels); }
      protected set { sampleRate = value.SampleRate; channels = value.Channels; }
    }
    private int sampleRate;
    private int channels;

    public SoundboardItem(Point location)
    {
      hotkey = new Hotkey();
      this.location = location;

    }

    public bool LoadSound(string filename)
    {
      try
      {
        using (AudioFileReader reader = new AudioFileReader(filename))
        {
          waveFormat = reader.WaveFormat;
          List<float> wholeFile = new List<float>((int)(reader.Length / 4));
          float[] buffer = new float[reader.WaveFormat.SampleRate * waveFormat.Channels];
          int samplesRead;
          while ((samplesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
          {
            wholeFile.AddRange(buffer.Take(samplesRead));
          }
          audioData = wholeFile.ToArray();
        }
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message);
        return false;
      }
      return true;
    }
  }
}
