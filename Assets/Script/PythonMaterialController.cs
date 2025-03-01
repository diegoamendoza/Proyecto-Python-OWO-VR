// PythonMaterialController.cs
// Descripción: Controla la interpolación y aplicación del vector recibido a materiales y efectos visuales,
// permitiendo mover y modificar shaders y campos de fuerza de partículas.


using System.Collections.Generic;
using UnityEngine;

public class PythonMaterialController : MonoBehaviour
{
    [Header("Materiales y Efectos")]
    public List<Material> mats;      // Lista de materiales a modificar
    public float strenght = 1.0f;      // Intensidad del efecto
    public float lerpSpeed = 5f;       // Velocidad de interpolación
    public float maxClamp = 2f;        // Valor máximo para clamping del vector
    public ParticleSystemForceField psField; // Campo de fuerza para partículas (opcional)

    // Vector de entrada interpolado
    private Vector3 input;

    void Update()
    {
        // Se interpola el vector de entrada recibido desde HandInputController para suavizar el movimiento
        input = Vector3.Lerp(input, HandInputController.instance.vectorInput * strenght, Time.deltaTime * lerpSpeed);

        // Se aplica el vector interpolado a cada material mediante la propiedad "_Input" del shader
        foreach (Material mat in mats)
        {
            mat.SetVector("_Input", Vector3.ClampMagnitude(input, maxClamp));
        }
    }
}
