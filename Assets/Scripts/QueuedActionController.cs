using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QueuedActionController : MonoBehaviour {

    [SerializeField]
    Image subjectIcon;

    [SerializeField]
    Image actionIcon;

    public void UpdateDepictedAction(Action action)
    {
        Sprite subjectSprite = action.entity.EntitySprite;
        subjectIcon.sprite = subjectSprite;

        Sprite actionSprite = ImageManager.GetActionSprite(action.card.Category, action.direction);
        actionIcon.sprite = actionSprite;
    }
}
