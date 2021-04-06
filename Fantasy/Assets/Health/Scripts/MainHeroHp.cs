using UnityEngine;
using UnityEngine.UI;

public class MainHeroHp : MonoBehaviour
{
    public float HeroHp = 250f;
    public float maxHeroHp = 250f;
    public float Lvl = 0;

    public Image lvlBar;
    public Text level;

    public float _ExpNum = 0;

    private void Start()
    {
        maxHeroHp = HeroHp;
    }
    private void Update()
    {
        if (Input.GetKey(KeyCode.Minus))
        {
            HeroHp -= 0.1f;
        }

        level.text = "LEVEL: " + Lvl;
        lvlBar.fillAmount = _ExpNum / 100;
        if (_ExpNum >= 100 + Lvl * 10)
        {
            Lvl++;
            _ExpNum = 0;
            HeroHp = maxHeroHp;
        }
        lvlBar.fillAmount = _ExpNum / (100 + Lvl * 10);

    }
}
