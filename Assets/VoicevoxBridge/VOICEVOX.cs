using System.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace VoicevoxBridge
{
    public class VOICEVOX : MonoBehaviour
    {
        [SerializeField] AudioSource audioSource = null;
        [SerializeField] string voicevoxEngineURL = "http://localhost:50021/";
        [SerializeField] bool enableLog = false;

        VoicevoxPlayer controller = null;

        void Awake()
        {
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            controller = new VoicevoxPlayer(voicevoxEngineURL, audioSource);
            controller.EnableLog = enableLog;
        }

        public async Task<Voice> CreateVoice(int speaker, string text, CancellationToken cancellationToken = default)
        {
            return await controller.CreateVoice(speaker, text, cancellationToken);
        }

        public async Task Play(Voice voice, CancellationToken cancellationToken = default, bool autoReleaseVoice = true)
        {
            await controller.Play(voice, cancellationToken, autoReleaseVoice);
        }

        public async Task PlayOneShot(int speaker, string text, CancellationToken cancellationToken = default)
        {
            await controller.PlayOneShot(speaker, text, cancellationToken);
        }

        void OnDestroy()
        {
            if (controller != null) controller.Dispose();
        }
    }
}