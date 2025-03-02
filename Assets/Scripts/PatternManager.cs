using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Skinetic;

public class PatternManager : MonoBehaviour
{
    public SkineticDevice m_device;
    public HapticEffect m_hapticEffect;
    void Awake()
    {
        m_hapticEffect.StrategyOnPlay = HapticEffect.PlayStrategy.E_PULLED;
        m_hapticEffect.TargetDevice = m_device;
    }

    public void TriggerFinger1()
    {
        m_hapticEffect.TargetPattern = m_device.LoadedPatterns[0];
        m_hapticEffect.PlayEffect();
    }

    public void TriggerFinger2()
    {
        m_hapticEffect.TargetPattern = m_device.LoadedPatterns[1];
        m_hapticEffect.PlayEffect();
    }

    public void TriggerFinger3()
    {
        m_hapticEffect.TargetPattern = m_device.LoadedPatterns[2];
        m_hapticEffect.PlayEffect();
    }

    public void TriggerFinger4()
    {
        m_hapticEffect.TargetPattern = m_device.LoadedPatterns[3];
        m_hapticEffect.PlayEffect();
    }

    public void TriggerFinger5()
    {
        m_hapticEffect.TargetPattern = m_device.LoadedPatterns[4];
        m_hapticEffect.PlayEffect();
    }


}
