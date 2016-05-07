using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;

namespace VeSoundboard
{
    public static class SoundboardAudio
    {
        static WaveOutEvent primaryOutput;
        static WaveOutEvent secondaryOutput;
        static int primaryDevice;
        static int secondaryDevice;

        public delegate void OnPlaybackStarted();
        public static OnPlaybackStarted PlaybackStarted;
        public delegate void OnPlaybackStopped();
        public static OnPlaybackStopped PlaybackStopped;

        public static void InitDevices(int primaryDevice, int secondaryDevice)
        {
            if (primaryOutput != null)
            {
                primaryOutput.Stop();
                primaryOutput.Dispose();
            }
            primaryOutput = new WaveOutEvent();
            SoundboardAudio.primaryDevice = primaryDevice;
            primaryOutput.DeviceNumber = primaryDevice;
            primaryOutput.PlaybackStopped += TriggerStopEvent;

            if (secondaryOutput != null)
            {
                secondaryOutput.Stop();
                secondaryOutput.Dispose();
            }
            secondaryOutput = new WaveOutEvent();
            secondaryOutput.DeviceNumber = secondaryDevice;
            SoundboardAudio.secondaryDevice = secondaryDevice;
        }

        public static void PlayAudio(string filename)
        {
            if (primaryOutput != null)
            {
                primaryOutput.Stop();
                primaryOutput.Dispose();
            }
            primaryOutput = new WaveOutEvent();
            primaryOutput.DeviceNumber = primaryDevice;
            primaryOutput.PlaybackStopped += TriggerStopEvent;
            AudioFileReader reader1 = new AudioFileReader(filename);
            primaryOutput.Init(reader1);
            if (primaryDevice != secondaryDevice)
            {
                if (secondaryOutput != null)
                {
                    secondaryOutput.Stop();
                    secondaryOutput.Dispose();
                }
                secondaryOutput = new WaveOutEvent();
                secondaryOutput.DeviceNumber = secondaryDevice;
                AudioFileReader reader2 = new AudioFileReader(filename);
                secondaryOutput.Init(reader2);
                secondaryOutput.Play();
            }

            if(primaryOutput.PlaybackState != PlaybackState.Playing)
            {
                if (PlaybackStarted != null)
                    PlaybackStarted.Invoke();
            }

            primaryOutput.Play();
        }

        public static void StopAllAudio()
        {
            if (primaryOutput != null) primaryOutput.Stop();
            if (secondaryOutput != null) secondaryOutput.Stop();
        }

        private static void TriggerStopEvent(object sender, StoppedEventArgs args)
        {
            if (PlaybackStopped != null) PlaybackStopped.Invoke();
        }
    }
}
