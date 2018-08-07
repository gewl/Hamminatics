using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorePane : MonoBehaviour {
    StoreItemDisplay[] itemDisplays;

    [SerializeField]
    Button buyItemButton;

    private void Awake()
    {
        itemDisplays = GetComponentsInChildren<StoreItemDisplay>();
    }

    public void ShowStore(int depth, Inventory inventory)
    {
        gameObject.SetActive(true);
        buyItemButton.interactable = false;
    }
}
