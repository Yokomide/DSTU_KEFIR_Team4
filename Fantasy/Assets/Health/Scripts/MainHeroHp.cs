using UnityEngine;
using UnityEngine.UI;

public class MainHeroHp : MonoBehaviour
{
    public float HeroHp = 250f;
    public float maxHeroHp = 250f;
    public float Lvl = 0;
    public float money = 0;
    public float damage = 50f;

    public Image lvlBar;
    public Text level;
    public Text moneyText;

    public float _ExpNum = 0;

    private void Start()
    {
        maxHeroHp = HeroHp;
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
            lvlBar.fillAmount = _ExpNum / 100;
            if (_ExpNum >= 100 + Lvl * 10)
            {
                Lvl++;
                _ExpNum = 0;
                HeroHp = maxHeroHp;
            }
            lvlBar.fillAmount = _ExpNum / (100 + Lvl * 10);
        }
        else
        {
            //gameObject.GetComponent<Player>().GetComponent<AttackRadiusTrigger>().enemies.Clear();
            //Destroy(gameObject.GetComponent<AttackRadiusTrigger>().GetComponent<SphereCollider>());

        }
    }
}
