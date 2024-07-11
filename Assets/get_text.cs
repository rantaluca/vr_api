using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Text;
using UnityEngine.XR.Interaction.Toolkit;

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
        var container = GameObject.FindWithTag("Container_text");
        SimpleHelvetica helveticaText = textObject.GetComponent<SimpleHelvetica>();
        if (helveticaText == null)
        {
            Debug.LogError("SimpleHelvetica component not found on textObject!");
            yield break;
        }
        
        //scaleMultiplier  depends on text size 
        float scaleMultiplier = 0.2f;
        /*if text.Length > 10
        {
            scaleMultiplier = 0.1f;
        }
        if text.Length > 50
        {
            scaleMultiplier = 0.05f;
        }
        else if text.Length > 100
        {
            scaleMultiplier = 0.02f;
        }
        else if text.Length > 300
        {
            scaleMultiplier = 0.01f;
        }
        else if text.Length > 500
        {
            scaleMultiplier = 0.005f;
        }
        else
        {
            scaleMultiplier = 0.2f;
        }*/
        switch (text.Length)
        {
            case int n when (n > 500):
                scaleMultiplier = 0.005f;
                break;
            case int n when (n > 300):
                scaleMultiplier = 0.01f;
                break;
            case int n when (n > 100):
                scaleMultiplier = 0.02f;
                break;
            case int n when (n > 50):
                scaleMultiplier = 0.05f;
                break;
            case int n when (n > 10):
                scaleMultiplier = 0.1f;
                break;
            default:
                scaleMultiplier = 0.2f;
                break;
        }


        helveticaText.transform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, scaleMultiplier);

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
                    case 'ç': case 'Ç':
                        currentChar = 'c';
                        break;
                }
                displayedText.Append(currentChar);
            }

            helveticaText.Text = displayedText.ToString();
            helveticaText.GenerateText();
            yield return new WaitForSeconds(0.1f);
        }

        // Parcourir tous les enfants de textObject et leur ajouter les composants nécessaires
        //Packages/com.unity.xr.interaction.toolkit/Runtime/Interaction/Interactables/XRGrabInteractable.cs
        //Packages/com.unity.xr.interaction.toolkit/Runtime/Interaction/Transformers/XRGeneralGrabTransformer.cs

        
        foreach (Transform child in textObject.transform)
        {
            if (child.gameObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>() == null)
            {
                child.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            }

            if (child.gameObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Transformers.XRGeneralGrabTransformer>() == null)
            {
                child.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Transformers.XRGeneralGrabTransformer>();
            }

            // add a fixed joint with the container object 
            if (child.gameObject.GetComponent<FixedJoint>() == null)
            {
                child.gameObject.AddComponent<FixedJoint>();
                child.gameObject.GetComponent<FixedJoint>().connectedBody = container.GetComponent<Rigidbody>();
            }
            //add a hinge joint between the letters 
            /*
            if (child.gameObject.GetComponent<HingeJoint>() == null)
            {
                //connect to neighbor is not the last letter
                if (child.GetSiblingIndex() < text.Length - 1)
                {
                    child.gameObject.AddComponent<HingeJoint>();
                    child.gameObject.GetComponent<HingeJoint>().connectedBody = textObject.transform.GetChild(child.GetSiblingIndex() + 1).GetComponent<Rigidbody>();
                    //use spring
                    child.gameObject.GetComponent<HingeJoint>().useSpring = true;
                    //use limits
                    child.gameObject.GetComponent<HingeJoint>().useLimits = true;
                    child.gameObject.GetComponent<HingeJoint>().limits = new JointLimits { min = -10, max = 10 };
                }
            }
            */

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