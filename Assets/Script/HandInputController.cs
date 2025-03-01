// HandInputController.cs
// Descripci�n: Esta clase establece un servidor TCP para recibir datos (posici�n y gestos)
// enviados desde Python y, seg�n dichos datos, dispara las sensaciones h�pticas correspondientes.


using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class HandInputController : MonoBehaviour
{
    // Instancia singleton para facilitar el acceso global
    public static HandInputController instance;

    [Header("Configuraci�n TCP")]
    public TcpListener tcpListener;         // Escucha las conexiones entrantes
    private TcpClient tcpClient;              // Cliente conectado
    private NetworkStream stream;             // Flujo de datos del cliente
    private byte[] data = new byte[1024];

    [Header("Interfaz de Usuario")]
    public Text connectionStatusText;         // Muestra el estado de la conexi�n

    [Header("Configuraci�n de Movimiento")]
    public Vector3 vectorInput;               // Vector de posici�n recibido desde Python
    [SerializeField] private float multiplier = 0.5f;  // Factor de multiplicaci�n para el vector
    public float forceStrength = 5.0f;
    public float movementSpeed = 5;
    public float lerpTime = 10;

    [Header("Detecci�n de Gestos")]
    public bool isFist;                     // Estado del gesto de pu�o detectado
    private bool previousFist = false;
    private bool previousLeft = false;
    private bool previousRight = false;
    private bool previousUp = false;

    private void Awake()
    {
        // Establece la instancia singleton
        instance = this;
    }

    void Start()
    {
        // Inicia la escucha de conexiones TCP
        StartListening();
    }

    /// <summary>
    /// Inicia el TcpListener en la IP local y el puerto 45678.
    /// </summary>
    void StartListening()
    {
        try
        {
            tcpListener = new TcpListener(System.Net.IPAddress.Parse("127.0.0.1"), 45678); //El puerto fu� elegido al azar, se puede colocar cualquiera disponible, pero igual se debe cambiar en el c�digo de Python.
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(OnClientConnect, tcpListener);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error al iniciar el TcpListener: " + ex.Message);
        }
    }

    /// <summary>
    /// Callback que se ejecuta cuando se conecta un cliente.
    /// </summary>
    /// <param name="result">Resultado de la conexi�n as�ncrona</param>
    void OnClientConnect(IAsyncResult result)
    {
        try
        {
            tcpClient = tcpListener.EndAcceptTcpClient(result);
            stream = tcpClient.GetStream();
            // Actualiza el estado de la conexi�n en la UI
            UpdateConnectionStatus();
        }
        catch (Exception ex)
        {
            Debug.LogError("Error al conectar con el cliente: " + ex.Message);
        }
    }

    /// <summary>
    /// Actualiza el estado de la conexi�n en la interfaz de usuario.
    /// </summary>
    void UpdateConnectionStatus()
    {
        if (connectionStatusText != null)
        {
            connectionStatusText.text = (tcpClient != null && tcpClient.Connected) ?
                "Python connection state: Connected" :
                "Python connection state: Disconnected";
        }
    }

    /// <summary>
    /// Se ejecuta en cada frame para leer y procesar los datos recibidos.
    /// </summary>
    void Update()
    {
        if (stream != null && stream.DataAvailable)
        {
            int bytesRead = stream.Read(data, 0, data.Length);
            string receivedMessage = Encoding.ASCII.GetString(data, 0, bytesRead).Trim();

            // Separa el mensaje recibido en partes usando 'Fist' como delimitador
            string[] messageParts = receivedMessage.Split(new string[] { "Fist" }, StringSplitOptions.None);

            // Procesa la posici�n si el mensaje comienza con "Position:"
            if (messageParts.Length > 0 && messageParts[0].StartsWith("Position:"))
            {
                ParsePosition(messageParts[0]);
            }

            // Detecta gestos basados en la posici�n y mensaje recibido
            DetectGestures(messageParts);
        }
    }

    /// <summary>
    /// Procesa el mensaje de posici�n y actualiza el vector de entrada.
    /// </summary>
    /// <param name="message">Mensaje en formato "Position:x,y,z"</param>
    void ParsePosition(string message)
    {
        string[] positionParts = message.Replace("Position:", "").Split(',');
        if (positionParts.Length == 3)
        {
            try
            {
                float x = float.Parse(positionParts[0], System.Globalization.CultureInfo.InvariantCulture);
                float y = float.Parse(positionParts[1], System.Globalization.CultureInfo.InvariantCulture);
                float z = float.Parse(positionParts[2], System.Globalization.CultureInfo.InvariantCulture);

                vectorInput = new Vector3(x, y, z) * multiplier;
                Debug.Log("Vector Input: " + vectorInput);
            }
            catch (FormatException ex)
            {
                Debug.LogError("Error al convertir posici�n: " + ex.Message);
            }
        }
        else
        {
            Debug.LogWarning("El mensaje de posici�n no tiene el formato esperado: " + message);
        }
    }

    /// <summary>
    /// Detecta gestos (izquierda, derecha, arriba y pu�o) y dispara las sensaciones correspondientes.
    /// </summary>
    /// <param name="messageParts">Partes del mensaje recibido</param>
    void DetectGestures(string[] messageParts)
    {
        // Detecci�n de gesto hacia la izquierda
        bool currentLeft = vectorInput.x < 0.25f;
        if (!previousLeft && currentLeft)
        {
            OWOSensationsManager.instance.TriggerLeftSens();
        }
        previousLeft = currentLeft;

        // Detecci�n de gesto hacia la derecha
        bool currentRight = vectorInput.x > 0.75f;
        if (!previousRight && currentRight)
        {
            OWOSensationsManager.instance.TriggerRightSens();
        }
        previousRight = currentRight;

        // Detecci�n de gesto hacia arriba
        bool currentUp = vectorInput.y < 0.25f;
        if (!previousUp && currentUp)
        {
            OWOSensationsManager.instance.TriggerUpSens();
        }
        previousUp = currentUp;

        // Detecci�n de pu�o
        bool previousFistState = isFist;
        if (messageParts.Length > 1 && messageParts[1] == "Detected")
        {
            isFist = true;
        }
        else
        {
            isFist = false;
        }

        if (!previousFistState && isFist)
        {
            OWOSensationsManager.instance.TriggerDownSens();
        }
    }

    /// <summary>
    /// Detiene el TcpListener y cierra la conexi�n.
    /// </summary>
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
    }

    // Se llama al salir de la aplicaci�n para cerrar conexiones de forma segura.
    void OnApplicationQuit()
    {
        StopListening();
    }
}
