using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Threading.Tasks;

public class CameraFieldOfView : MonoBehaviour
{
    Camera mainCamera;
    Dictionary<string, float> importanceValues;
    SpeechSynthesizer synthesizer;

    void Start()
    {
        mainCamera = Camera.main;
        LoadImportanceValues();

        // Create SpeechConfig instance
        SpeechConfig speechConfig = SpeechConfig.FromSubscription("4de1d19d8bfe4fae9f46a2a3e848d548", "uksouth");

        // Create SpeechSynthesizer instance
        synthesizer = new SpeechSynthesizer(speechConfig);
    }

    void OnDestroy()
    {
        synthesizer?.Dispose();
    }

    void Update()
    {
        // Update any other logic or behavior here
    }

    public void CheckObjectsInFieldOfView()
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

        // Print the three objects with the highest new importance values
        for (int i = 0; i < Mathf.Min(3, objectsInFieldOfView.Count); i++)
        {
            GameObject obj = objectsInFieldOfView[i];
            if (GetImportanceValue(obj.name) > 0)
            {
                string message = obj.name;
                string messageFull = obj.name + " is within the field of view. Position: " + obj.transform.position + " Importance: " + GetImportanceValue(obj.name);
                Debug.Log(messageFull);
                SpeakText(message);
            }
        }
    }

    async void SpeakText(string text)
    {
        await synthesizer.SpeakTextAsync(text);
        // using (var result = await synthesizer.SpeakTextAsync(text))
        // {
        //     if (result.Reason == ResultReason.SynthesizingAudioCompleted)
        //     {
        //         await SaveAudioToWaveFile(result.AudioData, "audio.wav");
        //         PlayAudioFromFile("audio.wav");
        //     }
        //     else if (result.Reason == ResultReason.Canceled)
        //     {
        //         var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
        //         Debug.LogError($"Speech synthesis canceled: {cancellation.Reason}");

        //         if (cancellation.Reason == CancellationReason.Error)
        //         {
        //             Debug.LogError($"Error details: {cancellation.ErrorDetails}");
        //         }
        //     }
        // }
    }

    // async Task SaveAudioToWaveFile(AudioData audioData, string filePath)
    // {
    //     using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
    //     {
    //         await audioData.WriteToWaveStreamAsync(fileStream);
    //     }
    // }

    // void PlayAudioFromFile(string filePath)
    // {
    //     if (File.Exists(filePath))
    //     {
    //         StartCoroutine(PlayAudioClipCoroutine(filePath));
    //     }
    //     else
    //     {
    //         Debug.LogError("Audio file not found: " + filePath);
    //     }
    // }

    // IEnumerator<YieldInstruction> PlayAudioClipCoroutine(string filePath)
    // {
    //     using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
    //     {
    //         var audioClip = CreateAudioClipFromWaveStream(fileStream);
    //         if (audioClip != null)
    //         {
    //             var audioSource = gameObject.AddComponent<AudioSource>();
    //             audioSource.clip = audioClip;
    //             audioSource.Play();

    //             yield return new WaitForSeconds(audioClip.length);

    //             audioSource.Stop();
    //             Destroy(audioSource);
    //         }
    //     }
    // }

    // AudioClip CreateAudioClipFromWaveStream(Stream waveStream)
    // {
    //     byte[] waveBytes = new byte[waveStream.Length];
    //     waveStream.Read(waveBytes, 0, waveBytes.Length);

    //     AudioClip audioClip = WavUtility.ToAudioClip(waveBytes, 0, waveBytes.Length, 0);
    //     return audioClip;
    // }

    // Simple WavUtility class to convert audio data to AudioClip
    // public static class WavUtility
    // {
    //     public static AudioClip ToAudioClip(byte[] wavData)
    //     {
    //         // Convert audio data to float array
    //         float[] floatData = ConvertByteArrayToFloatArray(wavData);

    //         // Create an AudioClip from float data
    //         AudioClip clip = AudioClip.Create("GeneratedAudio", floatData.Length, 1, 16000, false);
    //         clip.SetData(floatData, 0);

    //         return clip;
    //     }

    //     private static float[] ConvertByteArrayToFloatArray(byte[] byteArray)
    //     {
    //         // Convert byte array to float array
    //         float[] floatArray = new float[byteArray.Length / 2];
    //         for (int i = 0; i < floatArray.Length; i++)
    //         {
    //             floatArray[i] = (float)(System.BitConverter.ToInt16(byteArray, i * 2)) / 32768.0f;
    //         }
    //         return floatArray;
    //     }
    // }

    
    
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
