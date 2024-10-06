using System;
using System.Collections;
using System.Collections.Generic;
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
    public GameObject ShopParent;
    public Button SkipShopButton;
    public Button[] ShopChoiceButtons;

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

    public void Shop(Action<Weapon> callback)
    {
        IEnumerator Coroutine()
        {
            ShopParent.SetActive(true);

            bool choiceMade = false;
            SkipShopButton.onClick.AddListener(() => choiceMade = true);

            while(!choiceMade)
            {
                // TODO: choose weapon
                yield return null;
            }
            SkipShopButton.onClick.RemoveAllListeners();

            ShopParent.SetActive(false);
            callback(null);
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
}
