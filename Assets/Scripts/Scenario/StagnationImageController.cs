using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class StagnationImageController : MonoBehaviour {

    [SerializeField]
    Sprite threateningStagnationSprite;
    [SerializeField]
    BoardController boardController;

    private void OnEnable()
    {
        GameStateDelegates.OnNewScenario += OnNewScenarioHandler;
        GameStateDelegates.OnRoundEnded += DrawStagnation;
    }

    private void OnDisable()
    {
        GameStateDelegates.OnNewScenario -= OnNewScenarioHandler;
        GameStateDelegates.OnRoundEnded -= DrawStagnation; 
    }

    void OnNewScenarioHandler(ScenarioState newState)
    {
        Clear();
    }

    void DrawStagnation(ScenarioState state)
    {
        Clear();
        List<Vector2Int> stagnatedPositions = state.stagnatedPositions;

        for (int i = 0; i < stagnatedPositions.Count; i++)
        {
            GameObject instantiatedStagnationTile = ScenarioImageManager.GetStagnationTile(transform);
            instantiatedStagnationTile.transform.position = boardController.GetCellPosition(stagnatedPositions[i]);
        }

        List<Vector2Int> threateningPositions = state.threatenedStagnationPositions;
        for (int i = 0; i < threateningPositions.Count; i++)
        {
            GameObject instantiatedThreatenedImage = ScenarioImageManager.GetOverlayImage(threateningStagnationSprite, transform);
            instantiatedThreatenedImage.transform.position = boardController.GetCellPosition(threateningPositions[i]);
        }
    }

    void Clear()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
