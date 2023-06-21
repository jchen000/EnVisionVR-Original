using UnityEngine;
using System.Threading.Tasks;

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
    }

    private void Update()
    {
        //if (cameraFieldOfView.lefttriggerButtonDown && !prevTriggerButtonState_)
        
            //Vector3 controllerDirection = controller.forward;
            //RaycastHit hit;
            //bool hasHit = Physics.Raycast(controller.position, controllerDirection, out hit, maxDistance);
            //Debug.Log("Clicked on " + hit.transform.name);
            //cameraFieldOfView.SpeakText(hit.transform.name);
        
        if (cameraFieldOfView.leftprimaryButtonDown && !prevPrimaryButtonState_)
        {
            Vector3 controllerDirection = controller.forward;
            RaycastHit hit;
            bool hasHit = Physics.Raycast(controller.position, controllerDirection, out hit, maxDistance);
            //Debug.Log("Clicked on " + hit.transform.name);
            cameraFieldOfView.SpeakText(hit.transform.name);
            rayReadTrigger = true;
        }
        //if (cameraFieldOfView.leftsecondaryButtonDown)
        //{
        //    rayReadTrigger = false;
        //}
        if (cameraFieldOfView.lefttriggerButtonDown)
        {
            //Debug.Log("Pressing Left Primary Button!");
                
            timer += Time.deltaTime;

            // Calculate the direction from the controller
            Vector3 controllerDirection = controller.forward;

            RaycastHit hit;
            bool hasHit = Physics.Raycast(controller.position, controllerDirection, out hit, maxDistance);
            
            //await Task.Delay((int)(300f));
            //Debug.Log("Hit.Transform:" + hit.transform);
            //.Log("Virtual Object Transform:" + virtualObjectTransform);
            //if (hasHit && hit.transform == virtualObjectTransform)
            //{
            //float distanceToObject = (controller.position - virtualObjectTransform.position).magnitude;
            float normalizedDistance = hit.distance / maxDistance;
            //float normalizedDistance = distanceToObject / maxDistance;
            float timeInterval = Mathf.Lerp(0.1f, maxInterval, normalizedDistance);

            // Check if the timer has reached the interval
            if (timer >= timeInterval)
            {
                // Reset the timer
                timer = 0f;

                // Play the sound
                audioSource.Play();
            }
            //if (cameraFieldOfView.leftsecondaryButtonDown)
            //{
            //    Debug.Log("Left Secondary button pressed!");
            //    localizationMode = false;
            //    soundPlayer.DeactivateBeep();
            //}
            //}

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
