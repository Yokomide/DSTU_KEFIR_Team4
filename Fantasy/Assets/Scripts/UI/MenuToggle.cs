using UnityEngine;

public class MenuToggle : MonoBehaviour
{
    public GameObject _menuExit;
    public GameObject cellContainer;
    public GameObject shop;

    [HideInInspector]
    public bool isMenuOpened = false;
    private void Start()
    {
        _menuExit.SetActive(false);
    }
    private void Update()
    {
        ToggleMenu();
    }

    private void ToggleMenu()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_menuExit.activeSelf)
            {
                _menuExit.SetActive(false);
            }
            else
            {
                _menuExit.SetActive(true);
                cellContainer.SetActive(false);
                shop.SetActive(false);
            }
        }
    }
}
