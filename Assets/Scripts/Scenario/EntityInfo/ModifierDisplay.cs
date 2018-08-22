using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ModifierDisplay : MonoBehaviour {

    [SerializeField]
    GameObject noActiveModifiersText;

    [SerializeField]
    GameObject modifierInfoDisplay;
    [SerializeField]
    Text modifierName;
    [SerializeField]
    Text modifierInfo;

    List<ModifierData> depictedModifiers;
    Image[] modifierThumbnails;

    private void Awake()
    {
        modifierThumbnails = GetComponentsInChildren<Image>(true);
    }

    public void UpdateDisplayedModifiers(List<ModifierData> modifiers)
    {
        depictedModifiers = modifiers;
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

    public void OnModifierClick(int slot)
    {
        modifierInfoDisplay.SetActive(true);

        ModifierData modifierToDisplay = depictedModifiers[slot];
        modifierName.text = modifierToDisplay.ID;
        modifierInfo.text = GetModifierInfoString(modifierToDisplay);

        GameStateManager.ActivateFullScreenTrigger((BaseEventData data) => OnClickAnywhereWhenPaneExpanded());
    }

    void OnClickAnywhereWhenPaneExpanded()
    {
        modifierInfoDisplay.SetActive(false);
        GameStateManager.DisableFullScreenTrigger();
    }

    string GetModifierInfoString(ModifierData modifier)
    {
        string result = "";
        switch (modifier.modifierCategory)
        {
            case ModifierCategory.Slow:
                result += "Decreases movement speed by " + modifier.value + ".";
                break;
            case ModifierCategory.Speed:
                result += "Increases movement speed by " + modifier.value + ".";
                break;
            case ModifierCategory.Weaken:
                result += "Decreases attack speed by " + modifier.value + ".";
                break;
            case ModifierCategory.Strength:
                result += "Increases attack speed by " + modifier.value + ".";
                break;
            case ModifierCategory.DamageOverTime:
                result += "Deals " + modifier.value + " damage to entity per turn.";
                break;
            case ModifierCategory.HealOverTime:
                result += "Heals entity for " + modifier.value + " damage per turn.";
                break;
            default:
                result += "Modifier description not found.";
                break;
        }

        result += "\nRemaining duration: " + modifier.duration + " turns.";
        return result;
    }

}
