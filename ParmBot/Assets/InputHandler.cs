using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using RESTObjects;
using NewsFetch;
using Summarizer;

public class InputHandler : MonoBehaviour
{
    public TMP_InputField inputField;
    private string API_KEY;
    private const string MODEL = "gemini-3.1-flash-lite";
    private string URL;
    private TMP_Text conversationText;
    bool firstInput = true;
    public NewsFetchHandler newsFetcher;
    public List<Article> articles = new List<Article>();
    public SummaryHandler Summarizer;
    private string summary;
    private StringBuilder articleContext;

    private string startingPrompt = "You are ParmBot, a chatbot made to talk about news\n" +
       "You should talk informally and directly. Base everything you say on the news articles given, always cite the article where you got the information\n" +
       "Talk in Spanish";

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
        newsFetcher = gameObject.AddComponent<NewsFetchHandler>();
        Summarizer = gameObject.AddComponent<SummaryHandler>();
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
            
            this.conversationText.text += "\n\nUsuario: " + value;
            if (this.firstInput)
            {
                Debug.Log("Se envia a NewsAPI");
                StartCoroutine(SummarizeEvent(value));
                this.firstInput = false;
            }
            else
            {
                Debug.Log("Se envía a gemini");
                StartCoroutine(CallGemini(value)); 
            }
            this.inputField.text = "";
        }
    }

    private IEnumerator SummarizeEvent(string userMessage)
    { 
        yield return StartCoroutine(newsFetcher.FetchEvents(userMessage,
            result => articles = result,
            error => Debug.LogError(error)));

        Debug.Log($"Se encuentran {articles?.Count} articulos");

        Summarizer.Summarize(articles,
        onSuccess: summary =>
        {
            this.summary = summary;
            this.summary += "\nFuentes:\n";
            this.summary += "=========================";
            foreach (Article a in articles)
            {
                this.summary += "\n----------------------";
                this.summary += "\n" + a.title + ": " + a.url;
                this.summary += "\n----------------------";
            }
            this.summary += "\n=========================";
            history.Add(new ConversationTurn
            {
                role = "model",
                parts = new Part[] { new Part { text = this.summary } }
            });
            conversationText.text += "\n\nParmbot: " + this.summary;
            this.CreateContext();
        },
        onError: err => Debug.LogError(err));

    }

    private void CreateContext()
    {
        this.articleContext = new StringBuilder();
        this.articleContext.AppendLine("\n\nReferencias:");
        this.articleContext.AppendLine("=========================");
        foreach (Article a in articles)
        {
            this.articleContext.AppendLine($"Título: {a.title}");
            this.articleContext.AppendLine($"Contenido: {a.body}");
            this.articleContext.AppendLine("-------------------------");
        }
    }

    private List<ConversationTurn> history = new();

    public IEnumerator CallGemini(string userMessage)
    {
        history.Add(new ConversationTurn
        {
            role = "user",
            parts = new Part[] { new Part { text = userMessage } }
        });

        var requestBody = new RequestBody
        {
            system_instruction = new SystemInstruction
            {
                parts = new Part[] { new Part { text = this.startingPrompt + this.articleContext.ToString() } }
            },
            contents = history,
        };

    string jsonBody = JsonUtility.ToJson(requestBody);
        Debug.Log("Sending JSON: " + jsonBody);

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using var request = new UnityWebRequest(URL, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            GeminiResponse response = JsonUtility.FromJson<GeminiResponse>(
                request.downloadHandler.text
            );
            string replyText = response.candidates[0].content.parts[0].text;

            history.Add(new ConversationTurn
            {
                role = "model",
                parts = new Part[] { new Part { text = replyText } }
            });

            conversationText.text += "\n\nParmbot: " + replyText;

        }
        else
        {
            Debug.LogError("HTTP Code: " + request.responseCode);
            Debug.LogError("Response: " + request.downloadHandler.text);
        }
    }

    void OnDestroy()
    {
        inputField.onSubmit.RemoveListener(Submit);
    }

    public string EscapeJson(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
}