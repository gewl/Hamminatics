using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModifierDisplay : MonoBehaviour {

    [SerializeField]
    GameObject noActiveModifiersText;
    Image[] modifierThumbnails;

    private void Awake()
    {
        modifierThumbnails = GetComponentsInChildren<Image>(true);
    }

    public void UpdateDisplayedModifiers(List<ModifierData> modifiers)
    {
        if (modifiers.Count == 0)
        {
            for (int i = 0; i < modifierThumbnails.Length; i++)
            {
                modifierThumbnails[i].gameObject.SetActive(false);
            }

            noActiveModifiersText.SetActive(true);
            return;
        }

        noActiveModifiersText.SetActive(false);

        for (int i = 0; i < modifiers.Count; i++)
        {
            Image modifierThumbnail = modifierThumbnails[i];
            modifierThumbnail.gameObject.SetActive(true);
            modifierThumbnail.sprite = modifiers[i].sprite;
        }

        for (int j = modifiers.Count; j < modifierThumbnails.Length; j++)
        {
            Image modifierThumbnail = modifierThumbnails[j];
            modifierThumbnail.gameObject.SetActive(false);
        }
    }

}
