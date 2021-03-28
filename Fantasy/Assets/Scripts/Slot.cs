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

    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        img.sprite = activeCell;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        img.sprite = defaultCell;
    }

    void OnDisable()
    {
        img.sprite = defaultCell;
    }
}
