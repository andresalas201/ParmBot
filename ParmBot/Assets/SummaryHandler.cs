using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using RESTObjects;

namespace Summarizer
{
    [System.Serializable]
    public class GeminiContent
    {
        public List<Part> parts;
    }

    [System.Serializable]
    public class GeminiRequestPayload
    {
        public List<GeminiContent> contents;
    }

    public class SummaryHandler : MonoBehaviour
    {
        private const string Model = "gemini-3.1-flash-lite";
        private string gemini_Key;

        private void Awake()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            TextAsset configAsset = Resources.Load<TextAsset>("config");
            if (configAsset == null)
            {
                Debug.LogError("SummaryHandler: Could not load config.json from Resources.");
                return;
            }

            var config = JsonUtility.FromJson<Config>(configAsset.text);
            gemini_Key = config.gemini_key;
        }

        public void Summarize(List<Article> articles, Action<string> onSuccess, Action<string> onError = null)
        {
            string prompt = BuildPrompt(articles);
            StartCoroutine(SendRequest(prompt, onSuccess, onError));
        }

        private string BuildPrompt(List<Article> articles)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < articles.Count; i++)
            {
                var article = articles[i];
                string keywordsStr = article.keywords != null
                    ? string.Join(", ", article.keywords)
                    : string.Empty;

                sb.AppendLine($"--- Article {i + 1} ---");
                sb.AppendLine($"Keywords: {keywordsStr}");
                sb.AppendLine(article.body);
                sb.AppendLine();
            }

            return "You are a news summarizer. Below are multiple articles. "
                 + "Write a single, cohesive summary that covers the key points from all of them. "
                 + "Do NOT invent or infer any information that is not explicitly stated in the articles. "
                 + "Use the provided keywords to guide emphasis and focus.\n\n"
                 + sb.ToString()
                 + "Provide a clear, concise joint summary:";
        }

        private IEnumerator SendRequest(string prompt, Action<string> onSuccess, Action<string> onError)
        {
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{Model}:generateContent?key={gemini_Key}";

            var payload = new GeminiRequestPayload
            {
                contents = new List<GeminiContent>
            {
                new GeminiContent
                {
                    parts = new List<Part>
                    {
                        new Part { text = prompt }
                    }
                }
            }
            };

            string jsonBody = JsonUtility.ToJson(payload);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

            using var request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                string errorMsg = $"SummaryHandler request failed: {request.error}\n{request.downloadHandler.text}";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
                yield break;
            }

            var response = JsonUtility.FromJson<GeminiResponse>(request.downloadHandler.text);
            string summary = response?.candidates?[0]?.content?.parts?[0]?.text;

            if (string.IsNullOrEmpty(summary))
            {
                string errorMsg = "SummaryHandler: Received empty or malformed response.";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
                yield break;
            }

            onSuccess?.Invoke(summary);
        }
    }
}