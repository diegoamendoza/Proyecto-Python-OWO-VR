"""
MediaPipeUnitySkinetic
----------------------
Script que utiliza MediaPipe para detectar las manos a partir de la cámara, extraer información de la posición del pulso y determinar gestos para disparar animaciones. 
Los datos se envían a Unity mediante una conexión TCP.

Requisitos:
- OpenCV (cv2)
- MediaPipe
- Conexión TCP activa con Unity (IP: 127.0.0.1, puerto: 12345)
"""

import cv2
import mediapipe as mp
import socket
import time

# Configuración del host y puerto de Unity
HOST = '127.0.0.1'
PORT = 12345

def setup_connection(host: str, port: int) -> socket.socket:
    """
    Establece la conexión TCP con Unity.
    
    Args:
        host (str): Dirección IP del servidor Unity.
        port (int): Puerto en el que escucha Unity.
    
    Returns:
        socket.socket: Socket cliente conectado a Unity.
    
    Si ocurre algún error durante la conexión, se cierra el script.
    """
    try:
        client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        client_socket.connect((host, port))
        print("Conexión establecida con Unity.")
        return client_socket
    except Exception as e:
        print(f"Error al conectar con Unity: {e}")
        exit()

def setup_mediapipe():
    """
    Inicializa MediaPipe Hands y la utilidad de dibujo.
    
    Returns:
        hands: Objeto para la detección y seguimiento de manos.
        mp_drawing: Utilidad para dibujar landmarks en la imagen.
    """
    mp_hands = mp.solutions.hands
    hands = mp_hands.Hands(min_detection_confidence=0.5, min_tracking_confidence=0.5)
    mp_drawing = mp.solutions.drawing_utils
    return hands, mp_drawing

def open_camera(index: int = 0) -> cv2.VideoCapture:
    """
    Abre la cámara especificada.
    
    Args:
        index (int): Índice de la cámara (por defecto 0).
    
    Returns:
        cv2.VideoCapture: Objeto de captura de video.
    
    Si no se puede abrir la cámara, se cierra el socket y finaliza el script.
    """
    cap = cv2.VideoCapture(index)
    if not cap.isOpened():
        print("Error al abrir la cámara.")
        exit()
    print("Cámara abierta correctamente.")
    return cap

def main():
    # Establece la conexión con Unity y configura MediaPipe
    client_socket = setup_connection(HOST, PORT)
    hands, mp_drawing = setup_mediapipe()
    cap = open_camera()

    # Variables para evitar enviar mensajes duplicados
    last_position_message = None
    last_trigger_message = None

    try:
        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                print("Error al leer el frame de la cámara.")
                break

            # Convierte el frame a RGB para procesarlo con MediaPipe
            frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            results = hands.process(frame_rgb)

            if results.multi_hand_landmarks:
                triggers = []  # Lista para almacenar mensajes de animación
                for hand_landmarks in results.multi_hand_landmarks:
                    # Dibuja los landmarks y conexiones en el frame
                    mp_drawing.draw_landmarks(frame, hand_landmarks, mp.solutions.hands.HAND_CONNECTIONS)

                    # Obtiene la posición de la muñeca (landmark 0)
                    wrist = hand_landmarks.landmark[0]
                    wrist_x = wrist.x
                    wrist_y = wrist.y

                    # Crea el mensaje de posición con la información de la muñeca
                    position_message = f"Position x:{wrist_x:.3f} y:{wrist_y:.3f} z:0.000"
                    # Envia el mensaje si ha cambiado respecto al último enviado
                    if position_message != last_position_message:
                        try:
                            client_socket.sendall(position_message.encode())
                            last_position_message = position_message
                        except Exception as e:
                            print(f"Error al enviar posición a Unity: {e}")
                            break

                    # Evalúa las condiciones de los landmarks para determinar animaciones
                    # Se comparan las posiciones Y de determinados landmarks para cada dedo
                    if hand_landmarks.landmark[4].y > hand_landmarks.landmark[0].y:
                        triggers.append("Animation 1")
                    if hand_landmarks.landmark[8].y > hand_landmarks.landmark[5].y:
                        triggers.append("Animation 2")
                    if hand_landmarks.landmark[12].y > hand_landmarks.landmark[9].y:
                        triggers.append("Animation 3")
                    if hand_landmarks.landmark[16].y > hand_landmarks.landmark[13].y:
                        triggers.append("Animation 4")
                    if hand_landmarks.landmark[20].y > hand_landmarks.landmark[17].y:
                        triggers.append("Animation 5")

                # Envía los mensajes de animación si existen y son diferentes al último enviado
                if triggers:
                    trigger_message = ";".join(triggers)
                    if trigger_message != last_trigger_message:
                        try:
                            client_socket.sendall(trigger_message.encode())
                            last_trigger_message = trigger_message
                            time.sleep(0.05)  # Pequeña pausa para evitar saturar la conexión
                        except Exception as e:
                            print(f"Error al enviar animaciones a Unity: {e}")
                            break

            # Muestra la imagen con los landmarks dibujados
            cv2.imshow('MediaPipe Hands', frame)

            # Permite salir del bucle presionando la tecla 'q'
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break

    except KeyboardInterrupt:
        print("Interrupción manual, cerrando script de Python.")
    except Exception as e:
        print(f"Error durante la ejecución del script: {e}")
    finally:
        # Libera la cámara, cierra las ventanas y finaliza la conexión con Unity
        cap.release()
        cv2.destroyAllWindows()
        client_socket.close()
        print("Conexión de Python cerrada.")

if __name__ == "__main__":
    main()
