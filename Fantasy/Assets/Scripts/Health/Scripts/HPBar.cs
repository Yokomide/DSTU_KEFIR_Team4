using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{

    public Image bar;
    public float fill;
    public GameObject player;

    void Start()
    {
        fill = 1f;
    }

    void Update()
    {
        fill = player.GetComponent<MainHeroHp>().HeroHp / 250;
        bar.fillAmount = fill;
    }
}
