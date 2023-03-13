using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;

namespace VoicevoxBridge
{
    public class AudioClipUtil
    {
        public static int BufferSize = 4096;

        public static async Task<AudioClip> CreateFromStreamAsync(Stream stream)
        {
            byte[] ftmChank = new byte[44];

            await stream.ReadAsync(ftmChank, 0, 44);
            int channels = BitConverter.ToInt16(ftmChank, 22);
            int bitPerSample = BitConverter.ToInt16(ftmChank, 34);

            if (channels != 1) throw new NotSupportedException("AudioClipUtil supports only single channel.");
            if (bitPerSample != 16) throw new NotSupportedException("AudioClipUtil supports only 16-bit quantization.");

            int bytePerSample = bitPerSample / 8;
            int frequency = BitConverter.ToInt32(ftmChank, 24);
            int length = BitConverter.ToInt32(ftmChank, 40);

            AudioClip audioClip = null;
            try
            {
                audioClip = AudioClip.Create("AudioClip", length / 2, channels, frequency, false);

                byte[] readBuffer = new byte[BufferSize];
                float[] samplesBuffer = new float[BufferSize / bytePerSample];
                int offset = 0;
                int read;

                while ((read = await stream.ReadAsync(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    // Supports only 16-bit quantization, and single channel.
                    for (int i = 0; i < read / bytePerSample; i++)
                    {
                        short value = BitConverter.ToInt16(readBuffer, i * bytePerSample);
                        samplesBuffer[i] = value / 32768f;
                    }

                    if (read == BufferSize)
                    {
                        audioClip.SetData(samplesBuffer, offset);
                    }
                    else
                    {
                        var lastSamples = new float[read / bytePerSample];
                        Array.Copy(samplesBuffer, lastSamples, read / bytePerSample);
                        audioClip.SetData(lastSamples, offset);
                    }
                    offset += read / 2;
                }
            }
            catch (Exception e)
            {
                if (audioClip != null) UnityEngine.Object.Destroy(audioClip);
                throw new IOException("AudioClipUtil: WAV data decode failed.", e);
            }

            return audioClip;
        }
    }
}