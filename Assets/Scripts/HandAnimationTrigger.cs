using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class HandAnimationTrigger : MonoBehaviour
{
    private TcpListener tcpListener;
    private TcpClient tcpClient;
    private NetworkStream stream;
    private byte[] data = new byte[1024];

    public Animator animator; 
    public Text connectionStatusText; 
    public Transform player; 
    public float movementSpeed = 1.0f; 

    private bool isPythonConnected = false; 
    private StringBuilder messageBuffer = new StringBuilder(); 
    public float lerpTime;

    void Start()
    {
        RunPythonScript();
        StartListening();
        UpdateConnectionStatus();
    }

   

    void RunPythonScript()
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

    void StartListening()
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

    void OnClientConnect(IAsyncResult result)
    {
        try
        {
            tcpClient = tcpListener.EndAcceptTcpClient(result);
            stream = tcpClient.GetStream();

            isPythonConnected = true;
            UnityMainThreadDispatcher.Enqueue(UpdateConnectionStatus);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Error al conectar con el cliente: " + ex.Message);
        }
    }

    void UpdateConnectionStatus()
    {
        if (connectionStatusText != null)
        {
            connectionStatusText.text = isPythonConnected ?
                "Python connection state: Connected" :
                "Python connection state: Disconnected";
        }
    }

    void Update()
    {
        if (stream != null && stream.DataAvailable)
        {
            int bytesRead = stream.Read(data, 0, data.Length);
            string receivedMessage = Encoding.ASCII.GetString(data, 0, bytesRead);

            
            messageBuffer.Append(receivedMessage);

            
            ProcessBuffer();
        }
    }

    void ProcessBuffer()
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

    void ProcessAnimationMessage(string message)
    {
        string[] animations = message.Split(new[] { "Animation " }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string anim in animations)
        {
            if (anim == "1")
            {
                animator.SetTrigger("finger1");
            }
            else if (anim == "2")
            {
                animator.SetTrigger("finger2");
            }
            else if (anim == "3")
            {
                animator.SetTrigger("finger3");
            }
            else if (anim == "4")
            {
                animator.SetTrigger("finger4");
            }
            else if (anim == "5")
            {
                animator.SetTrigger("finger5");
            }
        }
    }

    void ProcessPositionMessage(string message)
    {
        try
        {
            
            string positionPart = message.Split(new[] { "Animation" }, StringSplitOptions.None)[0].Trim();

           
            if (!positionPart.Contains("x:") || !positionPart.Contains("y:") || !positionPart.Contains("z:"))
            {
                UnityEngine.Debug.LogError($"Formato de mensaje inválido: {message}");
                return;
            }

            
            string[] parts = positionPart.Split(' ');

            
            if (parts.Length >= 4)
            {
                float x = float.Parse(parts[1].Split(':')[1], System.Globalization.CultureInfo.InvariantCulture);
                float y = float.Parse(parts[2].Split(':')[1], System.Globalization.CultureInfo.InvariantCulture);
                float z = float.Parse(parts[3].Split(':')[1], System.Globalization.CultureInfo.InvariantCulture);

                
                Vector3 targetPosition = new Vector3(x, y, z) * movementSpeed;
                Vector3 fixPos = new Vector3(5, 5, 0);
                targetPosition -= fixPos;
                UnityEngine.Debug.Log(targetPosition);
                player.position = Vector3.Lerp(player.position, targetPosition, Time.deltaTime * lerpTime);
            }
            else
            {
                UnityEngine.Debug.LogError($"Datos de posición incompletos: {message}");
            }

           
            string[] animations = message.Split(new[] { "Animation " }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string anim in animations)
            {
                if (anim == "1")
                {
                    animator.SetTrigger("finger1");
                }
                else if (anim == "2")
                {
                    animator.SetTrigger("finger2");
                }
                else if (anim == "3")
                {
                    animator.SetTrigger("finger3");
                }
                else if (anim == "4")
                {
                    animator.SetTrigger("finger4");
                }
                else if (anim == "5")
                {
                    animator.SetTrigger("finger5");
                }
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error al procesar el mensaje de posición: {ex.Message} | Mensaje: {message}");
        }
    }


    void StopListening()
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

    void OnApplicationQuit()
    {
        StopListening();
    }
}
