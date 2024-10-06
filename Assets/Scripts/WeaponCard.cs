using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WeaponCard : MonoBehaviour
{
    public TextMeshProUGUI Name;

    public void Init(Weapon w)
    {
        Name.text = w.Name;
    }
}
