using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStats : MonoBehaviour
{
    public string enemyName = "Враг";
    public float maxHp = 100;
    public float damage;
    public float minDamage;
    public float maxDamage;
    public float hp;
    private AttackRadiusTrigger _beingAttacked;

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

    public void Awake()
    {
        DeathAnim = gameObject.GetComponent<Animator>();
        agent = gameObject.GetComponent<NavMeshAgent>();
        
 
        linePos = transform;
        _hpLine = Instantiate(hpBar, linePos);
        _meshHpLine = _hpLine.GetComponent<MeshRenderer>();
        _transformHpLine = _hpLine.GetComponent<Transform>();
        _meshHpLine.enabled = false;
        _transformHpLine.localScale = new Vector3(hp / maxHp, 0.1f, 0.01f);

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
            _transformHpLine.localScale = new Vector3(hp / maxHp, 0.1f, 0.01f);
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
        _transformHpLine.localScale = new Vector3(hp / maxHp, 0.25f, 0.01f);
    }

    IEnumerator AttackedDelay(float heroDamage)
    {
        hp -= (Random.Range(10, 20) + heroDamage);
        if (hp <= 0)
        {
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
        Destroy(_hpLine);
        Destroy(_hpLineRed);

        var deadBody = Instantiate(gameObject.GetComponentInChildren<SkinnedMeshRenderer>(), gameObject.GetComponent<Transform>());
        deadBody.transform.parent = Corp.transform;
        Destroy(gameObject, 4);

        _isRedHpLineDestroyed = true;
        isAlive = false;
        gameObject.GetComponent<EnemyLootDrop>().DropItems();
        yield return new WaitForEndOfFrame();
    }
}









































