using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void SavePlayer (MainHeroHp player)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/player.idle";
        FileStream stream = new FileStream(path, FileMode.Create);

        PlayerData data = new PlayerData(player);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static PlayerData LoadPlayer()
    {
        string path = Application.persistentDataPath + "/player.idle";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            PlayerData data = formatter.Deserialize(stream) as PlayerData;
            stream.Close();

            return data;

        }
        else
        {
            Debug.LogError("No file in " + path);
            return null;
        }
    }

    public static void SaveInventory(Inventory inventory)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/inventory.idle";
        FileStream stream = new FileStream(path, FileMode.Create);

        InventoryData data = new InventoryData(inventory);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static InventoryData LoadInventory()
    {
        string path = Application.persistentDataPath + "/inventory.idle";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            InventoryData data = formatter.Deserialize(stream) as InventoryData;
            stream.Close();

            return data;

        }
        else
        {
            Debug.LogError("No file in " + path);
            return null;
        }
    }

}
