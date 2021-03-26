using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
	private Canvas canvas;
	public GameObject player;

	public List<Items> items;
	public GameObject cellContainer;
	void Start()
	{
		canvas = GetComponent<Canvas>();
		canvas.enabled = false;
		items = new List<Items>();
        for (int i = 0;i<cellContainer.transform.childCount;i++)
        {
			items.Add(new Items());
        }
	}
	

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.I))
		{
			canvas.enabled = !canvas.enabled;

		}
		if (Input.GetKeyDown(KeyCode.E))
		{

		}
	}
    

	void DisplayItem()
    {
		for (int i = 0; i < items.Count; i++)
        {   
			Transform cell = cellContainer.transform.GetChild(i);
			Transform icon = cell.GetChild(0);
			Image img = icon.GetComponent<Image>();
			if(items[i].id != 0)
            {
				img.enabled = true;
				img.sprite = Resources.Load<Sprite>(items[i].pathIcon);
			}
            else
            {
				img.enabled = false;
				img.sprite = null;
            }
        }
	}
}
