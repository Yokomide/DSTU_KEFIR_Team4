using UnityEngine;
using UnityEngine.UI;

public class HealCoolDown : MonoBehaviour
{

    public float timeElapsed;

    public Image HealPng;

    private HealingSkill Heal;

    private void Start()
    {
        Heal = gameObject.GetComponent<HealingSkill>();
        timeElapsed = 0f;
    }
    private void timeUpdated()
    {
        HealPng.fillAmount = timeElapsed / Heal.healTimeCounter;
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
        timeUpdated();
    }

}
