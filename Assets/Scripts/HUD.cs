using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [Header("Health")]
    public Image HealthFill;
    [Header("Weapons")]
    public RectTransform EquipmentFrame;
    [Header("GameOver")]
    public float FadeOutTime;
    public float PostFadeOutTime;
    public Image FadeOut;
    [Header("Shop")]
    public Inventory Inventory;
    public GameObject ShopParent;
    public Button SkipShopButton;
    public ShopOption[] ShopChoiceButtons;
    public WeaponCard WillReplace;
    public RectTransform ShopIndicator;

    public WeaponCard WeaponCardPrefab;
    private List<WeaponCard> weaponCards = new();

    private void Start()
    {
        ShopParent.SetActive(false);
    }

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

    public void Shop(Action<Weapon, Weapon> callback)
    {
        IEnumerator Coroutine()
        {
            ShopParent.SetActive(true);

            bool choiceMade = false;
            Weapon chosenWeapon = null;
            Weapon weaponToReplace = Inventory.GetRandom();
            SkipShopButton.onClick.AddListener(() => choiceMade = true);

            if (Inventory.AtMax)
            {
                WillReplace.gameObject.SetActive(true);
                WillReplace.Init(weaponToReplace);
            }
            else
            {
                WillReplace.gameObject.SetActive(false);
            }

            var weapons = Weapons.GetShop(ShopChoiceButtons.Length).ToList();
            for(int i = 0; i < ShopChoiceButtons.Length; i++)
            {
                Weapon w = weapons[i];
                ShopChoiceButtons[i].Button.onClick.AddListener(() => chosenWeapon = w);
                ShopChoiceButtons[i].Init(w);
            }

            while (!choiceMade && chosenWeapon == null)
            {
                yield return null;
            }
            SkipShopButton.onClick.RemoveAllListeners();

            ShopParent.SetActive(false);
            callback(chosenWeapon, weaponToReplace);
        }

        StartCoroutine(Coroutine());
    }

    public void GameOver()
    {
        // TODO: fade out
        
        // TODO: load new scene

        IEnumerator Coroutine()
        {
            float t = 0.0f;
            while(t < FadeOutTime)
            {
                t += Time.unscaledDeltaTime;
                Color color = FadeOut.color;
                color.a = t / FadeOutTime;
                FadeOut.color = color;
                yield return null;
            }
            Color c = FadeOut.color;
            c.a = 1.0f;
            FadeOut.color = c;

            yield return new WaitForSecondsRealtime(PostFadeOutTime);
            SceneManager.LoadScene(0);
        }
        StartCoroutine(Coroutine());
    }

    public void ShowShopIndicator(float angle)
    {
        ShopIndicator.gameObject.SetActive(true);
        Canvas canvas = GetComponent<Canvas>();
        float halfScreenWidth = (Screen.width / canvas.scaleFactor) / 2f;
        float halfScreenHeight = (Screen.height / canvas.scaleFactor) / 2f;

        Debug.Log($"{angle}, {halfScreenWidth}, {halfScreenHeight}");

        if(Mathf.Abs(angle) < 45)
        {
            // up -45 <-> 45
            float t = (angle + 45) / 90;
            float x = Mathf.Lerp(-halfScreenWidth, halfScreenWidth, t);
            ShopIndicator.anchoredPosition = new Vector2(x, halfScreenHeight);
        }
        else if(Mathf.Abs(angle) > 135)
        {
            // down 
            // left side 135-180, right side -180 - -135
            float angleSign = Mathf.Sign(angle);
            float t = (angle - 180 * angleSign) / (-45 * angleSign);
            float x = Mathf.Lerp(0, Screen.width * angleSign, t);
            ShopIndicator.anchoredPosition = new Vector2(x, -halfScreenHeight);
        }
        else
        {
            float angleSign = Mathf.Sign(angle);
            float t = (angle - 45 * angleSign) / (angleSign * 90);
            float y = Mathf.Lerp(Screen.height, -Screen.height, t);
            ShopIndicator.anchoredPosition = new Vector2(angleSign * halfScreenWidth, y);
        }
    }

    public void DisableShopIndicator()
    {
        ShopIndicator.gameObject.SetActive(false);

    }

}
