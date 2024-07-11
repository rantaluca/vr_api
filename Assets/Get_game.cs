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
    public string consign = "Answer in English. Be very short and funny with your answers. You must generate a funny sentence in less than 5 words";
    public string prompt = "Generate a funny sentence in less than 5 words";
    public string temperature = "0.7";
    public GameObject emptyParentObject;
    public GameObject simpleHelveticaPrefab;

    void Start()
    {
        if (emptyParentObject == null)
        {
            Debug.LogError("Empty Parent Object not set!");
            return;
        }

        if (simpleHelveticaPrefab == null)
        {
            Debug.LogError("Simple Helvetica Prefab not set!");
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
            StartCoroutine(DisplayText("ChatGPT is not working"));
            
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
        string[] words = text.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            words[i] = words[i].Replace("à", "a");
            words[i] = words[i].Replace("é", "e");
            words[i] = words[i].Replace("è", "e");
            words[i] = words[i].Replace("ê", "e");
            words[i] = words[i].Replace("ë", "e");
            words[i] = words[i].Replace("ï", "i");
            words[i] = words[i].Replace("î", "i");
            words[i] = words[i].Replace("ô", "o");
            words[i] = words[i].Replace("ö", "o");
            words[i] = words[i].Replace("ù", "u");
            words[i] = words[i].Replace("û", "u");
            words[i] = words[i].Replace("ü", "u");
            words[i] = words[i].Replace("ç", "c");
            words[i] = words[i].ToLower();

            GameObject wordObject = Instantiate(simpleHelveticaPrefab, emptyParentObject.transform);
            //define position to 00
            wordObject.transform.localPosition = new Vector3(0, 0, 0);
            wordObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            wordObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Transformers.XRGeneralGrabTransformer>();

            SimpleHelvetica helveticaText = wordObject.GetComponent<SimpleHelvetica>();
            if (helveticaText == null)
            {
                Debug.LogError("SimpleHelvetica component not found on prefab!");
                yield break;
            }

            helveticaText.Text = words[i];
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
        if (Input.GetKeyDown(KeyCode.N))
        {
            StartCoroutine(SendGPTRequest());
        }
    }
}