using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Text;

public class LocalGPTRequest : MonoBehaviour
{
    public string apiUrl = "http://localhost:1234/v1/chat/completions";
    public string apiKey = "lm-studio";
    public string model = "TheBloke/Mistral-7B-Instruct-v0.2-GGUF";
    public string consign = "Answer in french. Be very short with your answers.";
    public string prompt = "Comment ça va.";
    public string temperature = "0.7";
    public GameObject textObject;

    void Start()
    {
        if (textObject == null)
        {
            Debug.LogError("Text Object not set!");
            return;
        }

        // Start the coroutine to send the GPT request
        StartCoroutine(SendGPTRequest());
    }

    IEnumerator SendGPTRequest()
    {
        Debug.Log("Asking: " + prompt + " to " + model);
        
        string jsonBody = "{\"model\": \"" + model + "\", \"messages\": [{\"role\": \"system\", \"content\": \"" + consign + "\"}, {\"role\": \"user\", \"content\": \"" + prompt + "\"}], \"temperature\": " + temperature + "}";

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            string responseText = request.downloadHandler.text;
            Debug.Log("Response: " + responseText);
            
            GPTResponse response = JsonUtility.FromJson<GPTResponse>(responseText);

            if (response != null && response.choices.Length > 0)
            {
                StartCoroutine(DisplayText(response.choices[0].message.content));
            }
        }
    }

    IEnumerator DisplayText(string text)
    {
        SimpleHelvetica helveticaText = textObject.GetComponent<SimpleHelvetica>();
        if (helveticaText == null)
        {
            Debug.LogError("SimpleHelvetica component not found on textObject!");
            yield break;
        }
        
        // Display characters one by one
        StringBuilder displayedText = new StringBuilder();
        for (int i = 0; i < text.Length; i++)
        {
            char currentChar = text[i];
            Debug.Log(currentChar);

            // Jump a line if the character is a period
            if (currentChar == '.')
            {
                displayedText.Append('\n');
            }
            else
            {
                // Replace special characters with their non-accented versions
                switch (currentChar)
                {
                    case 'é': case 'è': case 'ê': case 'ë':
                        currentChar = 'e';
                        break;
                    case 'à': case 'â': case 'ä':
                        currentChar = 'a';
                        break;
                    case 'î':
                        currentChar = 'i';
                        break;
                    case 'ù':
                        currentChar = 'u';
                        break;
                    case 'ç':
                        currentChar = 'c';
                        break;
                }
                displayedText.Append(currentChar);
            }

            helveticaText.Text = displayedText.ToString();
            helveticaText.GenerateText();
            yield return new WaitForSeconds(0.1f);
        }
        
        yield return null;
    }

    [System.Serializable]
    public class GPTResponse
    {
        public Choice[] choices;
    }

    [System.Serializable]
    public class Choice
    {
        public Message message;
    }

    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            StartCoroutine(SendGPTRequest());
        }
    }
}