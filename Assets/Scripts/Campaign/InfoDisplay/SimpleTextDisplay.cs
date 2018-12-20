using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SimpleTextDisplay : MonoBehaviour {

    [SerializeField]
    Text titleText;
    [SerializeField]
    Text bodyText;

    public void ShowTextDisplay(string title, string body)
    {
        gameObject.SetActive(true);
        titleText.text = title;
        bodyText.text = body.Replace("\\n", "\n");

        GameStateManager.ActivateFullScreenTrigger((BaseEventData data) => OnClickAnywhereWhenPaneExpanded());
    }

    void OnClickAnywhereWhenPaneExpanded()
    {
        gameObject.SetActive(false);
        GameStateManager.DisableFullScreenTrigger();
    }
}
