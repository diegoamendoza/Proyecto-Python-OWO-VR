using System.Diagnostics;
using System.IO;
using UnityEngine;

public class ExecutePythonSemilla : MonoBehaviour
{
    private string pythonScriptPath;
    private int selectedCameraIndex = 0; 

    void Start()
    {
        
        selectedCameraIndex = PlayerPrefs.GetInt("CameraIndex", 0);

        
        pythonScriptPath = Path.Combine(Application.streamingAssetsPath, "MediaPipeSemilla.py");

        
        RunPythonScript(pythonScriptPath, selectedCameraIndex);
    }

    void RunPythonScript(string scriptPath, int cameraIndex)
    {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c start python \"{pythonScriptPath}\" {cameraIndex}", 
                UseShellExecute = true, 
                CreateNoWindow = false  
            };

            Process process = Process.Start(startInfo);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("Error al ejecutar el script de Python: " + e.Message);
        }
    }
}
