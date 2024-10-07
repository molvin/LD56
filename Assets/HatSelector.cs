using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HatSelector : MonoBehaviour
{
    public List<GameObject> Hats;

    public void SetHat(Weapon weapon)
    {
        foreach (GameObject hat in Hats)
            hat.SetActive(false);

        var h = Hats.FirstOrDefault(x => x.name == weapon.Name);
        if (h != null)
            h.SetActive(true);
    }
}
