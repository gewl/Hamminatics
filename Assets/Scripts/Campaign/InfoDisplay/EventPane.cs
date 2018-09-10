using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventPane : MonoBehaviour {

    [SerializeField]
    GameStateManager gameStateManager;

    [SerializeField]
    Text eventTitle; 
    [SerializeField]
    Text eventDescription;

    [SerializeField]
    Transform outcomeSelection;

    Button[] outcomeButtons;
    Text[] buttonTexts;

    #region Constants for event JSONObject parsing
    const string EVENT_NAME = "name";
    const string EVENT_DESCRIPTION = "description";
    const string OUTCOMES_ARRAY = "outcomes";

    const string OUTCOME_BUTTON_TEXT = "button_text";
    const string OUTCOME_TITLE_TEXT = "outcome_title";
    const string OUTCOME_DESCRIPTION_TEXT = "outcome_description";
    const string OUTCOME_EFFECTS = "effects";
    const string OUTCOME_TYPE = "type";
    const string OUTCOME_STRING = "string_data";
    const string OUTCOME_INT = "int_data";
    #endregion

    List<JSONObject> availableOutcomes;

    private void Awake()
    {
        outcomeButtons = outcomeSelection.GetComponentsInChildren<Button>();
        buttonTexts = outcomeSelection.GetComponentsInChildren<Text>();
    }

    public void DisplayEvent(CampaignState campaignState)
    {
        int currentDepth = campaignState.depth;

        JSONObject eventObject = DataRetriever.GetRandomEventByDepth(currentDepth);

        gameObject.SetActive(true);

        eventTitle.text = eventObject.GetField(EVENT_NAME).str;
        eventDescription.text = eventObject.GetField(EVENT_DESCRIPTION).str;

        availableOutcomes = eventObject.GetField(OUTCOMES_ARRAY).list;
        int numberOfOutcomes = availableOutcomes.Count;

        for (int i = 0; i < outcomeButtons.Length; i++)
        {
            Button outcomeButton = outcomeButtons[i];
            outcomeButton.onClick.RemoveAllListeners();
            if (i < numberOfOutcomes)
            {
                outcomeButton.onClick.AddListener(GenerateButtonListener(i));
                outcomeButton.gameObject.SetActive(true);
                Text buttonText = buttonTexts[i];
                buttonText.text = availableOutcomes[i].GetField(OUTCOME_BUTTON_TEXT).str;
            }
            else
            {
                outcomeButton.gameObject.SetActive(false);
            }
        }
    }

    UnityAction GenerateButtonListener(int index)
    {
        return () => ChooseOutcome(index);
    }

    public void ChooseOutcome(int index)
    {
        JSONObject selectedOutcome = availableOutcomes[index];

        List<JSONObject> outcomeEffects = selectedOutcome.GetField(OUTCOME_EFFECTS).list;

        for (int i = 0; i < outcomeEffects.Count; i++)
        {
            JSONObject outcomeEffect = outcomeEffects[i];
            string effectType = outcomeEffect.GetField(OUTCOME_TYPE).str;
            string effectStringData = outcomeEffect.GetField(OUTCOME_STRING).str;
            int effectIntData = (int)outcomeEffect.GetField(OUTCOME_INT).f;
            gameStateManager.ProcessEventOutcome(effectType, effectStringData, effectIntData);
        }

        eventTitle.text = selectedOutcome.GetField(OUTCOME_TITLE_TEXT).str;
        eventDescription.text = selectedOutcome.GetField(OUTCOME_DESCRIPTION_TEXT).str;

        for (int i = 0; i < outcomeButtons.Length; i++)
        {
            Button outcomeButton = outcomeButtons[i];
            if (i == 0)
            {
                outcomeButton.onClick.RemoveAllListeners();
                outcomeButton.onClick.AddListener(() => gameStateManager.CloseEventPane());
                buttonTexts[i].text = "Ok";
            }
            else
            {
                outcomeButton.gameObject.SetActive(false);
            }
        }
    }
}
