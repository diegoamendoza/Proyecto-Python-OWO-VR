using Skinetic;
using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Feathers : MonoBehaviour
{
    public TcpListener tcpListener;
    private TcpClient tcpClient;
    private NetworkStream stream;
    private byte[] data = new byte[1024];
    public Text connectionStatusText;
    public Rigidbody targetRigidbody; // Rigidbody para aplicar fuerza o movimiento
    public float forceStrength = 5.0f; // Fuerza al detectar un puño
    public Transform plumasTransform; // Objeto para representar la posición de la mano
    public float movementSpeed = 5;
    private bool isPythonConnected = false;
    public float lerpTime = 10;
    public HapticEffect m_hapticEffect;

    void Start()
    {
        StartListening();
    }

    void StartListening()
    {
        try
        {
            tcpListener = new TcpListener(System.Net.IPAddress.Parse("127.0.0.1"), 45678);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(OnClientConnect, tcpListener);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error al iniciar el TcpListener: " + ex.Message);
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
            Debug.LogError("Error al conectar con el cliente: " + ex.Message);
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
            string receivedMessage = Encoding.ASCII.GetString(data, 0, bytesRead).Trim();

            
            string[] messageParts = receivedMessage.Split(new string[] { "Fist" }, StringSplitOptions.None);

            
            if (messageParts.Length > 0 && messageParts[0].StartsWith("Position:"))
            {
                string[] positionParts = messageParts[0].Replace("Position:", "").Split(',');
                if (positionParts.Length == 3)
                {
                    try
                    {
                        float x = float.Parse(positionParts[0], System.Globalization.CultureInfo.InvariantCulture);
                        float y = float.Parse(positionParts[1], System.Globalization.CultureInfo.InvariantCulture);
                        float z = float.Parse(positionParts[2], System.Globalization.CultureInfo.InvariantCulture);

                        Vector3 targetPosition = new Vector3(x, y, z) * movementSpeed;
                        Vector3 fixPos = new Vector3(0.4f, 0.4f, 0);
                        targetPosition -= fixPos;
                        plumasTransform.position = Vector3.Lerp(plumasTransform.position, targetPosition, Time.deltaTime * lerpTime);
                    }
                    catch (FormatException ex)
                    {
                        Debug.LogError($"Error al convertir posición: {ex.Message}. Mensaje recibido: {messageParts[0]}");
                    }
                }
                else
                {
                    Debug.LogWarning($"El mensaje de posición no tiene el formato esperado: {messageParts[0]}");
                }
            }

            
            if (messageParts.Length > 1 && messageParts[1] == "Detected")
            {
                ApplyForceToRigidbody();
            }
        }
    }

    void ApplyForceToRigidbody()
    {
        if (targetRigidbody != null)
        {
            targetRigidbody.AddForce(Vector3.up * forceStrength, ForceMode.Impulse);
            m_hapticEffect.PlayEffect();


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
    }

    void OnApplicationQuit()
    {
        StopListening();
    }

   
}
