using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using VoicevoxBridge;

public class AudioClipUtilTest
{
    class CreateFromStreamAsync
    {
        [Test]
        [TestCase("testing_audio_16bit_mono_24kHz.wav", 24000)]
        [TestCase("testing_audio_16bit_mono_44.1kHz.wav", 44100)]
        public async Task WhenLoad16bitMonoWAVData_CreateAndReturnsAudioClip(string audioFileName, int frequency)
        {
            string testingAudioPath = $"Assets/Tests/AudioClipUtil/wav/{audioFileName}";
            long fileSize = new FileInfo(testingAudioPath).Length;

            using (var fileStream = new FileStream(testingAudioPath, FileMode.Open, FileAccess.Read))
            {
                var clip = await AudioClipUtil.CreateFromStreamAsync(fileStream);
                Assert.That(clip.channels, Is.EqualTo(1));
                Assert.That(clip.frequency, Is.EqualTo(frequency));
                Assert.That(clip.samples, Is.EqualTo((fileSize - 44) / 2)); // (filesize - chank1 size) / 2 bytes[16 bit])

                float[] last = new float[256]; 
                clip.GetData(last, clip.samples - 256);
                Assert.That(last, Has.Some.Not.EqualTo(0));
            }
        }
    }
}
