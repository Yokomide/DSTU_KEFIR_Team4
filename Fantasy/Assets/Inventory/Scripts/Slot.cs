using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        //img.GetComponent<Image>().sprite = defaultCell;
    }
}
