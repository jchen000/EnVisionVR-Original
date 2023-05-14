using UnityEngine;

public class ButtonCheckFieldOfView : MonoBehaviour
{
    CameraFieldOfView cameraFieldOfView;
    CameraController cameraController;

    void Start()
    {
        cameraFieldOfView = GetComponent<CameraFieldOfView>();
        cameraController = GetComponent<CameraController>();
    }

    public void OnCheckFieldOfViewButtonClick()
    {
        // Disable camera movement while the button is being clicked
        cameraController.DisableCameraMovement();

        // Check objects in the camera's field of view
        cameraFieldOfView.CheckObjectsInFieldOfView();

        // Re-enable camera movement after the button click is handled
        cameraController.EnableCameraMovement();
    }
}
