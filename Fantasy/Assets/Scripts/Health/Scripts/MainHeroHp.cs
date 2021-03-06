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
    public float SkillPoint = 0;
    public float money = 0;
    public float damage = 10;
    public float ExpNum = 0;
    public float magicDamage = 10;



    private void Start()
    {
        HeroHp = maxHeroHp;
    }
    private void Update()
    {
        if (HeroHp >= 0)
        {
            level.text = "LEVEL: " + Lvl;
            moneyText.text = "$" + money;
            if (ExpNum >= 100 + Lvl * 10)
            {
                SkillPoint++;
                Lvl++;
                ExpNum = ExpNum % (100 + Lvl * 10);
                HeroHp = maxHeroHp;
            }
        }
    }

    public void SavePlayer()
    {
        SaveSystem.SavePlayer(this);
    }

    public void LoadPlayer()
    {
        PlayerData data = SaveSystem.LoadPlayer();

        HeroHp = data.HeroHp;
        maxHeroHp = data.maxHeroHp;
        SkillPoint = data.SkillPoint;
        Lvl = data.Lvl;
        money = data.money;
        damage = data.damage;
        ExpNum = data.ExpNum;
        magicDamage = data.magicDamage;

    }
}
