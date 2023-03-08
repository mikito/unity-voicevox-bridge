using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using System.Text;
using System.Net;

namespace VoicevoxBridge
{
    public class VoicevoxEngineAPI
    {
        HttpClient httpClient;
        Uri engineServerURL;
        Logger logger;

        public VoicevoxEngineAPI(string serverURL, Logger logger)
        {
            httpClient = new HttpClient();
            this.engineServerURL = new Uri(serverURL);
            this.logger = logger;
        }

        public async Task<string> AudioQuery(int speaker, string text, CancellationToken cancellationToken = default)
        {
            string url = $"{engineServerURL}audio_query?speaker={speaker}&text={text}";
            logger.Log("request: " + url);

            try
            {
                var response = await httpClient.PostAsync(url, null, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    logger.Log("AudioQuery request success");
                    var jsonString = await response.Content.ReadAsStringAsync();
                    return jsonString;
                }
                else
                {
                    var message = await response.Content.ReadAsStringAsync();
                    throw new WebException($"AudioQuery Request failed: {(int)response.StatusCode} {response.StatusCode}\n{message}");
                }
            }
            catch (Exception e)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw e;
            }
        }

        public async Task<byte[]> Synthesis(int speaker, string jsonQuery, CancellationToken cancellationToken = default)
        {
            string url = $"{engineServerURL}synthesis?speaker={speaker}";
            logger.Log("request: " + url);

            var content = new StringContent(jsonQuery, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(url, content, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    logger.Log("Synthesis request success");
                    return await response.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    var message = await response.Content.ReadAsStringAsync();
                    throw new WebException($"Synthesis Request failed: {(int)response.StatusCode} {response.StatusCode}\n{message}");
                }
            }
            catch (Exception e)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw e;
            }
        }
    }
}