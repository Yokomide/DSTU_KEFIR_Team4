using UnityEngine;
using UnityEngine.UI;

public class MainHeroHp : MonoBehaviour
{
    public HeroStatsScriptableOvject heroStats;

    public Image lvlBar;
    public Text level;
    public Text moneyText;


    private void Start()
    {
        heroStats.HeroHp = heroStats.maxHeroHp;
    }
    private void Update()
    {
        if (heroStats.HeroHp >= 0)
        {
            if (Input.GetKey(KeyCode.Minus))
            {
                heroStats.HeroHp -= 0.1f;
            }

            level.text = "LEVEL: " + heroStats.Lvl;
            moneyText.text = "$" + heroStats.money;
            lvlBar.fillAmount = heroStats.ExpNum / 100;
            if (heroStats.ExpNum >= 100 + heroStats.Lvl * 10)
            {
                heroStats.Lvl++;
                heroStats.ExpNum = 0;
                heroStats.HeroHp = heroStats.maxHeroHp;
            }
            lvlBar.fillAmount = heroStats.ExpNum / (100 + heroStats.Lvl * 10);
        }
    }
}
