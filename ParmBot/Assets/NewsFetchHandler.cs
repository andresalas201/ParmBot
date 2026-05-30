using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using RESTObjects;

namespace NewsFetch
{

    public class NewsFetchHandler : MonoBehaviour
    {
        private const string NEWSAPI_ENDPOINT = "https://eventregistry.org/api/v1/article/getArticles";
        private const int MAX_EVENTS = 5;

        private string newsApiKey;

        void Awake()
        {
            // Load config from Resources/config.json
            TextAsset configAsset = Resources.Load<TextAsset>("config");
            if (configAsset != null)
            {
                var config = JsonUtility.FromJson<Config>(configAsset.text);
                newsApiKey = config.news_key;
            }
            else
            {
                Debug.LogError("config.json not found in Resources folder.");
            }
        }

        public void FetchMain(string query, Action<List<Article>> onSuccess, Action<string> onError = null)
        {
            StartCoroutine(FetchEvents(query, onSuccess, onError));
        }

        public IEnumerator FetchEvents(string query, Action<List<Article>> onSuccess, Action<string> onError)
        {
            string jsonPayload = $@"{{
            ""action"": ""getArticles"",
            ""query"": {{
                ""$query"": {{
                    ""keyword"": ""{EscapeJson(query)}"",
                    ""keywordLoc"": ""body"",
                    ""lang"": ""spa""
                }}
            }},
            ""resultType"": ""articles"",
            ""articlesSortBy"": ""date"",
            ""articlesSortByAsc"": false,
            ""articlesCount"": {MAX_EVENTS},
            ""includeArticleTitle"": true,
            ""includeArticleKeywords"": true,
            ""includeArticleUrl"": true,
            ""includeArticleBody"": true,
            ""articleBodyLen"": 3000,
            ""apiKey"": ""{newsApiKey}""
        }}";

            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

            using UnityWebRequest request = new UnityWebRequest(NEWSAPI_ENDPOINT, "POST");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"News API request failed: {request.error}");
                onError?.Invoke(request.error);
                yield break;
            }

            string json = request.downloadHandler.text;
            try
            {
                NewsApiResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<NewsApiResponse>(json);
                List<Article> articles = response?.articles?.results ?? new List<Article>();
                onSuccess?.Invoke(articles);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse API response: {e.Message}");
                onError?.Invoke(e.Message);
            }
        }

        public string EscapeJson(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

    }

    
}