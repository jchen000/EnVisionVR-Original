using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // Adjust the speed of camera movement
    public float movementSpeed = 5f;

    void Update()
    {
        // Move the camera based on arrow key input
        float horizontalMovement = Input.GetAxis("Horizontal");
        float verticalMovement = Input.GetAxis("Vertical");

        // Calculate the movement direction
        Vector3 movementDirection = new Vector3(horizontalMovement, 0f, verticalMovement);

        // Apply the movement to the camera's position
        transform.Translate(movementDirection * movementSpeed * Time.deltaTime);
    }
}
