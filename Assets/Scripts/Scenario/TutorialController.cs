using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class TutorialController : MonoBehaviour {
    [SerializeField]
    GameObject touchBlocker;

    [SerializeField]
    List<GameObject> tutorialElements;

    int tutorialElementPointer;

    public void EnableTutorial()
    {
        touchBlocker.SetActive(true);
        tutorialElementPointer = 0;
        tutorialElements[tutorialElementPointer].SetActive(true);
    }

    public void DisableTutorial()
    {
        touchBlocker.SetActive(false);
        tutorialElements[tutorialElementPointer].SetActive(false);
    }

    public void NextTutorialPane()
    {
        tutorialElements[tutorialElementPointer].SetActive(false);
        tutorialElementPointer++;
        tutorialElements[tutorialElementPointer].SetActive(true);
    }

    public void PreviousTutorialPane()
    {
        tutorialElements[tutorialElementPointer].SetActive(false);
        tutorialElementPointer--;
        tutorialElements[tutorialElementPointer].SetActive(true);
    }
}
