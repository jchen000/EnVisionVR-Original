using UnityEngine;

public class ButtonCheckFieldOfView : MonoBehaviour
{
    CameraFieldOfView cameraFieldOfView;
    CameraController cameraController;
    bool IsCheckFoV = false;
    private bool prevTriggerButtonState_ = false;

    void Start()
    {
        cameraFieldOfView = GetComponent<CameraFieldOfView>();
        cameraController = GetComponent<CameraController>();
    }

    private void Update()
    {
        if (cameraFieldOfView.triggerButtonDown && !prevTriggerButtonState_)
        {
            Debug.Log("Trigger Button Pressed!");
            TriggerCheckFoV();
            // TriggerCheckFieldOfView;
        }
        //if (cameraFieldOfView.triggerButtonDown)
        //{
        //    TriggerCheckFoV();
        //}

        OnCheckFieldOfViewButtonClick();
        prevTriggerButtonState_ = cameraFieldOfView.triggerButtonDown;

        //if (IsCheckFoV)
        //{
        //    OnCheckFieldOfViewButtonClick();
        //}
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

