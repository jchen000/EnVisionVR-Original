using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Threading.Tasks;
using System.Collections;

public class CameraFieldOfView : MonoBehaviour
{
    Camera mainCamera;
    Dictionary<string, float> importanceValues;
    // private SpeechSynthesizer synthesizer;
    SpeechSynthesizer synthesizer;
    private AudioSource audioSource;
    // private AudioSource audioSource;
    public float volume = 1f;
    public float spatialBlend = 1f;
    private SpatialSoundController soundController;


    void Start()
    {
        mainCamera = Camera.main;
        LoadImportanceValues();

        // Create SpeechConfig instance
        SpeechConfig speechConfig = SpeechConfig.FromSubscription("4de1d19d8bfe4fae9f46a2a3e848d548", "uksouth");

        // Create SpeechSynthesizer instance
        synthesizer = new SpeechSynthesizer(speechConfig);

        // Get the AudioSource component attached to the object
        audioSource = GetComponent<AudioSource>();

        soundController = GetComponent<SpatialSoundController>();
    }

    void Update()
    {
        // Update any other logic or behavior here
    }
    
    public async void CheckObjectsInFieldOfView()
    {
        if (mainCamera == null)
        {
            Debug.LogError("Main camera is not assigned!");
            return;
        }

        GameObject[] objectsInScene = GameObject.FindObjectsOfType<GameObject>();

        List<GameObject> objectsInFieldOfView = new List<GameObject>();

        foreach (GameObject obj in objectsInScene)
        {
            // Get the object's transform component to retrieve its position
            Transform objTransform = obj.transform;
            Vector3 objPosition = objTransform.position;

            // Check if the object is within the camera's field of view
            if (IsObjectVisible(objPosition, mainCamera.transform.position, mainCamera.transform.rotation, mainCamera.fieldOfView))
            {
                objectsInFieldOfView.Add(obj);
            }
        }

        // Calculate the new importance values for each object based on the distance from the camera
        foreach (GameObject obj in objectsInFieldOfView)
        {
            float originalImportance = GetImportanceValue(obj.name);
            float distance = Vector3.Distance(obj.transform.position, mainCamera.transform.position);
            float newImportance = originalImportance * (Mathf.Exp(-distance)-Mathf.Exp(-8));
            importanceValues[obj.name] = newImportance; // Update the importance value in the dictionary
        }

        // Sort the objects based on the new importance values
        objectsInFieldOfView.Sort((a, b) => GetImportanceValue(b.name).CompareTo(GetImportanceValue(a.name)));
        
        // // Print the three objects with the highest new importance values
        // for (int i = 0; i < Mathf.Min(3, objectsInFieldOfView.Count); i++)
        // {
        //     GameObject obj = objectsInFieldOfView[i];
        //     if (GetImportanceValue(obj.name) > 0)
        //     {
        //         string message = obj.name;
        //         string messageFull = obj.name + " is within the field of view. Position: " + obj.transform.position + " Importance: " + GetImportanceValue(obj.name);
        //         Debug.Log(messageFull);

        //         // SynthesizeText(message);
        //         // var result = SpeakText(message);
        //         SpeakText(message);

        //         // Get or add the SpatialSoundController component to the object
        //         SpatialSoundController soundController = obj.GetComponent<SpatialSoundController>();
        //         if (soundController == null)
        //             soundController = obj.AddComponent<SpatialSoundController>();

        //         // Play the audio clip at the object's position
        //         soundController.PlayAudioClip("Super Power Achieved", obj.transform.position);

        //         // Play the audio after a short delay
        //         float delay = 3.0f; // Adjust the delay time as needed
        //         StartCoroutine(PlayAudioClipWithDelay(soundController, "Super Power Achieved", objectsInFieldOfView[i].transform.position, delay));
        //         string objname = objectsInFieldOfView[i].name;
        //         Debug.Log("Sound of " + objname);
        //     }
        // }
        // Print the three objects with the highest new importance values
        for (int i = 0; i < Mathf.Min(3, objectsInFieldOfView.Count); i++)
        {
            GameObject obj = objectsInFieldOfView[i];
            if (GetImportanceValue(obj.name) > 0)
            {
                string message = obj.name;
                string messageFull = obj.name + " is within the field of view. Position: " + obj.transform.position + " Importance: " + GetImportanceValue(obj.name);
                Debug.Log(messageFull);
                // SynthesizeText(message);
                // var result = SpeakText(message);
                SpeakText(message);
                // Get or add the SpatialSoundController component to the object
                // SpatialSoundController soundController = obj.GetComponent<SpatialSoundController>();
                // if (soundController == null)
                //     soundController = obj.AddComponent<SpatialSoundController>();
                
                // Delay before playing the audio clip
                float delay = 2.0f; // Adjust the delay time as needed
                await Task.Delay((int)(delay * 800));

                // Play the audio clip at the object's position
                soundController.PlayAudioClip("Super Power Achieved", obj.transform.position);
        
                // float delay = 3.0f; // Adjust the delay time as needed
                // yield return new WaitForSeconds(delay);
                // Play the audio clip at the object's position
                // soundController.PlayAudioClip("Super Power Achieved", objectsInFieldOfView[i].transform.position);
                // PlayAudioClipWithDelay(soundController, "Super Power Achieved", objectsInFieldOfView[i].transform.position, delay);
                string objname = objectsInFieldOfView[i].name;
                Debug.Log("Sound of " + objname);

                await Task.Delay((int)(delay * 800));
            }
        }
    }

