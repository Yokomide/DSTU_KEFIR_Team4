using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryMain
{
    private static InventoryMain main = null;
    public static List<Items> items ;
    protected InventoryMain()
    {

    }
    public static InventoryMain Initialization()
    {
        if (main == null){main = new InventoryMain();}
            
        return main;
    }
    public static List<Items> ListInit()
    {
        if (items==null)
        {
            items = new List<Items>();
            for ( int i = 0 ; i < 21 ; i++ )//вместо 21 нужно ставить количество ячеек инвентаря
            {
                items.Add(new Items());
            }
        }
        return items;
    }
    public static List<Items> ReturnList()
    {
        return items;
    }
    public static void AddItem(Items item)
    {
        if (item.isStackable)
        {
            AddStackableItem(item);
        }
        else
        {
            AddUnstackableItem(item);
        }
    }

    static void AddStackableItem(Items item)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].id == item.id)
            {
                if (items[i].countItem < item.maxStackSize)
                {
                    items[i].countItem++;
                    return;
                }
            }
        }
        AddUnstackableItem(item);
    }

    static void AddUnstackableItem(Items item)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].id == 0)
            {
                items[i] = item;
                items[i].countItem = 1;
                break;
            }
        }
    }
}

