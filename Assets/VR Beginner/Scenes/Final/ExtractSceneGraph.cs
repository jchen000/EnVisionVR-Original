using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SceneGraphExtractor : MonoBehaviour
{
    [MenuItem("Tools/Extract Scene Graph")]
    public static void ExtractSceneGraph()
    {
        var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        // Create a list to store the scene graph data
        List<SceneGraphNode> sceneGraphNodes = new List<SceneGraphNode>();

        foreach (var rootObject in rootObjects)
        {
            SceneGraphNode rootNode = ExtractSceneGraphRecursive(rootObject);
            sceneGraphNodes.Add(rootNode);
        }

        // Convert the scene graph data to a JSON string
        string json = JsonUtility.ToJson(new SceneGraphData { sceneGraphNodes = sceneGraphNodes }, true);

        // Save the JSON string to a file
        string path = Application.dataPath + "/scene_graph.json";
        File.WriteAllText(path, json);

        Debug.Log("Scene graph saved to " + path);
    }

    [MenuItem("Tools/Print Scene Graph")]
    public static void PrintSceneGraph()
    {
        var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        Debug.Log("Scene Graph:");
        foreach (var rootObject in rootObjects)
        {
            Debug.Log(rootObject.name);
            PrintSceneGraphRecursive(rootObject, 1);
        }
    }

    private static SceneGraphNode ExtractSceneGraphRecursive(GameObject gameObject)
    {
        SceneGraphNode node = new SceneGraphNode
        {
            name = gameObject.name,
            children = new List<SceneGraphNode>()
        };

        foreach (Transform childTransform in gameObject.transform)
        {
            SceneGraphNode childNode = ExtractSceneGraphRecursive(childTransform.gameObject);
            node.children.Add(childNode);
        }

        return node;
    }

    private static void PrintSceneGraphRecursive(GameObject gameObject, int depth)
    {
        foreach (Transform childTransform in gameObject.transform)
        {
            string indent = new string(' ', depth * 2);
            Debug.Log($"{indent}- {childTransform.name}");
            PrintSceneGraphRecursive(childTransform.gameObject, depth + 1);
        }
    }
}

[System.Serializable]
public class SceneGraphData
{
    public List<SceneGraphNode> sceneGraphNodes;
}

[System.Serializable]
public class SceneGraphNode
{
    public string name;
    public List<SceneGraphNode> children;
}
