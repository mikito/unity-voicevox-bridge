using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Threading;

namespace VoicevoxBridge.Example
{
    public class ExampleBuffering : MonoBehaviour
    {
        [SerializeField] VOICEVOX voicevox;
        [SerializeField] TextAsset textAsset;
        [SerializeField] Text textView;
        [SerializeField] int speaker = 1;

        Queue<Voice> voiceQueue = new Queue<Voice>();
        Voice playingVoice = null;
        CancellationTokenSource cts = new CancellationTokenSource();

        void Start()
        {
            var sentences = textAsset.text.Replace("。", "。\n").Split('\n', System.StringSplitOptions.RemoveEmptyEntries).ToList();
            textView.text = textAsset.text;

            // Buffering voice synthesis 
            CreateVoices(sentences);
        }

        void OnDestroy()
        {
            while (voiceQueue.Count > 0)
            {
                voiceQueue.Dequeue().Dispose();
            }
            cts.Cancel();
            cts.Dispose();
        }

        void Update()
        {
            // Dispatch Queue
            if (voiceQueue.Count > 0 && playingVoice == null) PlayVoiceQueue();
        }

        void HighlightText(string text, string color)
        {
            textView.text = textAsset.text.Replace(playingVoice.Text, $"<color={color}>{playingVoice.Text}</color>");
        }

        async void PlayVoiceQueue()
        {
            try
            {
                playingVoice = voiceQueue.Dequeue();
                HighlightText(playingVoice.Text, "blue");
                await voicevox.Play(playingVoice, cts.Token);
                textView.text = textAsset.text;
                playingVoice = null;
            }
            catch (OperationCanceledException)
            {
                Debug.Log("voice playing canceled");
            }
        }

        async void CreateVoices(List<string> sentences)
        {
            try
            {
                foreach (var t in sentences)
                {
                    var voice = await voicevox.CreateVoice(speaker, t, cts.Token);
                    voiceQueue.Enqueue(voice);
                }

                Debug.Log("voice creating completed");
            }
            catch (OperationCanceledException)
            {
                Debug.Log("voice creating canceled");
            }
        }
    }
}