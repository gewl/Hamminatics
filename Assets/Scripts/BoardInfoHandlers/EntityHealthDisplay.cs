using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EntityHealthDisplay : MonoBehaviour {

    [SerializeField]
    Sprite fullHealthBub;
    [SerializeField]
    Sprite emptyHealthBub;

    public void UpdateHealthDisplay(int maxHealth, int currentHealth)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject childObject = transform.GetChild(i).gameObject;
            if (i < currentHealth)
            {
                childObject.GetComponent<Image>().sprite = fullHealthBub;
            }
            else if (i < maxHealth)
            {
                childObject.GetComponent<Image>().sprite = emptyHealthBub;
            }
            else
            {
                childObject.SetActive(false);
            }
        }
    }
}
