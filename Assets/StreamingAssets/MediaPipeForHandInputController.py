
"""
MediaPipeForHandInputController.py
Descripción:
    Script en Python que utiliza OpenCV y MediaPipe para detectar manos y enviar la posición
    de la palma a Unity a través de un socket TCP. Se redimensiona el frame para un procesamiento
    y visualización consistentes.

Autor: [Tu Nombre]
Fecha: [Fecha]
"""

import cv2
import mediapipe as mp
import socket
import sys

# Configuración del socket
HOST = '127.0.0.1'
PORT = 45678

def get_camera_index():
    """
    Obtiene el índice de la cámara desde los argumentos de línea de comandos.
    Retorna 0 si no se proporciona argumento.
    """
    if len(sys.argv) > 1:
        try:
            return int(sys.argv[1])
        except ValueError:
            print("Error: El índice de la cámara debe ser un número entero.")
            sys.exit(1)
    return 0

def setup_socket():
    """
    Configura y retorna un socket TCP conectado a la dirección especificada.
    """
    try:
        client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        client_socket.connect((HOST, PORT))
        print("Conexión establecida con Unity.")
        return client_socket
    except Exception as e:
        print(f"Error al conectar con Unity: {e}")
        sys.exit(1)

def initialize_mediapipe():
    """
    Inicializa los módulos de MediaPipe para la detección de manos.
    Retorna el objeto 'hands' y la utilidad para dibujo de landmarks.
    """
    mp_hands = mp.solutions.hands
    hands = mp_hands.Hands(min_detection_confidence=0.5, min_tracking_confidence=0.5)
    mp_drawing = mp.solutions.drawing_utils
    return hands, mp_drawing

def main():
    camera_index = get_camera_index()
    client_socket = setup_socket()

    # Inicializar MediaPipe
    hands, mp_drawing = initialize_mediapipe()

    # Inicializar captura de video
    cap = cv2.VideoCapture(camera_index)
    if not cap.isOpened():
        print(f"Error al abrir la cámara en índice {camera_index}.")
        client_socket.close()
        sys.exit(1)
    print(f"Cámara {camera_index} abierta correctamente.")

    try:
        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                print("Error al leer el frame de la cámara.")
                break

            # Redimensionar el frame para consistencia y rendimiento
            frame_resized = cv2.resize(frame, (640, 480))
            # Convertir a RGB para MediaPipe
            frame_rgb = cv2.cvtColor(frame_resized, cv2.COLOR_BGR2RGB)

            # Procesar el frame para detectar manos
            results = hands.process(frame_rgb)

            if results.multi_hand_landmarks:
                for hand_landmarks in results.multi_hand_landmarks:
                    # Dibujar las landmarks en el frame para visualización
                    mp_drawing.draw_landmarks(frame_resized, hand_landmarks, mp.solutions.hands.HAND_CONNECTIONS)

                    # Obtener la posición de la base de la palma (landmark 0)
                    palm_base = hand_landmarks.landmark[0]
                    x = palm_base.x
                    y = -palm_base.y  # Invertir Y si es necesario para adecuar la visualización
                    z = palm_base.z

                    position_message = f"Position:{x:.3f},{y:.3f},{z:.3f}"
                    try:
                        client_socket.sendall(position_message.encode())
                        print(f"Posición enviada: {position_message}")
                    except Exception as e:
                        print(f"Error al enviar posición a Unity: {e}")
                        break

            cv2.imshow('MediaPipe Hands', frame_resized)

            # Salir si se presiona 'q'
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break

    except KeyboardInterrupt:
        print("Interrupción manual, cerrando script de Python.")
    except Exception as e:
        print(f"Error durante la ejecución del script: {e}")
    finally:
        cap.release()
        cv2.destroyAllWindows()
        client_socket.close()
        print("Conexión de Python cerrada.")

if __name__ == "__main__":
    main()
