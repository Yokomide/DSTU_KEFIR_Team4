using UnityEngine.UI;
using UnityEngine;

public class PushCoolDown : MonoBehaviour
{
    public float timeElapsed;

    public Image PushPng;

    private PushSkill push;

    private void Start()
    {
        push = gameObject.GetComponent<PushSkill>();
        timeElapsed = 0f;
    }
    private void timeUpdated()
    {
        PushPng.fillAmount = timeElapsed / push.pushTimeCounter;
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
        timeUpdated();
    }
}
