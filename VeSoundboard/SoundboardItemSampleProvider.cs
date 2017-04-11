using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace VeSoundboard
{
  class SoundboardItemSampleProvider : ISampleProvider
  {
    private readonly SoundboardItem item;
    private long position;
    public WaveFormat WaveFormat { get { return item.waveFormat; } }

    public SoundboardItemSampleProvider(SoundboardItem item)
    {
      this.item = item;
    }

    public int Read(float[] buffer, int offset, int count)
    {
      long availableSamples = item.audioData.Length - position;
      long samplesToCopy = Math.Min(availableSamples, count);
      Array.Copy(item.audioData, position, buffer, offset, samplesToCopy);
      position += samplesToCopy;

      return (int)samplesToCopy;
    }
  }
}
