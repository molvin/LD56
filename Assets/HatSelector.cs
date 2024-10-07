using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HatSelector : MonoBehaviour
{
    [System.Serializable]
    public struct WeaponHat
    {
        public string WeaponName;
        public GameObject Hat;
        public Color Color;
    }
    public List<WeaponHat> WeaponHats;

    public void SetHat(Weapon weapon)
    {
        foreach (WeaponHat hat in WeaponHats)
        {
            hat.Hat.gameObject.SetActive(false);
        }

        foreach (WeaponHat hat in WeaponHats)
        {
            if (weapon.Name == hat.WeaponName)
            {
                hat.Hat.gameObject.SetActive(true);
                hat.Hat.GetComponent<MeshRenderer>().material.color = hat.Color;
            }
        }
    }
}
