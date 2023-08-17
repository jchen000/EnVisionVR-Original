using UnityEngine;
using System.Threading.Tasks;
//using System.Collections;
//using System.Collections.Generic;
//using System.Runtime.InteropServices.ComTypes;

public class RaycastReadObject : MonoBehaviour
{
    CameraFieldOfView cameraFieldOfView;
    CameraController cameraController;
    bool IsCheckFoV = false;
    private bool prevPrimaryButtonState_ = false;
    private bool rayReadTrigger = false;
    private float timer = 0f; // Timer to track the elapsed time
    public Transform controller;
    public float maxDistance = 10f;
    public float maxInterval = 3f;
    private AudioSource audioSource;

    //public bool highlight = false;
    //private bool priorHighlight = false;
    //public bool Guideline = false;
    //private bool priorGuidline = false;
    //public bool dynamicScanning = false;
    //public Color highlightColor = Color.green;
    //private Color priorHighlighColor = Color.white;
    //public Color guidelineColor = Color.red;
    //private Color priorGuidlineColor = Color.white;
    //public float forwardFactor = 0.5f;
    //private float priorForwardFactor = 0.5f;
    //public float radius = 0.25f;
    //private float priorRadius = 0.25f;

    void Start()
    {
        cameraFieldOfView = GetComponent<CameraFieldOfView>();
        cameraController = GetComponent<CameraController>();
        // Check if an AudioSource component is already attached
        if (audioSource == null)
        {
            // Create and attach a new AudioSource component
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Load the audio clip from the Resources folder
        AudioClip clip = Resources.Load<AudioClip>("beep");

        // Set the audio clip for the repeating sound
        audioSource.clip = clip;

        //GameObject[] allObjects = FindObjectsOfType<GameObject>();
        //foreach (GameObject obj in allObjects)
        //{
        //    if (obj.activeInHierarchy && obj.isSalience())
        //    {
        //        AddContours contours = obj.AddComponent<AddContours>();
        //    }
        //}
    }

    private void Update()
    {        
        if (cameraFieldOfView.leftprimaryButtonDown && !prevPrimaryButtonState_)
        {
            Vector3 controllerDirection = controller.forward;
            RaycastHit hit;
            bool hasHit = Physics.Raycast(controller.position, controllerDirection, out hit, maxDistance);
            string hitobjname = hit.transform.name;
            if (hitobjname == "PotionBottle_1")
                hitobjname = "Purple Potion Bottle";
            if (hitobjname == "PotionBottle_2")
                hitobjname = "Green Potion Bottle";
            cameraFieldOfView.SpeakText(hitobjname.Replace("_", " "));

      //      GameObject[] allObjects = FindObjectsOfType<GameObject>();
	     //   foreach (GameObject obj in allObjects)
	     //   {
	     //       if (obj.activeInHierarchy && obj.isSalience())
	     //       {
	     //           AddContours contours = obj.GetComponent<AddContours>();
	     //           if (contours == null)
	     //           {
	     //               contours = obj.AddComponent<AddContours>();

	     //           }
	     //           contours.whetherHighlighted = highlight;
	     //           contours.whetherLink = Guideline;
	     //           contours.color = highlightColor;
	     //           contours.guidlineColor = guidelineColor;
		    //        contours.forwardFactor = forwardFactor;
		    //        contours.radius = radius;
	     //       }
	     //   }
	     //   priorHighlight = highlight;
	     //   priorGuidline = Guideline;
	     //   priorHighlighColor = highlightColor;
	     //   priorGuidlineColor = guidelineColor;
		    //priorForwardFactor = forwardFactor;
		    //priorRadius = radius;

            rayReadTrigger = true;
        }

        if (cameraFieldOfView.lefttriggerButtonDown)
        {                
            timer += Time.deltaTime;

            // Calculate the direction from the controller
            Vector3 controllerDirection = controller.forward;

            RaycastHit hit;
            bool hasHit = Physics.Raycast(controller.position, controllerDirection, out hit, maxDistance);
            
            float normalizedDistance = hit.distance / maxDistance;
            float timeInterval = Mathf.Lerp(0.2f, maxInterval, normalizedDistance);

            // Check if the timer has reached the interval
            if (timer >= timeInterval)
            {
                // Reset the timer
                timer = 0f;

                // Play the sound
                audioSource.Play();
            }
           
        }
        prevPrimaryButtonState_ = cameraFieldOfView.leftprimaryButtonDown;
    }

    public void TriggerCheckFoV()
    {
        IsCheckFoV = true;
    }



    public void OnCheckFieldOfViewButtonClick()
    {
        if (IsCheckFoV)
        {
            // Disable camera movement while the button is being clicked
            cameraController.DisableCameraMovement();

            // Check objects in the camera's field of view
            cameraFieldOfView.CheckObjectsInFieldOfView();

            // Re-enable camera movement after the button click is handled
            cameraController.EnableCameraMovement();

            IsCheckFoV = false;
        }
    }
}
