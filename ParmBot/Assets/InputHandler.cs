using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public TMP_InputField inputField;
    private string API_KEY;
    private const string MODEL = "gemini-3.1-flash-lite-preview";
    private string URL;
    private TMP_Text conversationText;

    private string startingPrompt = "You are ParmBot, a chatbot made to talk about news\n" +
       "You should talk informally and base everything you say on news articles found on the web, always cite the article where you got the information\n" +
        "Talk in Spanish";

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
        Debug.Log("InputHandler exists");
        URL = "https://generativelanguage.googleapis.com/v1beta/models/"
              + MODEL + ":generateContent?key=" + API_KEY;

        this.conversationText = GameObject.FindWithTag("Conversation").GetComponent<TMP_Text>();
        this.conversationText.text = "\nParmBot: Bienvenido, soy ParmBot, tu ayudante para aprender";
        this.inputField = GameObject.FindWithTag("Input").GetComponent<TMP_InputField>();
        inputField.onEndEdit.AddListener(Submit);
    }

    void Submit(string value)
    {
        var keyboard = Keyboard.current;
        if (keyboard != null &&
            (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame))
        {
            Debug.Log("Se envía a gemini");
            this.conversationText.text += "\nUsuario: " + value;
            StartCoroutine(CallGemini(value));
            this.inputField.text = "";
        }
    }

    public IEnumerator CallGemini(string prompt)
    {
        string fullPrompt = (this.startingPrompt + this.conversationText.text)
        .Replace("\\", "\\\\")
        .Replace("\"", "\\\"")
        .Replace("\n", "\\n")
        .Replace("\r", "");

        string jsonBody = $@"{{
            ""contents"": [{{
                ""parts"": [{{""text"": ""{fullPrompt}""}}]
            }}],
            ""tools"": [{{
                ""google_search"": {{}}
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
            this.conversationText.text +=  "\nParmbot: " + text;
        }
        else
        {
            Debug.LogError("Error: " + request.error);
            Debug.LogError("Response body: " + request.downloadHandler.text);
        }
    }

    void OnDestroy()
    {
        inputField.onSubmit.RemoveListener(Submit);
    }
}