using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QueuedActionController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    RectTransform rect;
    Action currentDepictedAction;

    ActionQueueDash queueDash;
    public bool isPlayerAction;

    [SerializeField]
    Image subjectIcon;
    [SerializeField]
    Image actionIcon;

    int cachedSiblingIndex;

    float budgingTime = 0.2f;
    float unbudgedXPosition, budgedXPosition;

    private void Awake()
    {
        queueDash = GetComponentInParent<ActionQueueDash>();
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

    public void UpdateDepictedAction(Action action)
    {
        currentDepictedAction = action;

        Sprite subjectSprite = action.entity.EntitySprite;
        subjectIcon.sprite = subjectSprite;

        Sprite actionSprite = ImageManager.GetActionSprite(action.card.Category, action.direction);
        actionIcon.sprite = actionSprite;
    }

    public void OnBeginDrag(PointerEventData pointerEventData)
    {
        if (!isPlayerAction)
        {
            return;
        }
        queueDash.OnQueuedActionBeginDrag(transform);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData pointerEventData)
    {
        if (!isPlayerAction)
        {
            return;
        }
        transform.position = new Vector2(pointerEventData.position.x, transform.position.y);
        queueDash.OnQueuedActionDrag();
    }

    public void OnEndDrag(PointerEventData pointerEventData)
    {
        if (!isPlayerAction)
        {
            return;
        }
        queueDash.OnQueuedActionDrop();
        transform.SetSiblingIndex(cachedSiblingIndex);
    }

    public void OnOtherActionDragEnded()
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
