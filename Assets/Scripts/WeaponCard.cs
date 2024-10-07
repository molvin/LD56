using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponCard : MonoBehaviour
{
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Damage;
    public TextMeshProUGUI Description;
    public Image Icon;
    public Button Button;
    public GameObject DescriptionObject;
    public OnHover HoverHelper;
    private bool hoverable;

    public void Init(Weapon w, System.Action callback, bool hoverable)
    {
        this.hoverable = hoverable;
        Name.text = w.Name;
        Damage.text = $"{w.GetDamage()}";
        Description.text = w.Name;

        if(callback != null)
        {
            Button.interactable = true;
            Button.onClick.AddListener(() => callback());
        }
        else
        {
            Button.interactable = false;
        }
    }

    public void DeInit()
    {
        hoverable = false;
        DescriptionObject.SetActive(false);
        Button.onClick.RemoveAllListeners();
        Button.interactable = false;
    }

    private void Update()
    {
        if (!hoverable)
            return;

        // Show details if hovered, else hide
        DescriptionObject.SetActive(HoverHelper.Hovered);
    }
}
