using System.Collections.Generic;
using UnityEngine;

public class LootManager : MonoBehaviour
{
    public List<Items> allItems;

    private string pathPrefabs = "C:/Users/Admin/Documents/GitHub/DSTU_KEFIR_Team4/Fantasy/Assets/Resources/Prefabs";
    private void Start()
    {
        allItems = new List<Items>();
        for (int i = 0; i < System.IO.Directory.GetFiles(pathPrefabs).Length; i++)
        {
            if (!System.IO.Directory.GetFiles(pathPrefabs)[i].Contains(".meta"))
            {
                //Debug.Log(Resources.Load<GameObject>(System.IO.Directory.GetFiles(pathPrefabs)[i]).GetComponent<Items>().lootType);
                //Debug.Log(System.IO.Directory.GetFiles(pathPrefabs)[i]);
                if (Resources.Load<GameObject>(System.IO.Directory.GetFiles(pathPrefabs)[i]).GetComponent<Items>().lootType=="low")
                {
                    Debug.Log(System.IO.Directory.GetFiles(pathPrefabs)[i]);
                }
                //if ((Resources.Load<GameObject>(System.IO.Directory.GetFiles(pathPrefabs)[i]).GetComponent<Items>().lootType == "low"))
            }
        }
    }
}
