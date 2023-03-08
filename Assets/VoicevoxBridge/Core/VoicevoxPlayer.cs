using System;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace VoicevoxBridge
{
    public class Voice : IDisposable
    {
        public int Speaker { get; private set; }
        public string Text { get; private set; }
        public AudioClip AudioClip { get; private set; }

        public bool IsDisposed { get; private set; }

        public Voice(int speaker, string text, AudioClip audioClip)
        {
            this.Speaker = speaker;
            this.Text = text;
            this.AudioClip = audioClip;
        }

        public void Dispose()
        {
            if (IsDisposed) return;
            if (AudioClip != null) UnityEngine.Object.Destroy(AudioClip);
            Text = null;
            IsDisposed = true;
        }
    }

    public class VoicevoxPlayer : IDisposable
    {
        const int MaxRequestConcurrency = 5;

        static AudioSource _sharedAudioSource = null;
        static AudioSource SharedAudioSource
        {
            get
            {
                if (_sharedAudioSource == null)
                {
                    var go = new GameObject("[VOICEVOX]SharedAudioSource");
                    _sharedAudioSource = go.AddComponent<AudioSource>();
                    GameObject.DontDestroyOnLoad(go);
                }
                return _sharedAudioSource;
            }
        }

        CancellationTokenSource cts = new CancellationTokenSource();
        AudioSource audioSource = null;

        Logger logger = new Logger();
        VoicevoxEngineAPI voicevoxAPI = null;
        SemaphoreSlim semaphore = null;

        public bool EnableLog { get => logger.enable; set => logger.enable = value; }

        public VoicevoxPlayer(string voicevoxEngineURL) : this(voicevoxEngineURL, SharedAudioSource) { }

        public VoicevoxPlayer(string voicevoxEngineURL, AudioSource audioSource)
        {
            this.audioSource = audioSource;
            voicevoxAPI = new VoicevoxEngineAPI(voicevoxEngineURL, logger);
            semaphore = new SemaphoreSlim(MaxRequestConcurrency, MaxRequestConcurrency);
        }

        public async Task<Voice> CreateVoice(int speaker, string text, CancellationToken cancellationToken = default)
        {
            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken).Token;

            await semaphore.WaitAsync(linkedToken);
            try
            {
                var jsonQuery = await voicevoxAPI.AudioQuery(speaker, text, linkedToken);
                var bytes = await voicevoxAPI.Synthesis(speaker, jsonQuery, linkedToken);
                return new Voice(speaker, text, AudioClipUtil.CreateFromBytes(bytes));
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task Play(Voice voice, CancellationToken cancellationToken = default, bool autoReleaseVoice = true)
        {
            if (voice.IsDisposed) throw new ArgumentException("This voice has already been disposed.");

            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken).Token;

            try
            {
                await PlayAudioClip(voice, linkedToken);
            }
            finally
            {
                if (autoReleaseVoice) voice.Dispose();
            }
        }

        public async Task PlayOneShot(int speaker, string text, CancellationToken cancellationToken = default)
        {
            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken).Token;
            using (var voice = await CreateVoice(speaker, text, linkedToken))
            {
                await PlayAudioClip(voice, linkedToken);
            }
        }

        async Task PlayAudioClip(Voice voice, CancellationToken cancellationToken)
        {
            audioSource.clip = voice.AudioClip;
            audioSource.Play();

            while (true)
            {
                await Task.Delay(1000 / 30, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                if (!audioSource.isPlaying) break;
            }
        }

        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();
        }
    }
}