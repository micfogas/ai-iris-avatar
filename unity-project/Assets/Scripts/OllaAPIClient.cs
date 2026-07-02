using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

[System.Serializable]
public class OllaMessage
{
    public string role;
    public string content;
}

[System.Serializable]
public class OllaRequest
{
    public string model = "aomi-base"; 
    public OllaMessage[] messages;
    public float temperature = 0.7f;
    public bool stream = false;
}

[System.Serializable]
public class OllaResponse
{
    public Choice[] choices;
    [System.Serializable]
    public class Choice { public OllaMessage message; }
}

public class OllaAPIClient : MonoBehaviour
{
    [Tooltip("The local Olla Proxy endpoint.")]
    public string ollaEndpoint = "http://127.0.0.1:8080/v1/chat/completions";

    [Tooltip("Called when a text prompt is sent.")]
    public UnityEvent<string> onSendQuery;

    [Tooltip("Called when the text response is received from Olla.")]
    public UnityEvent<string> onTextResponseReceived;

    [Tooltip("System prompt to establish Aomi's personality.")]
    [TextArea(3, 10)]
    public string systemPrompt = "You are Aomi, a helpful and empathetic AI personal assistant.";

    public void SendQuery(string userText)
    {
        Debug.Log($"User Prompt: {userText}");
        onSendQuery?.Invoke(userText);
        StartCoroutine(PostToOlla(userText));
    }

    private IEnumerator PostToOlla(string userText)
    {
        // Build the message history (System Prompt + User Input)
        var reqObj = new OllaRequest
        {
            messages = new[] 
            { 
                new OllaMessage { role = "system", content = systemPrompt },
                new OllaMessage { role = "user", content = userText } 
            }
        };

        string jsonPayload = JsonUtility.ToJson(reqObj);

        using (UnityWebRequest request = new UnityWebRequest(ollaEndpoint, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Olla Error] {request.error}: {request.downloadHandler.text}");
            }
            else
            {
                var response = JsonUtility.FromJson<OllaResponse>(request.downloadHandler.text);
                if (response != null && response.choices != null && response.choices.Length > 0)
                {
                    string aiResponse = response.choices[0].message.content;
                    Debug.Log($"Aomi: {aiResponse}");
                    
                    onTextResponseReceived?.Invoke(aiResponse);
                }
            }
        }
    }
}
