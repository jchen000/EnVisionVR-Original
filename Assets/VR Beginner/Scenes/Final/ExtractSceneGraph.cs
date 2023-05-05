using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneGraphExtractor : MonoBehaviour
{
    [MenuItem("Scene Graph/Print Scene Graph")]
    public static void PrintSceneGraph()
    {
        Debug.Log("Scene Graph:");
        PrintGameObjectHierarchy(SceneManager.GetActiveScene().GetRootGameObjects(), 0);
    }

    [MenuItem("Scene Graph/Export Scene Graph to JSON")]
    public static void ExportSceneGraphToJson()
    {
        // Get the active scene
        Scene scene = SceneManager.GetActiveScene();

        // Extract the hierarchy of game objects
        List<GameObjectInfo> gameObjects = new List<GameObjectInfo>();
        foreach (GameObject rootGameObject in scene.GetRootGameObjects())
        {
            gameObjects.Add(ExtractGameObjectInfo(rootGameObject));
        }

        // Serialize the scene graph data as JSON using JsonUtility
        string json = JsonUtility.ToJson(new SceneGraph(scene.name, gameObjects), true);

        // Write the JSON data to a file
        string outputPath = Path.Combine(Application.dataPath, "scene-graph.json");
        File.WriteAllText(outputPath, json);

        Debug.Log("Scene graph saved to " + outputPath);
    }

    private static void PrintGameObjectHierarchy(GameObject[] gameObjects, int depth)
    {
        foreach (GameObject gameObject in gameObjects)
        {
            Debug.Log(new string('\t', depth) + gameObject.name);

            // Recursively print the child game objects
            PrintGameObjectHierarchy(GetChildGameObjects(gameObject), depth + 1);
        }
    }

    private static GameObject[] GetChildGameObjects(GameObject gameObject)
    {
        Transform[] childTransforms = gameObject.GetComponentsInChildren<Transform>();
        GameObject[] childGameObjects = new GameObject[childTransforms.Length - 1];
        for (int i = 1; i < childTransforms.Length; i++)
        {
            childGameObjects[i - 1] = childTransforms[i].gameObject;
        }
        return childGameObjects;
    }

    private static GameObjectInfo ExtractGameObjectInfo(GameObject gameObject)
    {
        List<ComponentInfo> componentInfos = new List<ComponentInfo>();
        foreach (Component component in gameObject.GetComponents<Component>())
        {
            componentInfos.Add(new ComponentInfo(component.GetType().ToString(), GetSerializedData(component)));
        }

        List<GameObjectInfo> childGameObjectInfos = new List<GameObjectInfo>();
        foreach (GameObject childGameObject in GetChildGameObjects(gameObject))
        {
            childGameObjectInfos.Add(ExtractGameObjectInfo(childGameObject));
        }

        return new GameObjectInfo(gameObject.name, componentInfos.ToArray(), childGameObjectInfos.ToArray());
    }

    private static string GetSerializedData(Component component)
    {
        SerializedObject serializedObject = new SerializedObject(component);
        SerializedProperty serializedProperty = serializedObject.GetIterator();
        serializedProperty.Next(true);
        while (serializedProperty.Next(false))
        {
            if (serializedProperty.propertyType == SerializedPropertyType.ObjectReference)
            {
                serializedProperty.objectReferenceValue = null;
            }
        }
        return serializedObject.ToString();
    }

    [System.Serializable]
    private class SceneGraph
    {
        public string sceneName;
        public GameObjectInfo[] gameObjects;

        public SceneGraph(string sceneName, List<GameObjectInfo> gameObjects)
        {
            this.sceneName = sceneName;
            this.gameObjects = gameObjects.ToArray();
    }
}

[System.Serializable]
private class GameObjectInfo
{
    public string name;
    public ComponentInfo[] components;
    public GameObjectInfo[] children;

    public GameObjectInfo(string name, ComponentInfo[] components, GameObjectInfo[] children)
    {
        this.name = name;
        this.components = components;
        this.children = children;
    }
}

[System.Serializable]
private class ComponentInfo
{
    public string type;
    public string serializedData;

    public ComponentInfo(string type, string serializedData)
    {
        this.type = type;
        this.serializedData = serializedData;
    }
}
}

