// OWOSensationsManager.cs
// Descripción: Administra y envía las sensaciones hápticas utilizando la librería OWOGame.
// Permite disparar sensaciones basadas en gestos detectados y también mediante entradas del teclado.


using UnityEngine;
using OWOGame;
using TMPro;
using UnityEngine.UI;

public class OWOSensationsManager : MonoBehaviour
{
    // Instancia singleton para acceso global
    public static OWOSensationsManager instance;

    // Sensaciones predefinidas configuradas en el Sensations Creator
    private BakedSensation upsens = BakedSensation.Parse("0~up~100,3,98,100,100,0,Impact|0%100,1%100,7%100,6%100~alert-3~");
    private BakedSensation downsens = BakedSensation.Parse("1~down~100,3,98,100,100,0,Impact|3%100,2%100,9%100,8%100~alert-3~");
    private BakedSensation leftsens = BakedSensation.Parse("2~left~100,3,98,100,100,0,Impact|4%100~alert-3~");
    private BakedSensation rightSens = BakedSensation.Parse("3~right~100,3,98,100,100,0,Impact|5%100~alert-3~");

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Debug.Log("Inicializando OWOSensationsManager");
        var auth = GameAuth.Create(upsens, downsens, leftsens, rightSens);
        OWO.Configure(auth);
        OWO.AutoConnect();
    }

    void Update()
    {
        // Permite disparar sensaciones manualmente con el teclado (para pruebas)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerUpSens();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            TriggerDownSens();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            TriggerLeftSens();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            TriggerRightSens();
        }
    }

    public void TriggerUpSens()
    {
        TriggerSensation(upsens);
    }

    public void TriggerDownSens()
    {
        TriggerSensation(downsens);
    }

    public void TriggerLeftSens()
    {
        TriggerSensation(leftsens);
    }

    public void TriggerRightSens()
    {
        TriggerSensation(rightSens);
    }

    /// <summary>
    /// Envía la sensación especificada a través del sistema OWO.
    /// </summary>
    /// <param name="sensation">La sensación a enviar</param>
    private void TriggerSensation(BakedSensation sensation)
    {
        OWO.Send(sensation);
    }
}
