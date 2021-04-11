using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LootObjects", menuName = "ScriptableObjects/LootObjects", order = 1)]
public class LootManager : ScriptableObject
{
    public List<GameObject> CommonItems;
    public List<GameObject> RareItems;
    public List<GameObject> EpicItems;
    
}
