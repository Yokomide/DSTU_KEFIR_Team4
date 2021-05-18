using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{

    public Image bar;
    public float fill;
    public GameObject player;

    private MainHeroHp _heroStats;

    void Start()
    {
        _heroStats = player.GetComponent<MainHeroHp>();
        fill = 1f;
    }

    void Update()
    {
        fill = _heroStats.HeroHp / _heroStats.maxHeroHp;
        bar.fillAmount = fill;
    }
}
