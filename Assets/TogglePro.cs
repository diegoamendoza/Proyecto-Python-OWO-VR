using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TogglePro : MonoBehaviour
{

    public Toggle toggle;
    public UnityEvent _eventOn;
    public UnityEvent _eventOff;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CheckLogic()
    {
        if (toggle.isOn)
        {
            _eventOn.Invoke();
        }
        else
        {
            _eventOff.Invoke();
        }
    }

}
