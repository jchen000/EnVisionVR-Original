using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SceneIntroduction : MonoBehaviour
{
    CameraFieldOfView cameraFieldOfView;
    CameraController cameraController;
    public string csvFilePath;  // Path to the CSV file
    private Camera mainCamera;
    private Dictionary<string, int> headerIndices;
    private List<string[]> csvData;
    private bool prevSecondaryButtonState_ = false;
    private string description;
    private string cameraAnchor;

    void Start()
    {
        cameraFieldOfView = GetComponent<CameraFieldOfView>();
        cameraController = GetComponent<CameraController>();
        mainCamera = Camera.main;

        // Parse the CSV file
        ParseCSV();

        // Check for specific values and print the description
        CheckAndPrintDescription(-3.47f, 1.2f, 0f, 120f);      

    }

    private void Update()
    {
        if (cameraFieldOfView.leftsecondaryButtonDown && !prevSecondaryButtonState_)
        {
            Vector3 cameraPosition = mainCamera.transform.position;
            Vector3 cameraRotation = mainCamera.transform.eulerAngles;
            CheckAndPrintDescription(cameraPosition.x, cameraPosition.y, cameraPosition.z, cameraRotation.y);
        }
        prevSecondaryButtonState_ = cameraFieldOfView.leftsecondaryButtonDown;
    }

    void ParseCSV()
    {
        csvData = new List<string[]>();

        // Read the CSV file
        using (StreamReader reader = new StreamReader(csvFilePath))
        {
            // Read header line
            string headerLine = reader.ReadLine();
            if (headerLine != null)
            {
                string[] headers = headerLine.Split(',');
                headerIndices = new Dictionary<string, int>();

                // Store the indices of each header
                for (int i = 0; i < headers.Length; i++)
                {
                    headerIndices[headers[i]] = i;
                }
            }

            // Read data lines
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    string[] values = line.Split(',');
                    csvData.Add(values);
                }
            }
        }
    }

    void CheckAndPrintDescription(float posX, float posY, float posZ, float rotY)
    {
        int anchorIndex = headerIndices["Camera Anchor"];
        int posXIndex = headerIndices["PositionX"];
        int posYIndex = headerIndices["PositionY"];
        int posZIndex = headerIndices["PositionZ"];
        int rotYIndex = headerIndices["RotationY"];
        int descriptionIndex = headerIndices["Description"];
        double prev_diff = 100000000f;

        // Iterate through the CSV data
        for (int i = 0; i < csvData.Count; i++)
        {
            string[] row = csvData[i];
            float.TryParse(row[posXIndex], out float x);
            float.TryParse(row[posYIndex], out float y);
            float.TryParse(row[posZIndex], out float z);
            float.TryParse(row[rotYIndex], out float rot);
            var difference = (rotY - rot) * (rotY - rot) + 0.4 * ((posX - x) * (posX - x) + (posY - y) * (posY - y) + (posZ - z) * (posZ - z));
            if (difference < prev_diff)
            {
                description = row[descriptionIndex];
                cameraAnchor = row[anchorIndex];
            }

            prev_diff = difference;
        }
        Debug.Log("Matching Camera Anchor:" + cameraAnchor);
        cameraFieldOfView.SpeakText(description);
    }

    void PrintCameraTransform()
    {
        Vector3 cameraPosition = mainCamera.transform.position;
        Vector3 cameraRotation = mainCamera.transform.eulerAngles;

        Debug.Log("Camera Position - X: " + cameraPosition.x + ", Y: " + cameraPosition.y + ", Z: " + cameraPosition.z);
        Debug.Log("Camera Rotation - X: " + cameraRotation.x + ", Y: " + cameraRotation.y + ", Z: " + cameraRotation.z);
    }
}
