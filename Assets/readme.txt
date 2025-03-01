# Proyecto Gestos y Sensaciones con Unity y Python

Este repositorio integra detección de gestos mediante Python con efectos visuales y hápticos en Unity. Se utiliza **MediaPipe** para detectar la posición de la mano y el gesto de puño, y se envían estos datos en tiempo real a Unity a través de un socket TCP. En Unity, estos datos se utilizan para mover objetos, actualizar efectos visuales y disparar sensaciones hápticas mediante la librería **OWOGame**.

---

## Tabla de Contenidos

- [Características](#características)
- [Arquitectura del Proyecto](#arquitectura-del-proyecto)
- [Documentación de HandInputController.cs](#documentación-de-handinputcontrollercs)
- [Requisitos](#requisitos)
- [Instalación y Configuración](#instalación-y-configuración)
- [Uso y Demostración](#uso-y-demostración)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Contribuciones](#contribuciones)
- [Licencia](#licencia)
- [Contacto](#contacto)

---

## Características

- **Detección de Manos:** Utiliza MediaPipe para identificar la posición de la palma y detectar gestos (como el puño).
- **Comunicación en Tiempo Real:** El script de Python envía datos de posición y gestos a Unity mediante un socket TCP.
- **Efectos Visuales y Hápticos:** Los datos recibidos en Unity permiten mover objetos y activar sensaciones hápticas usando OWOGame.
- **Interactividad:** El sistema está diseñado para responder en tiempo real a la entrada de gestos, permitiendo una experiencia inmersiva.

---

## Arquitectura del Proyecto

El proyecto se compone de dos partes principales:

### 1. Scripts en C# (Unity)
- **HandInputWithTriggerController.cs:**  
  - Se encarga de establecer la conexión TCP con el script de Python.
  - Recibe datos de posición y gestos.
  - Activa sensaciones hápticas a través de OWOGame.
  
- **HandInputController.cs:**  
  - (Anteriormente conocido como *Feathers.cs*)  
  - Establece un servidor TCP para recibir datos (posición y gestos) enviados desde Python.
  - Procesa dichos datos y, según la información recibida, dispara sensaciones hápticas mediante llamadas a **OWOSensationsManager**.
  
- **PythonMaterialController.cs:**  
  - Aplica interpolación a los datos recibidos y actualiza materiales y efectos visuales.
  
- **OWOSensationsManager.cs:**  
  - Gestiona el envío de sensaciones hápticas configuradas a través de la librería OWOGame.

### 2. Script en Python
- **main.py:**  
  - Utiliza OpenCV y MediaPipe para detectar la posición de la mano y el gesto de puño.
  - Envía en tiempo real la posición (y, opcionalmente, el gesto) a Unity mediante un socket TCP.
  - Muestra una ventana interactiva con la detección de manos.

---
## HandInputWithTriggerController.cs
Esta clase se encarga de establecer una conexión TCP para recibir datos (posición y gestos) desde un script en Python y, en función de dichos datos, actualizar el vector de entrada y enviar una sensación mediante el sistema OWO. 


### Métodos Principales

- **StartListening():** Inicia el `TcpListener` en la IP local y el puerto 45678.
- **OnClientConnect(IAsyncResult result):** Callback que se ejecuta al conectar un cliente. Obtiene el stream de datos para su posterior lectura.
- **OnClientConnect():** Se activa cuando un cliente se conecta y establece el stream.
- **ParsePosition():** Procesa el mensaje de posición recibido y actualiza el vector de entrada.
- **StopListening():** Detiene el TcpListener y cierra todas las conexiones abiertas.


---
## HandInputController.cs
Esta clase establece un servidor TCP para recibir datos (posición y gestos) enviados desde Python y, según dichos datos, dispara las sensaciones hápticas correspondientes.

### Métodos Principales

- **Awake():** Se ejecuta al inicializar el objeto y establece la instancia singleton.
- **Start():** Inicia la escucha de conexiones TCP.
- **StartListening():** Configura un `TcpListener` en `127.0.0.1:45678`.
- **OnClientConnect():** Se activa cuando un cliente se conecta y establece el stream.
- **Update():** Procesa los datos recibidos en cada frame.
- **ParsePosition():** Extrae los valores de posición del mensaje.
- **DetectGestures():** Analiza los datos para activar sensaciones hápticas.
- **StopListening():** Cierra las conexiones abiertas.
- **OnApplicationQuit():** Garantiza el cierre de conexiones al salir.

---

## Requisitos

### Para el Script de Python
- **Python 3.x**
- **OpenCV:** `pip install opencv-python`
- **MediaPipe:** `pip install mediapipe`

### Para el Proyecto Unity
- **Unity 2019.4** o superior.
- **OWOGame:** Importar la librería en Unity.
- **Scripts de C#:** Colocar en `Assets/Scripts`.

---

## Instalación y Configuración

1. **Clonar el Repositorio:**
   ```bash
   git clone https://github.com/tu-usuario/tu-repositorio.git
   cd tu-repositorio
   ```
2. **Configurar Unity:**
   - Agregar los scripts en `Assets/Scripts`.
   - Crear GameObjects y asignar los scripts.
   - Configurar OWOGame.
3. **Ejecutar el Script de Python:**
   ```bash
   python main.py 0
   ```

---

## Uso y Demostración

1. **Ejecutar Unity** y la escena configurada.
2. **Ejecutar el script de Python** (`main.py`).
3. **Interacción:**
   - La cámara detecta la mano y envía la posición a Unity.
   - Unity actualiza los efectos visuales y activa sensaciones hápticas si se detecta el puño.
4. **Finalizar:**
   - Presiona `q` en la ventana de Python o `Ctrl+C` en la terminal.

---

## Estructura del Proyecto

```plaintext
tu-repositorio/
├── Assets/
│   ├── Scripts/
│   │   ├── Semilla.cs
│   │   ├── HandInputController.cs
│   │   ├── PetalController.cs
│   │   └── OWOSensationsManager.cs
├── main.py
├── README.md
└── LICENSE
```

---
