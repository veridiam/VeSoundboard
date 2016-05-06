using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;

namespace VeSoundboard
{
    public static class SoundboardAudio
    {
        static DirectSoundOut primaryOutput;
        static DirectSoundOut secondaryOutput;
        static Guid primaryDeviceGuid;
        static Guid secondaryDeviceGuid;

        public delegate void OnPlaybackStarted();
        public static OnPlaybackStarted PlaybackStarted;
        public delegate void OnPlaybackStopped();
        public static OnPlaybackStopped PlaybackStopped;

        public static void InitDevices(Guid primaryGuid, Guid secondaryGuid)
        {
            if (primaryOutput != null)
            {
                primaryOutput.Stop();
                primaryOutput.Dispose();
            }
            primaryOutput = new DirectSoundOut(primaryGuid, 80);
            primaryOutput.PlaybackStopped += TriggerStopEvent;
            primaryDeviceGuid = primaryGuid;

            if (secondaryOutput != null)
            {
                secondaryOutput.Stop();
                secondaryOutput.Dispose();
            }
            secondaryOutput = new DirectSoundOut(secondaryGuid, 80);
            secondaryDeviceGuid = secondaryGuid;
        }

        public static void PlayAudio(string filename)
        {
            if (primaryOutput != null)
            {
                primaryOutput.Stop();
                primaryOutput.Dispose();
            }
            primaryOutput = new DirectSoundOut(primaryDeviceGuid, 80);
            AudioFileReader reader1 = new AudioFileReader(filename);
            primaryOutput.Init(reader1);
            if (secondaryDeviceGuid != primaryDeviceGuid)
            {
                if (secondaryOutput != null)
                {
                    secondaryOutput.Stop();
                    secondaryOutput.Dispose();
                }
                secondaryOutput = new DirectSoundOut(secondaryDeviceGuid, 80);
                AudioFileReader reader2 = new AudioFileReader(filename);
                secondaryOutput.Init(reader2);
                secondaryOutput.Play();
            }

            if(primaryOutput.PlaybackState != PlaybackState.Playing)
            {
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
