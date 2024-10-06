using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Image HealthFill;

    public void SetHealth(float t)
    {
        HealthFill.fillAmount = t;
    }
}
