using System.Diagnostics;
using UnityEngine;

public class ExecutePythonScript : MonoBehaviour
{
    // Enumeración para seleccionar el script de Python desde el Inspector de Unity
    public enum PythonScriptOptions
    {
        MediaPipeForHandInputWithTriggerController,
        MediaPipeForHandInputController,
        MediaPipeForUnitySkinetic
    }

    // Variable pública para seleccionar el script desde el Inspector
    public PythonScriptOptions selectedScript = PythonScriptOptions.MediaPipeForHandInputWithTriggerController;
    private int selectedCameraIndex = 0;

    void Start()
    {
        // Obtener el índice de la cámara almacenado en PlayerPrefs
        selectedCameraIndex = PlayerPrefs.GetInt("Camera", 0);

        // Ejecutar el script de Python con la cámara seleccionada
        RunPythonScript(selectedCameraIndex);
    }

    void RunPythonScript(int cameraIndex)
    {
        try
        {
            // Determinar el nombre del script según la selección en el Inspector
            string scriptName = "";
            switch(selectedScript)
            {
                case PythonScriptOptions.MediaPipeForHandInputWithTriggerController:
                    scriptName = "MediaPipeForHandInputWithTriggerController.py";
                    break;
                case PythonScriptOptions.MediaPipeForHandInputController:
                    scriptName = "MediaPipeForHandInputController.py";
                    break;
                case PythonScriptOptions.MediaPipeForUnitySkinetic:
                    scriptName = "MediaPipeUnitySkinetic.py";
                    break;
            }

            // Construir la ruta completa del script dentro de la carpeta StreamingAssets
            string pythonScriptPath = System.IO.Path.Combine(Application.streamingAssetsPath, scriptName);

            // Configurar el proceso para ejecutar el script de Python
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe", // Ejecutar mediante la terminal de Windows
                Arguments = $"/c start python \"{pythonScriptPath}\" {cameraIndex}", // Pasar el índice de la cámara como argumento
                UseShellExecute = true, // Permitir la ejecución en la terminal
                CreateNoWindow = false // Mostrar la ventana de la terminal
            };

            // Iniciar el proceso
            Process process = Process.Start(startInfo);
        }
        catch (System.Exception e)
        {
            // Capturar y mostrar errores en la consola de Unity
            UnityEngine.Debug.LogError("Error al ejecutar el script de Python: " + e.Message);
        }
    }
}
