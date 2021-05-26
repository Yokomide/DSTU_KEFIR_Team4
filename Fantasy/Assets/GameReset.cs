using UnityEngine.SceneManagement;
using UnityEngine;

public class GameReset : MonoBehaviour
{
    public void Clicked()
    {
        SceneManager.LoadScene(1);
        InventoryMain.ListInit();
    }
}
