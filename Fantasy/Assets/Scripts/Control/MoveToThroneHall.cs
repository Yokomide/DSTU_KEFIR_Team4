using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveToThroneHall : MonoBehaviour
{
    public GameObject inventory;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<MainHeroHp>().SavePlayer();
            SceneManager.LoadScene(2);
        }
    }
}
