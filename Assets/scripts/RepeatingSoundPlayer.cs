using System.Media;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class RepeatingSoundPlayer : MonoBehaviour
{
    private Transform virtualObjectTransform;
    public Transform controller;
    public float maxDistance = 10f;
    public float maxInterval = 3f;
    private AudioSource audioSource;
    private float timer = 0f; // Timer to track the elapsed time
    private bool isTriggered = false;
    public InputHelpers.Button secondaryButton;
    public bool secondaryButtonDown;
    public XRController rightHand;
    private CameraFieldOfView camerafieldofview;

    private void Start()
    {
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

        camerafieldofview = GameObject.Find("CameraController").GetComponent<CameraFieldOfView>();
    }

    //public void UpdatePosition(Transform virtualObject)
    public void Update()
    {
        secondaryButtonDown = false;
        rightHand.inputDevice.IsPressed(secondaryButton, out secondaryButtonDown);
        if (isTriggered)
        {
            Debug.Log("Playing tone for localization...");
            timer += Time.deltaTime;

            // Calculate the direction from the controller
            Vector3 controllerDirection = controller.forward;

            RaycastHit hit;
            bool hasHit = Physics.Raycast(controller.position, controllerDirection, out hit, maxDistance);
            //Debug.Log("Hit.Transform:" + hit.transform);
            //.Log("Virtual Object Transform:" + virtualObjectTransform);
            //if (hasHit && hit.transform == virtualObjectTransform)
            //{
            float distanceToObject = (controller.position - virtualObjectTransform.position).magnitude;
            //float normalizedDistance = hit.distance / maxDistance;
            float normalizedDistance = distanceToObject / maxDistance;
            float timeInterval = Mathf.Lerp(0.2f, maxInterval, normalizedDistance);

            // Check if the timer has reached the interval
            if (timer >= timeInterval)
            {
                // Reset the timer
                timer = 0f;

                // Play the sound
                audioSource.Play();
            }
            //}
            if (secondaryButtonDown)
            {
                Debug.Log("Secondary button pressed!");
                camerafieldofview.localizationMode = false;
                DeactivateBeep();
            }
        }
        else
        {

        }
    }

    public void TriggerBeep(Transform ObjectToLocateTransform)
    {
        if (ObjectToLocateTransform == null)
        {
            Debug.LogError("Beeping Virtual Object not assigned!");
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
            isTriggered = true;
            virtualObjectTransform = ObjectToLocateTransform;
        }
    }

    public void DeactivateBeep()
    {
        isTriggered= false;
    }

}
