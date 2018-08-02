using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QueuedTurnController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    RectTransform rectTransform;
    Turn depictedTurn;

    TurnQueueDash queueDash;
    public bool isPlayerTurn;

    [SerializeField]
    Image subjectIcon;
    [SerializeField]
    Image firstActionIcon;
    [SerializeField]
    Image secondActionIcon;

    int cachedSiblingIndex;

    float budgingTime = 0.2f;
    float unbudgedXPosition, budgedXPosition;

    private void Awake()
    {
        queueDash = GetComponentInParent<TurnQueueDash>();
        cachedSiblingIndex = transform.GetSiblingIndex();

        rectTransform = GetComponent<RectTransform>();
    }

    public void RecalculateBudgedPosition(bool isAlreadyBudged)
    {
        if (isAlreadyBudged)
        {
            budgedXPosition = transform.localPosition.x;
            unbudgedXPosition = budgedXPosition + rectTransform.rect.width;
        }
        else
        {
            unbudgedXPosition = transform.localPosition.x;
            budgedXPosition = unbudgedXPosition - rectTransform.rect.width;
        }
    }

    public void UpdateDepictedTurn(Turn turn)
    {
        depictedTurn = turn;

        Sprite subjectSprite = turn.Entity.EntitySprite;
        subjectIcon.sprite = subjectSprite;

        Sprite directionSprite = turn.moves.Count > 0 ? ScenarioImageManager.GetMovementSprite(turn.moves[0]) : ScenarioImageManager.GetEmptyActionSprite();
        firstActionIcon.sprite = directionSprite;

        Sprite actionSprite = turn.action.card != null ? ScenarioImageManager.GetActionSprite(turn.action.card.Category, turn.action.direction) : ScenarioImageManager.GetEmptyActionSprite();
        secondActionIcon.sprite = actionSprite;
    }

    public void OnBeginDrag(PointerEventData pointerEventData)
    {
        if (!isPlayerTurn)
        {
            return;
        }
        queueDash.OnQueuedTurnBeginDrag(transform);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData pointerEventData)
    {
        if (!isPlayerTurn)
        {
            return;
        }
        Vector3 adjustedEventData = queueDash.UnscalePointerData(pointerEventData.position);
        rectTransform.anchoredPosition = new Vector2(adjustedEventData.x, rectTransform.anchoredPosition.y);
        queueDash.OnQueuedTurnDrag();
    }

    public void OnEndDrag(PointerEventData pointerEventData)
    {
        if (!isPlayerTurn)
        {
            return;
        }
        queueDash.OnQueuedTurnDrop();
        transform.SetSiblingIndex(cachedSiblingIndex);
    }

    public void OnOtherTurnDragEnded()
    {
        StopAllCoroutines();
    }

    public void ToggleBudgedStatus(bool isBudged)
    {
        StopAllCoroutines();

        if (isBudged)
        {
            StartCoroutine(Budge(true));
        }
        else
        {
            StartCoroutine(Budge(false));
        }
    }

    IEnumerator Budge(bool isBudging)
    {
        float timeElapsed = 0.0f;

        float initialX = unbudgedXPosition;
        float destinationX = budgedXPosition;

        if (!isBudging)
        {
            initialX = budgedXPosition;
            destinationX = unbudgedXPosition;
        }

        while (timeElapsed < budgingTime)
        {
            timeElapsed += Time.deltaTime;
            float percentageComplete = timeElapsed / budgingTime;

            float updatedXPosition = Mathf.Lerp(initialX, destinationX, percentageComplete);
            Vector2 newPosition = new Vector2(updatedXPosition, transform.localPosition.y);

            transform.localPosition = newPosition;

            yield return null;
        }

        transform.localPosition = new Vector2(destinationX, transform.localPosition.y);
    }
}
