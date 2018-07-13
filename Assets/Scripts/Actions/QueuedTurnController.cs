using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QueuedTurnController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    RectTransform rect;
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

        rect = GetComponent<RectTransform>();
    }

    public void RecalculateBudgedPosition(bool isAlreadyBudged)
    {
        if (isAlreadyBudged)
        {
            budgedXPosition = transform.position.x;
            unbudgedXPosition = budgedXPosition + rect.rect.width;
        }
        else
        {
            unbudgedXPosition = transform.position.x;
            budgedXPosition = unbudgedXPosition - rect.rect.width;
        }
    }

    public void UpdateDepictedTurn(Turn turn)
    {
        depictedTurn = turn;

        Sprite subjectSprite = turn.Entity.EntitySprite;
        subjectIcon.sprite = subjectSprite;

        Sprite directionSprite = ImageManager.GetMovementSprite(turn.moves[0]);
        firstActionIcon.sprite = directionSprite;

        Sprite actionSprite = ImageManager.GetActionSprite(turn.action.card.Category, turn.action.direction);
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
        transform.position = new Vector2(pointerEventData.position.x, transform.position.y);
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
            Vector2 newPosition = new Vector2(updatedXPosition, transform.position.y);

            transform.position = newPosition;

            yield return null;
        }

        transform.position = new Vector2(destinationX, transform.position.y);
    }
}
