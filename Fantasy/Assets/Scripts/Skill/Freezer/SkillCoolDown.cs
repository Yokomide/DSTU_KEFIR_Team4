
using UnityEngine;
using UnityEngine.UI;

public class SkillCoolDown : MonoBehaviour
{


    public float timeElapsed;

    public Image FreezePNG;

    private FreezeSkill freeze;

    private void Start()
    {
        freeze = gameObject.GetComponentInChildren<FreezeSkill>();
        timeElapsed = 0f;
    }
    private void timeUpdated()
    {
        FreezePNG.fillAmount = timeElapsed/freeze.freezeTimeCounter;
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
        timeUpdated();
    }

}
