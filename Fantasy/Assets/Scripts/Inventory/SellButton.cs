using UnityEngine;

public class SellButton : MonoBehaviour
{
    public GameObject shopContainer;
    public GameObject button;

    void Start()
    {

    }
    void Update()
    {
        if (shopContainer.activeSelf)
        {
            button.SetActive(true);
        }
        else
        {
            button.SetActive(false);
        }

    }

}
