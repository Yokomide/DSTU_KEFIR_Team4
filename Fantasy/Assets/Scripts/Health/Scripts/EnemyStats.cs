using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStats : MonoBehaviour
{
    public ScrObjEnemyStats enemyStats;
    private AttackRadiusTrigger _beingAttacked;
    [SerializeField]
    public float enemyHp;

    private NPCBase_FSM _patrol;
    public GameObject hpBar;
    private GameObject Corp;
    Transform linePos;
    private GameObject _hpLine;
    private GameObject _hpLineRed;
    private bool _isRedHpLineDestroyed = false;

    /*[HideInInspector]*/
    public bool isAlive = true;

    private MeshRenderer _meshHpLine;
    private Transform _transformHpLine;

    private MeshRenderer _meshRedHpLine;
    private Transform _transformRedHpLine;

    private Animator DeathAnim;
    private NavMeshAgent agent;

    [HideInInspector]
    public bool isBeenAttacked = false;


    public AudioClip sound;
    AudioSource audio;

    public void Awake()
    {


        gameObject.AddComponent<AudioSource>();
        audio = gameObject.GetComponent<AudioSource>();
        gameObject.GetComponent<AudioSource>().clip = sound;

        enemyStats.hp = enemyStats.maxHp;
        enemyHp = enemyStats.hp;

        DeathAnim = gameObject.GetComponent<Animator>();
        agent = gameObject.GetComponent<NavMeshAgent>();


        linePos = transform;
        _hpLine = Instantiate(hpBar, linePos);
        _meshHpLine = _hpLine.GetComponent<MeshRenderer>();
        _transformHpLine = _hpLine.GetComponent<Transform>();
        _meshHpLine.enabled = false;
        _transformHpLine.localScale = new Vector3(enemyHp / enemyStats.maxHp, 0.1f, 0.01f);

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
            _transformHpLine.localScale = new Vector3(enemyHp / enemyStats.maxHp, 0.1f, 0.01f);
            _transformHpLine.rotation = Quaternion.identity;
            _transformRedHpLine.position = new Vector3(_hpLine.transform.position.x, _hpLine.transform.position.y, _hpLine.transform.position.z + 0.01f);
            _transformRedHpLine.rotation = Quaternion.identity;
        }
        else if (!_isRedHpLineDestroyed)
        {
            StartCoroutine(DeathOnCommand());
        }

    }


    public void AttackM()
    {
        if (!isBeenAttacked)
        {
            _meshRedHpLine.enabled = true;
            _meshHpLine.enabled = true;
            StartCoroutine(AttackedDelay());
            StartCoroutine(IsBeenAttacked());
            _transformHpLine.localScale = new Vector3(enemyHp / enemyStats.maxHp, 0.25f, 0.01f);
        }
    }
    public void Escape()
    {
        StartCoroutine(StandardHp());

    }


    IEnumerator AttackedDelay()
    {
        if (!isAlive)
        {
            Destroy(gameObject.GetComponent<BoxCollider>());
            DeathAnim.SetTrigger("Active");
            StartCoroutine(DeathOnCommand());
        }
        yield return new WaitForSeconds(2f);
        if (!_isRedHpLineDestroyed)
        {
            _transformRedHpLine.localScale = _transformHpLine.localScale;
        }
    }

    IEnumerator DeathOnCommand()
    {
        Vector3 y = new Vector3(0f, 2f, 0);
        Destroy(_hpLine);
        Destroy(_hpLineRed);
        Destroy(gameObject.GetComponent<NavMeshAgent>());
        var deadBody = Instantiate(gameObject.GetComponentInChildren<SkinnedMeshRenderer>(), gameObject.GetComponentInChildren<Transform>());
        deadBody.transform.parent = Corp.transform;
        Destroy(gameObject, 4);

        _isRedHpLineDestroyed = true;
        gameObject.GetComponent<EnemyLootDrop>().DropItems();
        audio.PlayOneShot(sound);
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        gameObject.GetComponent<Animator>().Play("Death");
        yield return new WaitForEndOfFrame();
    }
    IEnumerator StandardHp()
    {
        yield return new WaitForSeconds(5f);
        enemyStats.hp = enemyHp;
    }

    IEnumerator IsBeenAttacked()
    {
        isBeenAttacked = true;
        yield return new WaitForSeconds(1.3f);
        isBeenAttacked = false;
    }
}
