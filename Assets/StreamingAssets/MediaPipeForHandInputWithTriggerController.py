"""
MediaPipeForHandInputWithTriggerController.py
Descripción: Script en Python que utiliza OpenCV y MediaPipe para detectar manos,
enviar la posición de la palma y el gesto de puño a Unity mediante un socket TCP.
"""

import cv2
import mediapipe as mp
import socket
import sys

# Configuración del socket para enviar datos a Unity
HOST = '127.0.0.1'
PORT = 45678

def setup_socket():
    """
    Configura y retorna el socket cliente para la conexión con Unity.
    """
    try:
        client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        client_socket.connect((HOST, PORT))
        print("Conexión establecida con Unity.")
        return client_socket
    except Exception as e:
        print(f"Error al conectar con Unity: {e}")
        sys.exit()

def get_camera_index():
    """
    Obtiene el índice de la cámara a utilizar desde los argumentos de línea de comandos.
    """
    if len(sys.argv) > 1:
        try:
            return int(sys.argv[1])
        except ValueError:
            print("Error: El índice de la cámara debe ser un número entero.")
            sys.exit()
    else:
        return 0  # Cámara por defecto

def main():
    client_socket = setup_socket()
    camera_index = get_camera_index()

    # Inicializar MediaPipe Hands
    mp_hands = mp.solutions.hands
    hands = mp_hands.Hands(min_detection_confidence=0.5, min_tracking_confidence=0.5)
    mp_drawing = mp.solutions.drawing_utils

    cap = cv2.VideoCapture(camera_index)
    if not cap.isOpened():
        print(f"Error al abrir la cámara {camera_index}.")
        client_socket.close()
        sys.exit()

    print(f"Cámara {camera_index} abierta correctamente.")

    try:
        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                print("Error al leer el frame de la cámara.")
                break

            frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            results = hands.process(frame_rgb)

            if results.multi_hand_landmarks:
                for hand_landmarks in results.multi_hand_landmarks:
                    # Dibuja las conexiones de la mano en el frame
                    mp_drawing.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)

                    # Obtener la posición de la base de la palma (landmark 0)
                    palm_base = hand_landmarks.landmark[0]
                    x, y, z = palm_base.x, palm_base.y, palm_base.z

                    # Enviar la posición a Unity en el formato esperado
                    position_message = f"Position:{x:.3f},{y:.3f},{z:.3f}"
                    try:
                        client_socket.sendall(position_message.encode())
                        print(f"Posición enviada: {position_message}")
                    except Exception as e:
                        print(f"Error al enviar posición a Unity: {e}")
                        break

                    # Función para detectar si la mano está cerrada en un puño
                    def is_fist(hand):
                        thumb = hand.landmark[4].y < hand.landmark[3].y
                        index = hand.landmark[8].y > hand.landmark[6].y
                        middle = hand.landmark[12].y > hand.landmark[10].y
                        ring = hand.landmark[16].y > hand.landmark[14].y
                        pinky = hand.landmark[20].y > hand.landmark[18].y
                        return thumb and index and middle and ring and pinky

                    if is_fist(hand_landmarks):
                        try:
                            client_socket.sendall("FistDetected".encode())
                            print("Puño detectado. Mensaje enviado a Unity.")
                        except Exception as e:
                            print(f"Error al enviar mensaje de puño a Unity: {e}")
                            break

            cv2.imshow('MediaPipe Hands', frame)

            # Presiona 'q' para salir
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
