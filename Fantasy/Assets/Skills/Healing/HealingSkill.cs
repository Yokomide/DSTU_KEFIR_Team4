using System.Collections;
using UnityEngine;

public class HealingSkill : MonoBehaviour
{
    static float t = 0.0f;

    
    private MainHeroHp _heroStats;
    private float _healTime = 0;
    [HideInInspector]
    public bool lerping = false;
    [HideInInspector]
    public float endHealingAmount = 0;
    private GameObject tempEffect;

    public float healTimeCounter = 10;
    public int healAmount = 25;
    public GameObject effect;

    public AudioClip sound;
    AudioSource audio;

    private HealCoolDown coolDown;


    private void Start()
    {
        coolDown = gameObject.GetComponent<HealCoolDown>();
        audio = gameObject.GetComponent<AudioSource>();
        _heroStats = gameObject.GetComponent<MainHeroHp>();
        endHealingAmount = 0;
    }
    private void Update()
    {
        _healTime += Time.deltaTime;
        if (Input.GetKey(KeyCode.Alpha2) && _healTime > healTimeCounter && _heroStats.HeroHp > 0)
        {
            if (endHealingAmount == 0)
            {
                audio.PlayOneShot(sound);
                Vector3 pos = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 3, gameObject.transform.position.z);
                tempEffect = Instantiate(effect,pos,Quaternion.identity, gameObject.transform);
                endHealingAmount = _heroStats.HeroHp + healAmount;
                lerping = true;
            }
        }
        if (lerping)
        {
            
            _heroStats.HeroHp = Mathf.Lerp(_heroStats.HeroHp, _heroStats.HeroHp + healAmount, t);
            t += 0.005f * Time.deltaTime;
            if (_heroStats.HeroHp>= endHealingAmount || _heroStats.HeroHp>=_heroStats.maxHeroHp)
            {
                lerping = false;
                StartCoroutine(healPortion());
                Destroy(tempEffect);
                coolDown.timeElapsed = 0;
            }
        }
    }

    IEnumerator healPortion()
    {
        yield return new WaitForSeconds(5f);
        endHealingAmount = 0;
    }
}
