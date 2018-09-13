using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class MainMenuController : SerializedMonoBehaviour {
    [SerializeField]
    GameObject unlockScreen;
    [SerializeField]
    GameObject whatScreen;
    [SerializeField]
    GameObject chaosScreen;
    [SerializeField]
    GameObject characterPicker;

    [SerializeField]
    GameObject menuButtons;

    GameObject currentScreen;

    [SerializeField]
    Button startGameButton;
    [SerializeField]
    Text classText;
    [SerializeField]
    List<string> classNames;

    [SerializeField]
    GameStateManager gameManager;

    const string DESIREMANCER_DESC = "An acolyte of pleasure and necessity, Gimbus has spent years learning to control other people's needs--yet he has no such mastery over his own obsessions.";
    const string LOCKED_DESC = "???";

    public void ShowUnlockScreen()
    {
        if (currentScreen != null)
        {
            currentScreen.SetActive(false);
        }

        unlockScreen.SetActive(true);
        currentScreen = unlockScreen;
    }

    public void ShowWhatScreen()
    {
        if (currentScreen != null)
        {
            currentScreen.SetActive(false);
        }

        whatScreen.SetActive(true);
        currentScreen = whatScreen;
    }

    public void ShowChaosScreen()
    {
        if (currentScreen != null)
        {
            currentScreen.SetActive(false);
        }

        chaosScreen.SetActive(true);
        currentScreen = chaosScreen;
    }

    public void ShowCharacterPicker()
    {
        menuButtons.SetActive(false);
        characterPicker.SetActive(true);
        startGameButton.interactable = false;
    }

    public void OnCharacterIconClicked()
    {
        int index = EventSystem.current.currentSelectedGameObject.transform.GetSiblingIndex();
        string charName = classNames[index];
        string charDescription = index == 0 ? DESIREMANCER_DESC : LOCKED_DESC;

        classText.text = charName + "\n" + charDescription;

        startGameButton.interactable = index == 0;
    }

    public void OnStartGame()
    {
        gameManager.BeginCampaign();
    }
}
