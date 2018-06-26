using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QueuedActionController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    RectTransform rect;

    ActionQueueDash queueDash;
    public bool isPlayerAction;

    [SerializeField]
    Image subjectIcon;
    [SerializeField]
    Image actionIcon;

    int cachedSiblingIndex;

    float budgingTime = 0.2f;
    float _unbudgedXPosition, budgedXPosition;
    float unbudgedXPosition
    {
        get
        {
            if (_unbudgedXPosition == 0)
            {
                _unbudgedXPosition = transform.position.x;
                budgedXPosition = transform.position.x - rect.rect.width;
            }

            return _unbudgedXPosition;
        }
    }

    private void Awake()
    {
        queueDash = GetComponentInParent<ActionQueueDash>();
        cachedSiblingIndex = transform.GetSiblingIndex();

        rect = GetComponent<RectTransform>();
    }

    public void UpdateDepictedAction(Action action)
    {
        Sprite subjectSprite = action.entity.EntitySprite;
        subjectIcon.sprite = subjectSprite;

        Sprite actionSprite = ImageManager.GetActionSprite(action.card.Category, action.direction);
        actionIcon.sprite = actionSprite;

        _unbudgedXPosition = 0f;
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

    IEnumerator Budge(bool isBudgingLeft)
    {
        float timeElapsed = 0.0f;

        float initialX = unbudgedXPosition;
        float destinationX = budgedXPosition;

        if (!isBudgingLeft)
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
