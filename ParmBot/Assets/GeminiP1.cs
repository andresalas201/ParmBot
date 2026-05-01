using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class GeminiCaller : MonoBehaviour
{
    private string API_KEY;
    private const string MODEL = "gemini-2.5-flash-lite";
    private string URL;

    [System.Serializable]
    private class Config
    {
        public string gemini_key;
    }

    [System.Serializable]
    private class GeminiResponse
    {
        public Candidate[] candidates;
    }

    [System.Serializable]
    private class Candidate
    {
        public Content content;
    }

    [System.Serializable]
    private class Content
    {
        public Part[] parts;
    }

    [System.Serializable]
    private class Part
    {
        public string text;
    }

    void Awake()
    {
        // Load key from Resources/config.json
        TextAsset configFile = Resources.Load<TextAsset>("config");

        if (configFile == null)
        {
            Debug.LogError("config.json not found in Resources folder!");
            return;
        }

        Config config = JsonUtility.FromJson<Config>(configFile.text);
        API_KEY = config.gemini_key;
    }

    void Start()
    {
        URL = "https://generativelanguage.googleapis.com/v1beta/models/"
              + MODEL + ":generateContent?key=" + API_KEY;

        StartCoroutine(CallGemini("¿Que fecha es hoy?"));
    }

    IEnumerator CallGemini(string prompt)
    {
        string jsonBody = $@"{{
            ""contents"": [{{
                ""parts"": [{{""text"": ""{prompt}""}}]
            }}]
        }}";

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using var request = new UnityWebRequest(URL, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            GeminiResponse response = JsonUtility.FromJson<GeminiResponse>(request.downloadHandler.text);
            string text = response.candidates[0].content.parts[0].text;
            Debug.Log("Gemini says: " + text);
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }
}