using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Threading;

namespace VeSoundboard
{
  public class SoundboardAudio : IDisposable
  {
    private readonly WaveOutEvent primaryOutput;
    private readonly WaveOutEvent secondaryOutput;

    private MixingSampleProvider mixer_one;
    private MixingSampleProvider mixer_two;

    public static readonly SoundboardAudio instance = new SoundboardAudio();

    public delegate void OnPlaybackStarted();
    public OnPlaybackStarted PlaybackStarted;
    public delegate void OnPlaybackStopped();
    public OnPlaybackStopped PlaybackStopped;

    public SoundboardAudio()
    {
      primaryOutput = new WaveOutEvent();
      secondaryOutput = new WaveOutEvent();

      primaryOutput.PlaybackStopped += (object sender, StoppedEventArgs args) =>
      {
        if (PlaybackStopped != null)
          PlaybackStopped.Invoke();
      };

      mixer_one = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
      //mixer_one.ReadFully = true;

      mixer_two = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
      //mixer_two.ReadFully = true;

    }

    public void InitDevices(int primaryDevice, int secondaryDevice)
    {
      if (primaryOutput != null)
      {
        primaryOutput.Stop();
        primaryOutput.DeviceNumber = primaryDevice;
        primaryOutput.Init(mixer_one);
        //primaryOutput.Play();
      }

      if (secondaryOutput != null)
      {
        secondaryOutput.Stop();
        if (primaryDevice != secondaryDevice)
        {
          secondaryOutput.DeviceNumber = secondaryDevice;
          secondaryOutput.Init(mixer_two);
          //secondaryOutput.Play();
        }
      }

    }

    public void PlayAudio(SoundboardItem item)
    {
      ISampleProvider itemSampleProvider_one = new SoundboardItemSampleProvider(item);
      ISampleProvider itemSampleProvider_two = new SoundboardItemSampleProvider(item);

      StopAllAudio();
      if (mixer_one.WaveFormat.Channels != item.waveFormat.Channels
        || mixer_one.WaveFormat.SampleRate != item.waveFormat.SampleRate
        )
      {
        mixer_one = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(item.waveFormat.SampleRate, item.waveFormat.Channels));
        primaryOutput.Init(mixer_one);
      }
      if (mixer_two.WaveFormat.Channels != item.waveFormat.Channels
        || mixer_two.WaveFormat.SampleRate != item.waveFormat.SampleRate
        )
      {
        mixer_two = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(item.waveFormat.SampleRate, item.waveFormat.Channels));
        secondaryOutput.Init(mixer_two);
      }

      mixer_one.AddMixerInput(itemSampleProvider_one);
      mixer_two.AddMixerInput(itemSampleProvider_two);

      if (PlaybackStarted != null)
        PlaybackStarted.Invoke();

      if (primaryOutput.PlaybackState != PlaybackState.Playing)
        primaryOutput.Play();

      if (secondaryOutput.PlaybackState != PlaybackState.Playing)
        secondaryOutput.Play();
    }

    public void StopAllAudio()
    {
      mixer_one.RemoveAllMixerInputs();
      mixer_two.RemoveAllMixerInputs();
    }

    public void Dispose()
    {
      primaryOutput.Dispose();
      secondaryOutput.Dispose();
    }
  }
}
