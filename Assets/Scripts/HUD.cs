using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [Header("Health")]
    public Image HealthFill;
    [Header("Kills")]
    public Image KillsFill;
    public TextMeshProUGUI Kills;
    public TextMeshProUGUI KillsToNextLevel;
    public TextMeshProUGUI CurrentLevel;
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
    public WeaponCard[] ShopChoices;
    public WeaponCard WillReplace;
    public RectTransform ShopIndicator;
    public float ShopIndicatorCenterOffset = 100;

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

    public void SetKills(int kills, int requiredKills, int currentLevel)
    {
        KillsFill.fillAmount = (kills / (float)requiredKills);
        Kills.text = $"Kills: {kills}";
        KillsToNextLevel.text = $"Kills to next level {requiredKills - kills}";
        CurrentLevel.text = $"{currentLevel + 1}";
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
            card.Init(item, null, false);
            weaponCards.Add(card);
        }
    }


    public void Shop(int level, Action<Weapon, Weapon> callback, Shop shop)
    {
        IEnumerator Coroutine()
        {
            //ShopParent.SetActive(true);
            var camera = Camera.main.GetComponentInParent<CameraController>();
            camera?.pause(true);
            Vector3 targetPosition = shop.CameraPos.transform.position;
            var smoothing = 10f;
            Vector3 camera_delta = camera.transform.position - camera.GetComponentInChildren<Camera>().transform.position;
            //targetPosition += camera_delta / 3f;
            Quaternion original_rotation = camera.transform.rotation;
            Vector3 original_position = camera.transform.position;
            var startTime = Time.unscaledTime;

            // Calculate the journey length.
            var journeyLength = Vector3.Distance(original_position, targetPosition);
            var speed = 5f;
            var player_original_position = GetComponentInParent<Player>().transform.position;
            var player_target_pos = shop.PlayerPos.transform.position;
            player_target_pos.y = player_original_position.y;
            while ((camera.transform.position - targetPosition).magnitude > 0.1f)
            {
                // Distance moved equals elapsed time times speed..
                float distCovered = (Time.unscaledTime - startTime) * speed;

                // Fraction of journey completed equals current distance divided by total distance.
                float fractionOfJourney = distCovered / journeyLength;

                camera.transform.position = Vector3.Lerp(original_position, targetPosition, fractionOfJourney);
                camera.transform.rotation = Quaternion.Lerp(original_rotation, shop.CameraPos.transform.rotation, fractionOfJourney);
                GetComponentInParent<Player>().transform.position = Vector3.Lerp(player_original_position, player_target_pos, fractionOfJourney);
                GetComponentInParent<Player>().Anim.updateMode = AnimatorUpdateMode.UnscaledTime;
                yield return new WaitForEndOfFrame();
            }
            GetComponentInParent<Player>().Anim.SetBool("Running", false);
            GetComponentInParent<Player>().transform.rotation.SetLookRotation((GetComponentInParent<Player>().transform.position - Camera.main.transform.position).normalized);


            bool choiceMade = false;
            Weapon chosenWeapon = null;
            Weapon weaponToReplace = Inventory.GetRandom();
            SkipShopButton.onClick.AddListener(() => choiceMade = true);

            if (Inventory.AtMax)
            {
                WillReplace.gameObject.SetActive(true);
                WillReplace.Init(weaponToReplace, null, true);
            }
            else
            {
                WillReplace.gameObject.SetActive(false);
            }
            ShopChoices = shop.GetComponentsInChildren<WeaponCard>();

            var weapons = Weapons.GetShop(ShopChoices.Length, level).ToList();
            for(int i = 0; i < ShopChoices.Length; i++)
            {
                Weapon w = weapons[i];
                ShopChoices[i].Init(w, () => chosenWeapon = w, true);
            }

            WeaponCard hovering = null;
            while (!choiceMade && chosenWeapon == null)
            {
                RaycastHit hit = default;
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                hovering?.showDescription(false);

                if (Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("UI")))
                {
                    hovering = hit.collider.GetComponentInChildren<WeaponCard>();
                    if (Input.GetMouseButtonDown(0))
                    {
                        hovering?.Call();
                    }
                    hovering?.showDescription(true);
                }

            
                yield return null;
            }
            camera.transform.rotation = original_rotation;
            GetComponentInParent<Player>().Anim.updateMode = AnimatorUpdateMode.Normal;

            camera?.pause(false);

            for (int i = 0; i < ShopChoices.Length; i++)
            {
                ShopChoices[i].DeInit();
            }
            if(WillReplace.gameObject.activeSelf)
            {
                WillReplace.DeInit();
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
            float x = Mathf.Lerp(0, halfScreenWidth * angleSign, t);
            ShopIndicator.anchoredPosition = new Vector2(x, -halfScreenHeight);
        }
        else
        {
            float angleSign = Mathf.Sign(angle);
            float t = (angle - 45 * angleSign) / (angleSign * 90);
            float y = Mathf.Lerp(halfScreenHeight, -halfScreenHeight, t);
            ShopIndicator.anchoredPosition = new Vector2(angleSign * halfScreenWidth, y);
        }
        ShopIndicator.anchoredPosition -= ShopIndicator.anchoredPosition.normalized * ShopIndicatorCenterOffset;
    }

    public void DisableShopIndicator()
    {
        ShopIndicator.gameObject.SetActive(false);

    }

}
