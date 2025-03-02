using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la animación de la mano a partir de los mensajes recibidos vía TCP.
/// Inicia un proceso de Python para la captura de datos y establece la comunicación con él.
/// </summary>
public class HandAnimationTrigger : MonoBehaviour
{
    #region Variables privadas

    private TcpListener tcpListener;
    private TcpClient tcpClient;
    private NetworkStream stream;
    private readonly byte[] data = new byte[1024];
    private readonly StringBuilder messageBuffer = new StringBuilder();
    private bool isPythonConnected = false;

    #endregion

    #region Variables públicas y configuraciones

    [Header("Componentes de Unity")]
    public Animator animator;
    public Text connectionStatusText;
    public Transform player;

    [Header("Configuraciones")]
    public float movementSpeed = 1.0f;
    public float lerpTime;

    #endregion

    #region Métodos de Unity

    /// <summary>
    /// Inicializa el script ejecutando el proceso de Python, iniciando la escucha de conexiones y actualizando el estado.
    /// </summary>
    private void Start()
    {
        RunPythonScript();
        StartListening();
        UpdateConnectionStatus();
    }

    /// <summary>
    /// Lee datos del stream cada frame y procesa los mensajes recibidos.
    /// </summary>
    private void Update()
    {
        if (stream != null && stream.DataAvailable)
        {
            int bytesRead = stream.Read(data, 0, data.Length);
            string receivedMessage = Encoding.ASCII.GetString(data, 0, bytesRead);
            messageBuffer.Append(receivedMessage);
            ProcessBuffer();
        }
    }

    /// <summary>
    /// Se invoca al salir de la aplicación para detener la escucha y cerrar conexiones.
    /// </summary>
    private void OnApplicationQuit()
    {
        StopListening();
    }

    #endregion

    #region Métodos de inicialización y comunicación