    // IEnumerator PlayAudioClipWithDelay(SpatialSoundController soundController, string clipName, Vector3 position, float delay)
    // {
    //     yield return new WaitForSeconds(delay);

    //     // Play the audio clip at the specified position
    //     soundController.PlayAudioClip(clipName, position);
    // }
 

    async void SpeakText(string text)
    {
        await synthesizer.SpeakTextAsync(text);
    }

    
    bool IsObjectVisible(Vector3 objectPosition, Vector3 cameraPosition, Quaternion cameraRotation, float fieldOfView)
    {
        // Calculate the direction from the camera to the object
        Vector3 cameraToObject = objectPosition - cameraPosition;

        // Calculate the angle between the camera's forward direction and the direction to the object
        float angle = Vector3.Angle(cameraRotation * Vector3.forward, cameraToObject);

        // Check if the angle is within the camera's field of view
        if (angle <= fieldOfView / 2f)
        {
            // Object is within the field of view
            return true;
        }

        // Object is outside the field of view
        return false;
    }

    void LoadImportanceValues()
    {
        string jsonFilePath = Path.Combine(Application.dataPath, "scene_graph_importance.json");

        if (File.Exists(jsonFilePath))
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
            SceneGraph sceneGraphImportance = JsonConvert.DeserializeObject<SceneGraph>(jsonContent);

            if (sceneGraphImportance != null)
            {
                importanceValues = new Dictionary<string, float>();
                // Traverse the scene graph and extract the importance values
                TraverseSceneGraph(sceneGraphImportance.children);

                Debug.Log("Importance values loaded successfully.");
            }
            else
            {
                Debug.LogError("Failed to deserialize JSON content.");
            }
        }
        else
        {
            Debug.LogError("JSON file not found at path: " + jsonFilePath);
        }
    }

    void TraverseSceneGraph(List<SceneGraph> children)
    {
        if (children == null || children.Count == 0)
            return;

        foreach (SceneGraph child in children)
        {
            if (child.importance.HasValue)
            {
                if (!importanceValues.ContainsKey(child.name))
                {
                    importanceValues.Add(child.name, child.importance.Value);
                }
                else
                {
                    Debug.LogWarning("Duplicate entry found for object: " + child.name);
                }
            }

            TraverseSceneGraph(child.children);
        }
    }

    float GetImportanceValue(string objectName)
    {
        if (importanceValues.ContainsKey(objectName))
        {
            return importanceValues[objectName];
        }

        Debug.LogWarning("Importance value not found for object: " + objectName);
        return 0f;
    }
}

[System.Serializable]
public class SceneGraph
{
public string name;
public string type;
public List<SceneGraph> children;
public List<ComponentData> components;
public float? importance;
}

[System.Serializable]
public class ComponentData
{
public string type;
// Add other component properties as needed
}
