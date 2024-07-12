using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Text;
using UnityEditor;

public class AttachOnCollision : MonoBehaviour
{
    private bool hasJoint = false;
    private bool hasTouchedGround = false;
    public GameObject simpleHelveticaPrefab;
    public GameObject emptyParentObject;

	//sound effect Victory
	public AudioSource audioSourceV;
	public AudioClip audioClipV;
	
	//sound effect Lose
	public AudioSource audioSourceL;
	public AudioClip audioClipL;
	
	//sound effect Grass
	public AudioSource audioSourceG;
	public AudioClip audioClipG;
	
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log(hasTouchedGround);

        // Check if the other object has a Rigidbody and there's no joint yet
        // Get the neighbors of the object from the list 'words' in the parent object
        string[] words = gameObject.GetComponentInParent<LocalGPTRequest>().words;
        // Get the index of the object in the list
        int index = System.Array.IndexOf(words, gameObject.tag);
        // Get the neighbor of the object
        string neighbor = words.Length > index + 1 ? words[index + 1] : null;

        if (neighbor != null && collision.gameObject.GetComponent<Rigidbody>() != null && collision.gameObject.tag == neighbor && hasTouchedGround)
        {
			// PLAY SOUND
			audioSourceV.clip = audioClipV;
			audioSourceV.Play();
			
            // CREATE A NEW HELVETICA TEXT OBJECT COMBINING THE TWO OTHERS AND DELETE THE TWO COLLIDING 
            AddTag(words[index] + "_" + neighbor);
            string[] newWords = new string[words.Length - 1];
            int newWordsIndex = 0;

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i] == words[index]) 
                {
                    newWords[newWordsIndex++] = words[index] + " " + neighbor;
                    continue;
                }
                if (words[i] == neighbor)
                {
                    continue;
                }
                newWords[newWordsIndex++] = words[i];
            }
            gameObject.GetComponentInParent<LocalGPTRequest>().words = newWords;

            // Use the parent of the current word object
            Transform parentTransform = gameObject.transform.parent;

            // Instantiate the new word object as a child of the same parent
            GameObject wordObject = Instantiate(simpleHelveticaPrefab, parentTransform);

            // position of the old word object 

            // Define position ( position of the old word object)
            wordObject.transform.localPosition = gameObject.transform.localPosition;
            wordObject.tag = newWords[index];

            SimpleHelvetica helveticaText = wordObject.GetComponent<SimpleHelvetica>();
            if (helveticaText == null)
            {
                Debug.LogError("SimpleHelvetica component not found on prefab!");
                return;
            }
            helveticaText.Text = newWords[index];
            helveticaText.GenerateText();

            // Destroy the two objects
            Destroy(gameObject);
            Destroy(collision.gameObject);
        }

        else if (collision.gameObject.name == "rabbids_coliseum_fbx")
        {
			// PLAY SOUND
			audioSourceG.clip = audioClipG;
			audioSourceG.Play();
			
            hasTouchedGround = true;
			
        }
		
		else
		{
			// PLAY SOUND
			audioSourceL.clip = audioClipL;
			audioSourceL.Play();
		}
        Debug.Log(collision.gameObject.name);    
    }

    // This method adds a new tag to the Tags list in the Editor settings
    public void AddTag(string tag)
    {
        if (!IsTagExists(tag))
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            int tagsCount = tagsProp.arraySize;
            tagsProp.InsertArrayElementAtIndex(tagsCount);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(tagsCount);
            newTagProp.stringValue = tag;
            tagManager.ApplyModifiedProperties();
            tagManager.Update();
        }
    }

    // This method checks if a tag already exists
    public bool IsTagExists(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty tagProp = tagsProp.GetArrayElementAtIndex(i);
            if (tagProp.stringValue.Equals(tag)) return true;
        }
        return false;
    }
}