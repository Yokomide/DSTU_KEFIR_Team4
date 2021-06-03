using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossStats_ : MonoBehaviour
{
    public ScrObjEnemyStats bossStats;
    public GameObject hpBar;

    [SerializeField]
    public float bossHp;

    [HideInInspector]
    public bool isBeenAttacked = false;

    [HideInInspector]
    public bool isAlive = true;


    private GameObject Corp;
    private GameObject _hpLine;
    private GameObject _hpLineRed;

    private AttackRadiusTrigger _beingAttacked;

    private bool _isRedHpLineDestroyed = false;

    private MeshRenderer _meshHpLine;
    private Transform _transformHpLine;

    private MeshRenderer _meshRedHpLine;
    private Transform _transformRedHpLine;

    private Animator DeathAnim;
    private NavMeshAgent agent;


    public AudioClip sound;

    Transform linePos;
    AudioSource audio;


    public void Awake()
    {


        gameObject.AddComponent<AudioSource>();
        audio = gameObject.GetComponent<AudioSource>();
        gameObject.GetComponent<AudioSource>().clip = sound;

        bossStats.hp = bossStats.maxHp;
        bossHp = bossStats.hp;

        DeathAnim = gameObject.GetComponent<Animator>();
        agent = gameObject.GetComponent<NavMeshAgent>();


        linePos = transform;
        _hpLine = Instantiate(hpBar, linePos);
        _meshHpLine = _hpLine.GetComponent<MeshRenderer>();
        _transformHpLine = _hpLine.GetComponent<Transform>();
        _meshHpLine.enabled = false;
        _transformHpLine.localScale = new Vector3(bossHp / bossStats.maxHp, 0.1f, 0.01f);

        _hpLineRed = Instantiate(_hpLine, linePos);
        _meshRedHpLine = _hpLineRed.GetComponent<MeshRenderer>();
        _transformRedHpLine = _hpLineRed.GetComponent<Transform>();
        _meshRedHpLine.material.color = Color.red;
        _transformRedHpLine.localScale = _transformHpLine.localScale;

        Corp = GameObject.Find("Corpses");
    }
    private void LateUpdate()
    {
        if (isAlive)
        {
            _transformHpLine.position = new Vector3(transform.position.x, transform.position.y + 4, transform.position.z);
            _transformHpLine.localScale = new Vector3(bossHp / bossStats.maxHp, 0.1f, 0.01f);
            _transformHpLine.rotation = Quaternion.identity;
            _transformRedHpLine.position = new Vector3(_hpLine.transform.position.x, _hpLine.transform.position.y, _hpLine.transform.position.z + 0.02f);
            _transformRedHpLine.rotation = Quaternion.identity;
        }
    }

    public void Attacked(float heroDamage)
    {
        _meshRedHpLine.enabled = true;
        _meshHpLine.enabled = true;
        StartCoroutine(AttackedDelay(heroDamage));
        _transformHpLine.localScale = new Vector3(bossHp / bossStats.maxHp, 0.25f, 0.01f);
    }
    public void AttackM(float heroDamage)
    {
        _meshRedHpLine.enabled = true;
        _meshHpLine.enabled = true;
        StartCoroutine(AttackedDelay(heroDamage));
        _transformHpLine.localScale = new Vector3(bossHp / bossStats.maxHp, 0.25f, 0.01f);
    }
    IEnumerator AttackedDelay(float heroDamage)
    {
        bossHp -= (heroDamage);
        if (!isAlive ||bossHp<=0)
        {
            DeathAnim.SetTrigger("Death");
            DeathAnim.Play("Death");
            Destroy(gameObject.GetComponent<BoxCollider>());
            Destroy(gameObject.GetComponent<Rigidbody>());
            StartCoroutine(DeathOnCommand());

            yield break;
        }
        yield return new WaitForSeconds(3f);
        if (!_isRedHpLineDestroyed)
        {
            _transformRedHpLine.localScale = _transformHpLine.localScale;
        }
    }

    IEnumerator DeathOnCommand()
    {
        Destroy(_hpLine);
        Destroy(_hpLineRed);

        _isRedHpLineDestroyed = true;
        isAlive = false;
        gameObject.GetComponent<EnemyLootDrop>().DropItems();
        audio.PlayOneShot(sound);
        yield return new WaitForEndOfFrame();

    }

    IEnumerator IsBeenAttacked()
    {
        isBeenAttacked = true;
        yield return new WaitForSeconds(1.3f);
        isBeenAttacked = false;
    }
}


