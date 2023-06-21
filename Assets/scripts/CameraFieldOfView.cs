using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;

public class CameraFieldOfView : MonoBehaviour
{
    Camera mainCamera;
    Dictionary<string, float> importanceValues;
    //Dictionary<string, string> descriptionDict;
    // private SpeechSynthesizer synthesizer;
    SpeechSynthesizer synthesizer;
    private AudioSource audioSource;
    public bool localizationMode = false;
    //public bool objectDescriptionMode = false;
    // private AudioSource audioSource;
    public float volume = 1f;
    public float spatialBlend = 1f;
    private SpatialSoundController soundController;
    private RepeatingSoundPlayer soundPlayer;
    // Add a private variable to store the most recently spoken object
    private GameObject recentlySpokenObject;

    public XRController rightHand;
    //public XRController leftHand;
    public InputHelpers.Button primaryButton;
    public InputHelpers.Button secondaryButton;
    public InputHelpers.Button triggerButton;
    public bool primaryButtonDown;
    public bool secondaryButtonDown;
    //public bool leftprimaryButtonDown;
    //public bool leftsecondaryButtonDown;
    public bool triggerButtonDown;
    //private bool prevTriggerButtonState_ = false;
    //private RepeatingSoundPlayer repeatingSoundPlayer;
    private float timer = 0f; // Timer to track the elapsed time
    //ButtonCheckFieldOfView buttoncheckfieldofview;


    void Start()
    {
        mainCamera = Camera.main;
        LoadImportanceValues();

        //buttoncheckfieldofview = GetComponent<ButtonCheckFieldOfView>();

        // Create SpeechConfig instance
        SpeechConfig speechConfig = SpeechConfig.FromSubscription("4de1d19d8bfe4fae9f46a2a3e848d548", "uksouth");

        // Create SpeechSynthesizer instance
        synthesizer = new SpeechSynthesizer(speechConfig);

        // Get the AudioSource component attached to the object
        audioSource = GetComponent<AudioSource>();

        soundController = GetComponent<SpatialSoundController>();

        //soundPlayer = GetComponent<RepeatingSoundPlayer>();
        soundPlayer = GameObject.Find("BeepAudioSource").GetComponent<RepeatingSoundPlayer>();


        //repeatingSoundPlayer = GetComponent<RepeatingSoundPlayer>();

    }

    async void Update()
    {
        // Check if the button on the right hand controller is pressed
        primaryButtonDown = false;//OVRInput.Get(OVRInput.Button.One);
        rightHand.inputDevice.IsPressed(primaryButton, out primaryButtonDown);
        secondaryButtonDown= false;
        rightHand.inputDevice.IsPressed(secondaryButton, out secondaryButtonDown);
        triggerButtonDown = false;
        rightHand.inputDevice.IsPressed(triggerButton, out triggerButtonDown);
        //leftprimaryButtonDown = false;
        //leftHand.inputDevice.IsPressed(primaryButton, out leftprimaryButtonDown);
        //if(leftprimaryButtonDown)
        //{
        //    Debug.Log("Left primary button pressed!");
        //}
        //leftsecondaryButtonDown = false;
        //leftHand.inputDevice.IsPressed(secondaryButton, out leftsecondaryButtonDown);

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

        // Print the three objects with the highest new importance values
        for (int i = 0; i < Mathf.Min(3, objectsInFieldOfView.Count); i++)
        {
            GameObject obj = objectsInFieldOfView[i];
            if (GetImportanceValue(obj.name) > 0 && localizationMode==false)
            {
                string message = obj.name;
                string messageFull = obj.name + " is within the field of view. Position: " + obj.transform.position + " Importance: " + GetImportanceValue(obj.name);
                Debug.Log(messageFull);
                SpeakText(message);

                // Store the most recently spoken object
                recentlySpokenObject = obj;
                
                // Delay before playing the audio clip
                float delay = 2.0f; // Adjust the delay time as needed
                await Task.Delay((int)(delay * 800));

                // Play the audio clip at the object's position
                string path = obj.name;
                Transform currentTransform = obj.transform;

                while (currentTransform.parent != null)
                {
                    currentTransform = currentTransform.parent;
                    path = currentTransform.name + "/" + path;
                }
                Debug.Log("Object path:" + path);
                Debug.Log("Directory:" + currentTransform.name);
                if (currentTransform.name == "Interactables")
                {
                    soundController.PlayAudioClip("Fantasy", obj.transform.position);
                }
                else if(currentTransform.name == "Interior")
                {
                    soundController.PlayAudioClip("Magic Spell", obj.transform.position);
                }
                else if (currentTransform.name == "Potions")
                {
                    soundController.PlayAudioClip("Magic", obj.transform.position);
                }
                else if (currentTransform.name == "Exterior")
                {
                    soundController.PlayAudioClip("Confirm", obj.transform.position);
                }
                else
                {
                    soundController.PlayAudioClip("Sweet Notification", obj.transform.position);
                }
        
                string objname = objectsInFieldOfView[i].name;
                Debug.Log("Sound of " + objname);

                await Task.Delay((int)(delay * 500));

                // Check if the button on the right hand controller is pressed
                //objectButtonDown = OVRInput.Get(OVRInput.Button.Two);
                //Debug.Log("OVRInput.Get Object Button state: " + objectButtonDown);

                //rightHand.inputDevice.IsPressed(objectButton, out objectButtonDown);
                //Debug.Log("rightHand.inputDevice Object Button state: " + objectButtonDown);

                //rightHand.inputDevice.IsPressed(objectButton, out objectButtonDown);
                //Debug.Log("Button state: " + objectButtonDown);
                if (primaryButtonDown)
                // if (XRControllerRightButtonPressed())
                {
                    string selectMessage = message + " Selected!";
                    SpeakText(selectMessage);
                    Debug.Log(message + " Selected!");
                    if (recentlySpokenObject != null)
                    {
                        Debug.Log("Playing sound to indicate distance between controller and virtual object...");
                        localizationMode = true;
                        soundPlayer.TriggerBeep(recentlySpokenObject.transform);
                        if (secondaryButtonDown)
                        {
                            Debug.Log("Secondary button pressed!");
                            localizationMode = false;
                            soundPlayer.DeactivateBeep();
                        }
                        //soundPlayer.UpdatePosition(recentlySpokenObject.transform);
                        //UpdatePosition(recentlySpokenObject.transform);
                    }
                }

                //else if (leftprimaryButtonDown)
                //{
        
                //    string describeMessage = "Describing " + message;
                //    SpeakText(describeMessage);
                //    Debug.Log("Describing " + message);
                //    if (recentlySpokenObject != null)
                //    {
                //        Debug.Log("Describing object from importance json file");
                //        objectDescriptionMode = true;
                //        Debug.Log("GetObjectDescription(recentlySpokenObject.name):" + GetObjectDescription(recentlySpokenObject.name));
                //        SpeakText(GetObjectDescription(recentlySpokenObject.name));
                //        if (secondaryButtonDown)
                //        {
                //            objectDescriptionMode = false;
                //        }
                //    }
                //}

                await Task.Delay((int)(delay * 300));

            }

            
        }
        
        await Task.Delay((int)(500f));

        // string description = "In this 3D virtual reality scene, you are standing in a room with a desk. On the desk, there is a piece of paper with instructions, a book holder, and a key. The instructions paper on the desk has a 3D image of a treasure map, hinting at an adventurous journey ahead. As you explore the room, you notice a bookshelf filled with various books, some of which might contain clues or information about the adventure. The scene is set in a cartoonish, computer-generated world, inviting you to embark on an exciting quest using the objects at hand.";

        // SpeakText(description);

    }

