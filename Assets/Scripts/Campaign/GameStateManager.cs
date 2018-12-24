using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class GameStateManager : MonoBehaviour {
    const string PLAYER_ID = "Player";

    public static CampaignState CurrentCampaign { get; private set; }

    [SerializeField]
    MapController mapController;
    [SerializeField]
    ScenarioStateManager scenarioManager;
    [SerializeField]
    MainMenuController mainMenuController;
    [SerializeField]
    EventPane eventPane;
    [SerializeField]
    StorePane storePane;
    [SerializeField]
    ItemSlotPickerController itemSlotPicker;
    [SerializeField]
    SimpleTextDisplay simpleTextDisplay;

    [SerializeField]
    EnemySpawnGroupManager enemySpawnGroupManager;

    [SerializeField]
    GameObject dimmer;
    [SerializeField]
    EventTrigger fullScreenTrigger;

    [SerializeField]
    CardData healthCard;
    public CardData HealthCard { get { return healthCard; } }
    [SerializeField]
    CardData upgradeCard;
    public CardData UpgradeCard { get { return upgradeCard; } }
    [SerializeField]
    CardData goldRewardCard;
    public CardData GoldRewardCard { get { return goldRewardCard; } }

    // TODO: Dynamic player selection/assignment in opening menu
    [SerializeField]
    EntityData playerData;

    static GameStateManager instance;

    // Scenario rewards. Remainder is gold.
    int oddsOfNewCard = 70;
    int oddsOfUpgrade = 20;

    System.Random rand;

    private void Awake()
    {
        instance = this;
        rand = new System.Random();
    }

    public void BeginCampaign()
    {
        mainMenuController.gameObject.SetActive(false);
        mapController.gameObject.SetActive(true);
        EntityData player = ScriptableObject.Instantiate(playerData);

        CurrentCampaign = new CampaignState(new Inventory(), new Map(1), player);
        DataRetriever.UpdateDepthData(CurrentCampaign.depth);
        GameStateDelegates.OnCampaignStateUpdated(CurrentCampaign);

        InitializeFirstMap();
    }

    void InitializeFirstMap()
    {
        Map currentMap = CurrentCampaign.CurrentMap;
        mapController.DrawMap(currentMap);
        mapController.UpdateMapState(CurrentCampaign);
    }

    public void UpdatePlayerPosition(MapNode newPlayerNode)
    {
        float nodeDistance = CurrentCampaign.CurrentMap.GetNodeDistance(newPlayerNode);
        CurrentCampaign.UpdatePlayerNode(newPlayerNode, nodeDistance);
        mapController.UpdateMapState(CurrentCampaign);

        switch (newPlayerNode.nodeType)
        {
            case MapNodeType.Start:
                Debug.Log("moved to " + newPlayerNode.nodeType);
                break;
            case MapNodeType.Scenario:
                SwitchToScenario();
                break;
            case MapNodeType.Event:
                TriggerEvent();
                break;
            case MapNodeType.Store:
                DisplayStore();
                break;
            case MapNodeType.End:
                GenerateNewMap();
                break;
            case MapNodeType.Boss:
                SwitchToBossScenario(CurrentCampaign.depth);
                break;
            default:
                break;
        }
    }

    void GenerateNewMap()
    {
        CurrentCampaign.depth++;
        Map newMap = new Map(CurrentCampaign.depth);
        CurrentCampaign.UpdateMap(newMap);
        mapController.DrawMap(newMap);
        mapController.UpdateMapState(CurrentCampaign);

        DataRetriever.UpdateDepthData(CurrentCampaign.depth);
        GameStateDelegates.OnCampaignStateUpdated(CurrentCampaign);
    }

    #region event processing
    public void ProcessEventOutcome(string effect, string stringData, int intData)
    {
        switch (effect)
        {
            case EventType.CHANGE_VALUE:
                HandleChangeValue(stringData, intData);
                break;
            case EventType.OFFER_CARD:
                HandleOfferCard(stringData, intData);
                break;
            case EventType.TRIGGER_SCENARIO:
                HandleTriggerScenario(stringData, intData);
                break;
            case EventType.APPLY_MODIFIER:
                HandleApplyModifier(stringData, intData);
                break;
            case EventType.OFFER_UPGRADE:
                HandleOfferUpgrade();
                break;
            default:
                Debug.LogError("No handler found for effect: " + effect);
                break;
        }

        GameStateDelegates.OnCampaignStateUpdated(CurrentCampaign);
    }

    void HandleChangeValue(string stringData, int intData)
    {
        switch (stringData)
        {
            case ChangeValueTarget.GOLD:
                CurrentCampaign.inventory.gold += intData;
                break;
            case ChangeValueTarget.HEALTH:
                CurrentCampaign.player.ChangeHealthValue_Campaign(intData);
                break;
            default:
                Debug.LogError("Unable to HandleChangeValue because stringData not found: " + stringData);
                break;
        }
    }

    void HandleOfferCard(string stringData, int intData)
    {
        CardData newCard = DataRetriever.GetPlayerCardData(stringData, CurrentCampaign.depth);
        itemSlotPicker.OfferCard(newCard);
    }

    void HandleOfferUpgrade()
    {
        itemSlotPicker.DisplaySlotPickerForUpgrade();
    }

    void HandleTriggerScenario(string stringData, int intData)
    {
        EnemySpawnGroupData enemySpawnGroup = enemySpawnGroupManager.GetEventScenarioEnemySpawn(stringData);
        SwitchToScenario(enemySpawnGroup);
    }

    void HandleApplyModifier(string stringData, int intData)
    {
        ModifierData newModifier = DataRetriever.GetModifier(stringData);
        CurrentCampaign.player.activeModifiers.Add(newModifier);
    }
    #endregion

    #region focus/pane switching
    void SwitchToScenario()
    {
        mapController.gameObject.SetActive(false);
        scenarioManager.GenerateAndDrawScenario(CurrentCampaign.depth, CurrentCampaign.progressThroughMap);
    }

    void SwitchToScenario(EnemySpawnGroupData enemySpawnGroup)
    {
        mapController.gameObject.SetActive(false);
        scenarioManager.GenerateAndDrawScenario(enemySpawnGroup);
    }

    void SwitchToBossScenario(int depth)
    {
        mapController.gameObject.SetActive(false);
        EnemySpawnGroupData bossSpawnGroup = DataRetriever.GetBossSpawnGroup();
        scenarioManager.GenerateAndDrawBossScenario(bossSpawnGroup);
    }

    void DisplayStore()
    {
        storePane.ShowStore(CurrentCampaign.depth);
        mapController.ShowMap(false);
        SetDim(true);
    }

    public void LeaveStore()
    {
        storePane.gameObject.SetActive(false);
        mapController.ShowMap(true);
        SetDim(false);
    }

    public void SwitchToCampaign(ScenarioState lastScenarioState)
    {
        scenarioManager.gameObject.SetActive(false);
        mapController.gameObject.SetActive(true);

        CurrentCampaign.player = lastScenarioState.player;
        CurrentCampaign.player.activeModifiers.Clear();
        CurrentCampaign.inventory = lastScenarioState.inventory;

        if (lastScenarioState.enemies.Count == 0)
        {
            CardData scenarioReward = lastScenarioState.scenarioReward;
            if (scenarioReward == null)
            {
                scenarioReward = GenerateRandomScenarioReward();
            }

            if (scenarioReward == upgradeCard)
            {
                simpleTextDisplay.ShowTextDisplay(ScenarioRewardText.UPGRADE_TITLE, ScenarioRewardText.UPGRADE_BODY);
                itemSlotPicker.DisplaySlotPickerForUpgrade();
            }
            else if (scenarioReward == healthCard)
            {
                simpleTextDisplay.ShowTextDisplay(ScenarioRewardText.HEALTH_TITLE, ScenarioRewardText.HEALTH_BODY);
                CurrentCampaign.player.ChangeHealthValue_Campaign(1);
            }
            else if (scenarioReward == goldRewardCard)
            {
                simpleTextDisplay.ShowTextDisplay(ScenarioRewardText.GOLD_TITLE, ScenarioRewardText.GOLD_BODY);
                CurrentCampaign.inventory.gold += (5 * DataRetriever.GetDepthGoldMultiplier());
            }
            else
            {
                simpleTextDisplay.ShowTextDisplay(ScenarioRewardText.NEW_CARD_TITLE, ScenarioRewardText.NEW_CARD_BODY);
                itemSlotPicker.OfferCard(scenarioReward);
            }
        }
        GameStateDelegates.OnCampaignStateUpdated(CurrentCampaign);
    }

    CardData GenerateRandomScenarioReward()
    {
        int randomNumber = rand.Next(1, 101);
        CardData scenarioReward = goldRewardCard;

        if (randomNumber <= oddsOfNewCard)
        {
            CardData[] currentlyEquippedCards = CurrentCampaign.inventory.equippedCards;
            List<CardData> availableCardPool = DataRetriever.GetRandomCardPool().Where(card => !currentlyEquippedCards.Contains(card)).ToList();
            scenarioReward = availableCardPool.GetRandomElement();
        }
        else if (randomNumber <= oddsOfNewCard + oddsOfUpgrade)
        {
            scenarioReward = upgradeCard;
        }
        return scenarioReward;
    }

    void TriggerEvent()
    {
        eventPane.DisplayEvent(CurrentCampaign);
        SetDim(true);
    }

    public void CloseEventPane()
    {
        eventPane.gameObject.SetActive(false);
        SetDim(false);
    }

    public static void SetDim(bool isDim)

    {
        instance.dimmer.gameObject.SetActive(isDim);
    }

    public static void ActivateFullScreenTrigger(UnityAction<BaseEventData> pointerClickHandler)
    {
        instance.fullScreenTrigger.gameObject.SetActive(true);

        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };
        entry.callback.AddListener(pointerClickHandler);
        instance.fullScreenTrigger.triggers.Add(entry);
    }

    public static void DisableFullScreenTrigger()
    {
        instance.fullScreenTrigger.triggers.Clear();
        instance.fullScreenTrigger.gameObject.SetActive(false);
    }
    #endregion
}
