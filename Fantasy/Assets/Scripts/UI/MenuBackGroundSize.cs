using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuBackGroundSize : MonoBehaviour
{
    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
        InventoryMain.ListInit();
    }
}
