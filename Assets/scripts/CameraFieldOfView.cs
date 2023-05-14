using UnityEngine;

public class CameraFieldOfView : MonoBehaviour
{
    Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main; // Assuming you want to use the main camera in the scene
    }

    void Update()
    {
        // Update any other logic or behavior here
    }

    public void CheckObjectsInFieldOfView()
    {
        GameObject[] objectsInScene = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in objectsInScene)
        {
            // Get the object's transform component to retrieve its position
            Transform objTransform = obj.transform;
            Vector3 objPosition = objTransform.position;

            // Check if the object is within the camera's field of view
            if (IsObjectVisible(objPosition, mainCamera.transform.position, mainCamera.transform.rotation, mainCamera.fieldOfView))
            {
                // The object is within the camera's field of view
                // Do something with the object's positional information
                Debug.Log(obj.name + " is within the field of view. Position: " + objPosition);
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
}