    //private void HandleRightControllerButtonPress()
    //{
        //if (recentlySpokenObject != null)
        //{
            //// Play sound to indicate distance between controller and virtual object
            ////soundPlayer.UpdatePosition(recentlySpokenObject.transform);
            //UpdatePosition(recentlySpokenObject.transform);
        //}
    //}

    //private bool XRControllerRightButtonPressed()
    //{
    //    bool buttonpress = Input.GetButtonDown("Oculus_CrossPlatform_SecondaryIndexTrigger");
    //    Debug.Log("Button status:"+buttonpress);
    //    // Replace "YOUR_RIGHT_CONTROLLER_BUTTON" with the actual button you want to use
    //    // Check the Unity Input documentation for the appropriate button names
    //    return Input.GetButtonDown("PrimaryButton");
    //}


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
            SceneGraph sceneGraphContent = JsonConvert.DeserializeObject<SceneGraph>(jsonContent);

            // Parse the JSON string into SceneData object
            //SceneGraph sceneGraph = JsonUtility.FromJson<SceneGraph>(jsonContent);
            //if (sceneGraphImportance != null)
            if (sceneGraphContent != null)
            {
                importanceValues = new Dictionary<string, float>();
                //descriptionDict = new Dictionary<string, string>();
                // Traverse the scene graph and extract the importance values
                //TraverseSceneGraph(sceneGraphImportance.children);
                TraverseSceneGraph(sceneGraphContent.children);

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
                //Debug.Log("Child_name:" + child.name);
                //Debug.Log("Child_type:" + child.type);
                //Debug.Log("Child_description:" + child.description);

                if (!importanceValues.ContainsKey(child.name))
                {
                    importanceValues.Add(child.name, child.importance.Value);
                }
                else
                {
                    Debug.LogWarning("Duplicate entry found for object: " + child.name);
                }
            }
            //else
            //{
            //    Debug.Log("Child_name:" + child.name);
            //    Debug.Log("Child_type:" + child.type);
            //    Debug.Log("Child_description:" + child.description);
            //}

            //if (!descriptionDict.ContainsKey(child.name) && child.description is string)
            //{
            //    Debug.Log("I am HERE!");
            //    Debug.Log("Childname:" + child.name);
            //    Debug.Log("Child_description:" + child.description);
            //    descriptionDict.Add(child.name, child.description);
            //}

            //if (child.description != null)
            //{
            //    Debug.Log("I am here!");
            //    if (!descriptionDict.ContainsKey(child.name))
            //    {
            //        descriptionDict.Add(child.name, child.description);
            //        Debug.Log("Dictionary Add:" + child.name + child.description);
            //    }
            //    else
            //    {
            //        Debug.LogWarning("Duplicate entry found for object: " + child.name);
            //    }
            //}

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

    //string GetObjectDescription(string objectName)
    //{
    //    if (descriptionDict.ContainsKey(objectName))
    //    {
    //        return descriptionDict[objectName];
    //    }

    //    Debug.LogWarning("Description not found for object: " + objectName);
    //    return "";
    //}
}

[System.Serializable]
public class SceneGraph
{
public string name;
public string type;
public List<SceneGraph> children;
public List<ComponentData> components;
public float? importance;
public string description;
}

[System.Serializable]
public class ComponentData
{
public string type;
// Add other component properties as needed
}