    /// <summary>
    /// Ejecuta el script de Python encargado de enviar datos.
    /// </summary>
    private void RunPythonScript()
    {
        string pythonScriptPath = System.IO.Path.Combine(Application.streamingAssetsPath, "MediaPipeUnity.py");
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = pythonScriptPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        try
        {
            Process process = Process.Start(startInfo);
            if (process == null)
            {
                UnityEngine.Debug.LogError("No se pudo iniciar el proceso de Python.");
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Error al ejecutar el script de Python: " + ex.Message);
        }
    }

    /// <summary>
    /// Inicia el TcpListener para aceptar conexiones en la dirección y puerto especificados.
    /// </summary>
    private void StartListening()
    {
        try
        {
            tcpListener = new TcpListener(System.Net.IPAddress.Parse("127.0.0.1"), 12345);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(OnClientConnect, tcpListener);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Error al iniciar el TcpListener: " + ex.Message);
        }
    }

    /// <summary>
    /// Callback que se ejecuta al aceptar una conexión de cliente.
    /// Establece el stream de comunicación y actualiza el estado de conexión.
    /// </summary>
    /// <param name="result">Resultado asíncrono de la conexión.</param>
    private void OnClientConnect(IAsyncResult result)
    {
        try
        {
            tcpClient = tcpListener.EndAcceptTcpClient(result);
            stream = tcpClient.GetStream();
            isPythonConnected = true;
            // Asegura que la actualización del estado se ejecute en el hilo principal.
            UnityMainThreadDispatcher.Enqueue(UpdateConnectionStatus);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Error al conectar con el cliente: " + ex.Message);
        }
    }

    /// <summary>
    /// Actualiza el texto que indica el estado de la conexión con Python.
    /// </summary>
    private void UpdateConnectionStatus()
    {
        if (connectionStatusText != null)
        {
            connectionStatusText.text = isPythonConnected
                ? "Python connection state: Connected"
                : "Python connection state: Disconnected";
        }
    }

    /// <summary>
    /// Detiene la escucha y cierra las conexiones de red.
    /// </summary>
    private void StopListening()
    {
        if (tcpListener != null)
        {
            tcpListener.Stop();
            tcpListener = null;
        }

        if (tcpClient != null)
        {
            tcpClient.Close();
            tcpClient = null;
        }

        if (stream != null)
        {
            stream.Close();
            stream = null;
        }

        isPythonConnected = false;
        UpdateConnectionStatus();
    }

    #endregion

    #region Procesamiento de mensajes

    /// <summary>
    /// Procesa el contenido del buffer, separando mensajes mediante el delimitador "|END|".
    /// </summary>
    private void ProcessBuffer()
    {
        string[] messages = messageBuffer.ToString().Split(new string[] { "|END|" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string message in messages)
        {
            if (message.StartsWith("Position"))
            {
                ProcessPositionMessage(message);
            }
            else if (message.StartsWith("Animation"))
            {
                ProcessAnimationMessage(message);
            }
            else
            {
                UnityEngine.Debug.LogWarning("Mensaje no reconocido: " + message);
            }
        }
        messageBuffer.Clear();
    }

    /// <summary>
    /// Procesa un mensaje que contiene únicamente datos de animación.
    /// Se asume que el formato es "Animation X" (o múltiples animaciones separadas).
    /// </summary>
    /// <param name="message">Mensaje recibido con datos de animación.</param>
    private void ProcessAnimationMessage(string message)
    {
        string[] animations = message.Split(new[] { "Animation " }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string anim in animations)
        {
            TriggerAnimation(anim);
        }
    }

    /// <summary>
    /// Procesa un mensaje que contiene datos de posición y, opcionalmente, animaciones.
    /// Separa la información de posición y, si existe, la información de animación.
    /// </summary>
    /// <param name="message">Mensaje recibido con datos de posición y animación.</param>
    private void ProcessPositionMessage(string message)
    {
        try
        {
            // Separa la parte de posición de la parte de animación (si existe)
            string[] parts = message.Split(new[] { "Animation" }, StringSplitOptions.None);
            string positionPart = parts[0].Trim();

            if (!positionPart.Contains("x:") || !positionPart.Contains("y:") || !positionPart.Contains("z:"))
            {
                UnityEngine.Debug.LogError($"Formato de mensaje inválido: {message}");
                return;
            }

            string[] positionData = positionPart.Split(' ');
            if (positionData.Length >= 4)
            {
                float x = float.Parse(positionData[1].Split(':')[1], System.Globalization.CultureInfo.InvariantCulture);
                float y = float.Parse(positionData[2].Split(':')[1], System.Globalization.CultureInfo.InvariantCulture);
                float z = float.Parse(positionData[3].Split(':')[1], System.Globalization.CultureInfo.InvariantCulture);

                Vector3 targetPosition = new Vector3(x, y, z) * movementSpeed;
                // Ajuste de posición fija
                Vector3 offset = new Vector3(5, 5, 0);
                targetPosition -= offset;
                UnityEngine.Debug.Log(targetPosition);
                player.position = Vector3.Lerp(player.position, targetPosition, Time.deltaTime * lerpTime);
            }
            else
            {
                UnityEngine.Debug.LogError($"Datos de posición incompletos: {message}");
            }

            // Procesa la parte de animación, si existe
            if (parts.Length > 1)
            {
                string animationPart = parts[1];
                string[] animations = animationPart.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string anim in animations)
                {
                    TriggerAnimation(anim);
                }
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error al procesar el mensaje de posición: {ex.Message} | Mensaje: {message}");
        }
    }

    #endregion

    #region Utilidades

    /// <summary>
    /// Dispara la animación correspondiente según el identificador recibido.
    /// </summary>
    /// <param name="anim">Identificador de la animación.</param>
    private void TriggerAnimation(string anim)
    {
        switch (anim)
        {
            case "1":
                animator.SetTrigger("finger1");
                break;
            case "2":
                animator.SetTrigger("finger2");
                break;
            case "3":
                animator.SetTrigger("finger3");
                break;
            case "4":
                animator.SetTrigger("finger4");
                break;
            case "5":
                animator.SetTrigger("finger5");
                break;
            default:
                UnityEngine.Debug.LogWarning($"Animación desconocida: {anim}");
                break;
        }
    }

    #endregion
}
