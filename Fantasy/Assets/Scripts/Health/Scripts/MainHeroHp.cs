using UnityEngine;
using UnityEngine.UI;

public class MainHeroHp : MonoBehaviour
{
    public Image lvlBar;
    public Text level;
    public Text moneyText;

    public float HeroHp = 250;
    public float maxHeroHp = 250;
    public float Lvl = 0;
    public float money = 0;
    public float damage = 10;
    public float ExpNum = 0;



    private void Start()
    {
        HeroHp = maxHeroHp;
    }
    private void Update()
    {
        if (HeroHp >= 0)
        {
            if (Input.GetKey(KeyCode.Minus))
            {
                HeroHp -= 0.1f;
            }

            level.text = "LEVEL: " + Lvl;
            moneyText.text = "$" + money;
            lvlBar.fillAmount = ExpNum / 100;
            if (ExpNum >= 100 + Lvl * 10)
            {
                Lvl++;
                ExpNum = 0;
                HeroHp = maxHeroHp;
            }
            lvlBar.fillAmount = ExpNum / (100 + Lvl * 10);
        }
    }
}
