using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class AssetIsolationScreenshot : EditorWindow
{
    public GameObject targetObject;
    public string outputPath = "Screenshots/";

    [MenuItem("EnVisionVR/Capture Screenshot")]


    //public static void ShowWindow()
    //{
    //    EditorWindow.GetWindow(typeof(AssetIsolationScreenshot));
    //}

    //private void OnGUI()
    //{
    //    GUILayout.Label("Asset Isolation Screenshot", EditorStyles.boldLabel);

    //    targetObject = EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true) as GameObject;
    //    outputPath = EditorGUILayout.TextField("Output Path", outputPath);

    //    if (GUILayout.Button("Capture Screenshot"))
    //    {
    //        CaptureScreenshot();
    //    }
    //}

    private static void CaptureScreenshot()
    {
        // Create a new scene for asset isolation
        //Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Instantiate the target object in the new scene
        //GameObject isolatedObject = Instantiate(targetObject);

        // Set the camera position to focus on the isolated object
        //Camera.main.transform.position = isolatedObject.transform.position - Camera.main.transform.forward * 2f;
        //Camera.main.transform.LookAt(isolatedObject.transform);

        // Capture a screenshot
        string screenshotName = "Desk_screenshot.png";
        string screenshotPath = screenshotName;
        ScreenCapture.CaptureScreenshot(screenshotPath);

        Debug.Log("Screenshot captured: " + screenshotPath);

        // Clean up the new scene
        //EditorSceneManager.CloseScene(newScene, true);
    }
}
