using System;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

namespace VoicevoxBridge.Example
{
    public class ExampleSimple : MonoBehaviour
    {
        [SerializeField] VOICEVOX voicevox;
        [SerializeField] Text textView;
        [SerializeField] InputField inputField;
        [SerializeField] Button button;
        [SerializeField] int speaker = 1;

        CancellationTokenSource cts = new CancellationTokenSource();

        void Awake()
        {
            button.onClick.AddListener(Speak);
        }

        async void Speak()
        {
            if(string.IsNullOrEmpty(inputField.text)) return;

            button.interactable = false;
            string text = inputField.text;
            inputField.text = "";
            textView.text = "Processing...";

            try
            {
                var voice = await voicevox.CreateVoice(speaker, text, cts.Token);
                textView.text = text;
                await voicevox.Play(voice, cts.Token);
            }
            catch (Exception e)
            {
                textView.text = e.Message;
            }

            button.interactable = true;
        }

        void OnDestroy()
        {
            cts.Cancel();
            cts.Dispose();
        }
    }
}