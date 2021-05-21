using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTipTextUI : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler
{
    public string text;
    CurrentItem currentItem;
	
    private void Start() 
    {
        currentItem = gameObject.GetComponent<CurrentItem>();
    }
	void IPointerEnterHandler.OnPointerEnter(PointerEventData e)
	{
        if(currentItem.inventory.items[currentItem.index].id != 0)
        {
            ToolTip.text = currentItem.inventory.items[currentItem.index].descriptionItem;
		    ToolTip.isUI = true;
        }
	}
	
	void IPointerExitHandler.OnPointerExit(PointerEventData e)
	{
		ToolTip.isUI = false;
	}
}
