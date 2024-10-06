using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopOption : MonoBehaviour
{
    public TextMeshProUGUI Name;
    public Button Button;

    public void Init(Weapon weapon)
    {
        Name.text = weapon.Name;
    }
}
