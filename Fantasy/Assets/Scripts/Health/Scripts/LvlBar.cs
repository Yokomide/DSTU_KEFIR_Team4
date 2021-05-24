using UnityEngine;
using UnityEngine.UI;

public class LvlBar : MonoBehaviour
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
        fill = Mathf.Lerp(fill, _heroStats.ExpNum / (100 + _heroStats.Lvl * 10), 2 * Time.deltaTime);
        bar.fillAmount = fill;
    }
}
