using System.Collections;
using UnityEngine;
using System;

namespace VoicevoxBridge
{
    public class AudioClipUtil
    {
        public static AudioClip CreateFromBytes(byte[] data)
        {
            int channels = data[22];
            int frequency = BitConverter.ToInt32(data, 24);
            int length = data.Length - 44;
            float[] samples = new float[length / 2];

            for (int i = 0; i < length / 2; i++)
            {
                short value = BitConverter.ToInt16(data, i * 2 + 44);
                samples[i] = value / 32768f;
            }

            AudioClip audioClip = AudioClip.Create("AudioClip", samples.Length, channels, frequency, false);
            audioClip.SetData(samples, 0);

            return audioClip;
        }
    }
}