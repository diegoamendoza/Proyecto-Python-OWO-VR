using System.Diagnostics;
using UnityEngine;

public class ExecutePythonScriptP2 : MonoBehaviour
{
    
    private string pythonScriptPath;

    void Start()
    {

        pythonScriptPath = System.IO.Path.Combine(Application.streamingAssetsPath, "MediaPipeForFeathers.py");

       
        RunPythonScript(pythonScriptPath);
    }

    void RunPythonScript(string scriptPath)
    {
        try
        {
           
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "python", 
                Arguments = scriptPath, 
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = Process.Start(startInfo);
            //process.WaitForExit();
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("Error al ejecutar el script de Python: " + e.Message);
        }
    }
}
