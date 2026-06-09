using UnityEngine;
using InputScript;
using UnityEngine.UI;

public class MuteButtonHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject.FindWithTag("AudioButton").GetComponent<Button>().onClick.AddListener(OnClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick()
    {
        Debug.Log("MuteButton Clicked");
        InputHandler handler = GameObject.FindWithTag("Input").GetComponent<InputHandler>();
        handler.ttsHandler.ToggleMute();
    }
}
