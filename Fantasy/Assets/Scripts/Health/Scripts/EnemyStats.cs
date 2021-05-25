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

    [HideInInspector]
    public bool isAlive = true;

    private MeshRenderer _meshHpLine;
    private Transform _transformHpLine;

    private MeshRenderer _meshRedHpLine;
    private Transform _transformRedHpLine;

    private Animator DeathAnim;
    private NavMeshAgent agent;


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

    }


    public void AttackM(float heroDamage)
    {
        _meshRedHpLine.enabled = true;
        _meshHpLine.enabled = true;
        StartCoroutine(AttackedDelay(heroDamage));
        _transformHpLine.localScale = new Vector3(enemyHp / enemyStats.maxHp, 0.25f, 0.01f);
    }
    public void Escape()
    {
        StartCoroutine(StandardHp());

    }


    IEnumerator AttackedDelay(float heroDamage)
    {
        if (enemyHp <= 0)
        {
            Destroy(gameObject.GetComponent<BoxCollider>());
            DeathAnim.SetTrigger("Active");
     
            StartCoroutine(DeathOnCommand());
            yield break;
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
        Debug.Log(gameObject.GetComponent<Transform>().position);
        var deadBody = Instantiate(gameObject.GetComponentInChildren<SkinnedMeshRenderer>(), gameObject.GetComponentInChildren<Transform>());
        deadBody.transform.parent = Corp.transform;
        Destroy(gameObject, 4);

        _isRedHpLineDestroyed = true;
        isAlive = false;
        gameObject.GetComponent<EnemyLootDrop>().DropItems();
        audio.PlayOneShot(sound);
        yield return new WaitForEndOfFrame();
    }
    IEnumerator StandardHp()
    {
        yield return new WaitForSeconds(5f);
        Debug.Log("Дададад");
        enemyStats.hp = enemyHp;
Debug.Log(enemyStats.hp + "Из статс");
    }
}
