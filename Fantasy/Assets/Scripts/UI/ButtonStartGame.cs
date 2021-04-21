using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonStartGame : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
}
