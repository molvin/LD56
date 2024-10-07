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
    //public Button Button;
    public GameObject DescriptionObject;
    public OnHover HoverHelper;
    private bool hoverable;
    private System.Action callback;
    public void Init(Weapon w, System.Action callback, bool hoverable)
    {
        this.hoverable = hoverable;
        Name.text = w.Name;
        Damage.text = $"{w.GetDisplayDamage()}";

        Description.text = w.Name;

        this.callback = callback;
    }

    public void Call()
    {
        Debug.Log("im called00");
        callback?.Invoke();
    }

    public void DeInit()
    {
        hoverable = false;
        DescriptionObject.SetActive(false);
        callback = null;
    }

    private void Update()
    {
        if (!hoverable)
            return;

        // Show details if hovered, else hide
        //DescriptionObject.SetActive(HoverHelper.Hovered); :TODO
    }
}
