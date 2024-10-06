using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomSelectable : Selectable
{

    public MenuMinionController controller;
    override
    public void OnSelect(BaseEventData eventData)
    {
        Debug.Log("hej");
        controller.audio_man.PlaySound(Resources.Load<AudioOneShotClipConfiguration>("object/bulli_bulli"), this.transform.position);
        controller.setMinionDestination(this.gameObject);

    }
    
}
