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
    private const string MODEL = "gemini-2.5-flash-lite";
    private string URL;
    private TMP_Text conversationText;

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
        this.conversationText.text = "ParmBot: Bienvenido, soy ParmBot, tu ayudante para aprender";
        this.inputField = GameObject.FindWithTag("Input").GetComponent<TMP_InputField>();
        inputField.onEndEdit.AddListener(Submit);
    }

    void Submit(string value)
    {
        Debug.Log("Toilet");
        var keyboard = Keyboard.current;
        if (keyboard != null &&
            (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame))
        {
            Debug.Log("Se envía a gemini");
            this.conversationText.text += "\nUsuario: " + value;
            StartCoroutine(CallGemini(value));
        }
    }

    public IEnumerator CallGemini(string prompt)
    {
        string jsonBody = $@"{{
            ""contents"": [{{
                ""parts"": [{{""text"": ""{this.conversationText.text}""}}]
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
        }
    }

    void OnDestroy()
    {
        inputField.onSubmit.RemoveListener(Submit);
    }
}