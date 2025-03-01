using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CameraSelection : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    private List<string> cameraNames = new List<string>();
    public int selectedCameraIndex = 0;

    void Start()
    {
        dropdown.ClearOptions();
        WebCamDevice[] devices = WebCamTexture.devices;

        foreach (var device in devices)
        {
            cameraNames.Add(device.name);
        }

        dropdown.AddOptions(cameraNames);
        dropdown.onValueChanged.AddListener(UpdateCameraIndex);
    }

    void UpdateCameraIndex(int index)
    {
        selectedCameraIndex = index;
        PlayerPrefs.SetInt("Camera", index);
        Debug.Log("Cámara seleccionada: " + cameraNames[selectedCameraIndex]);
    }

    public void ChangeScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
