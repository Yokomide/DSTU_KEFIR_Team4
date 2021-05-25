using UnityEngine;

public class LoadSaveSecondLocation : MonoBehaviour
{
    public GameObject inventory;
    private void Start()
    {
        gameObject.GetComponent<MainHeroHp>().LoadPlayer();
    }

}
