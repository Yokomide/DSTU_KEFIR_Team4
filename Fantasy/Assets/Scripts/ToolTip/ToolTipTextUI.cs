using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTipTextUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    CurrentItem currentItem;
    bool iscurrentItem;
    CurrentItemMerchant currentItemMerchant;
    bool iscurrentItemMerchant;

    private void Start()
    {
        if (currentItem = gameObject.GetComponent<CurrentItem>())
        {
            currentItem = gameObject.GetComponent<CurrentItem>();
            iscurrentItem = true;
        }
        else
        {
            iscurrentItem = false;
        }
        if (currentItemMerchant = gameObject.GetComponent<CurrentItemMerchant>())
        {
            currentItemMerchant = gameObject.GetComponent<CurrentItemMerchant>();
            iscurrentItemMerchant = true;
        }
        else
        {
            iscurrentItemMerchant = false;
        }
    }
    void IPointerEnterHandler.OnPointerEnter(PointerEventData e)
    {
        if (iscurrentItem)
        {
            if (currentItem.inventory.items[currentItem.index].id != 0)
            {
                ToolTip.text = currentItem.inventory.items[currentItem.index].descriptionItem;
                ToolTip.isUI = true;
            }
        }
        if(iscurrentItemMerchant){
            if (currentItemMerchant.merchant._merchantsItems[currentItemMerchant.index].id != 0)
            {
                ToolTip.text = currentItemMerchant.merchant._merchantsItems[currentItemMerchant.index].descriptionItem;
                ToolTip.isUI = true;
            }
        }

    }

    void IPointerExitHandler.OnPointerExit(PointerEventData e)
    {
        ToolTip.isUI = false;
    }
}
