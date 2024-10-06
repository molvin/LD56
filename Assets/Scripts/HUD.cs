using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Image HealthFill;
    public RectTransform EquipmentFrame;

    public WeaponCard WeaponCardPrefab;
    private List<WeaponCard> weaponCards = new();

    public void SetHealth(float t)
    {
        HealthFill.fillAmount = t;
    }

    public void SetWeapons(IEnumerable<Weapon> weapons)
    {
        foreach(var card in weaponCards)
        {
            Destroy(card.gameObject);
        }
        weaponCards.Clear();

        foreach(var item in weapons)
        {
            var card = Instantiate(WeaponCardPrefab, EquipmentFrame);
            card.Init(item);
            weaponCards.Add(card);
        }
    }
}
