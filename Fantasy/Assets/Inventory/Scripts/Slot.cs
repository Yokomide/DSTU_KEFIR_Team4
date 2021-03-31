using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Sprite activeCell;
    public Sprite defaultCell;
    Image img;

    void Start()
    {
        img = GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        img.GetComponent<Image>().sprite = activeCell;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        img.GetComponent<Image>().sprite = defaultCell;
    }

    void OnDisable()
    {
        img.GetComponent<Image>().sprite = defaultCell;
    }
}
