using UnityEngine;

public class ToolTipText : MonoBehaviour
{
    Items item;
    [HideInInspector]
    public string text;
    private void Start() {
        item = gameObject.GetComponent<Items>();
        text = item.nameItem;
    }
    
}
