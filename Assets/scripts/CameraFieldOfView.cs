using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

public class CameraFieldOfView : MonoBehaviour
{
    Camera mainCamera;
    Dictionary<string, float> importanceValues;

    void Start()
    {
        mainCamera = Camera.main; // Assign the main camera in the scene to mainCamera

        // Load the importance values from the JSON file
        LoadImportanceValues();
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
            if (GetImportanceValue(obj.name)>0)
            {
            Debug.Log(obj.name + " is within the field of view. Position: " + obj.transform.position + "Importance:" + GetImportanceValue(obj.name));
            }
        }
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
