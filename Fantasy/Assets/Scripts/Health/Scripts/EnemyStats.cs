using System.Collections;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public float maxHp = 100;
    public float damage = 15;
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
    public void Awake()
    {
        hp = 100;
        linePos = transform;
        _hpLine = Instantiate(hpBar, linePos);
        _hpLine.GetComponent<MeshRenderer>().enabled = false;
        _hpLineRed = Instantiate(_hpLine, linePos);
        _hpLineRed.GetComponent<MeshRenderer>().material.color = Color.red;

        _hpLine.GetComponent<Transform>().localScale = new Vector3(hp / maxHp, 0.1f, 0.01f);
        _hpLineRed.GetComponent<Transform>().localScale = _hpLine.GetComponent<Transform>().localScale;

        Corp = GameObject.Find("Corpses");
    }
    private void Update()
    {
        if (isAlive)
        {
            _hpLine.GetComponent<Transform>().position = new Vector3(transform.position.x, transform.position.y + 4, transform.position.z);
            _hpLine.GetComponent<Transform>().localScale = new Vector3(hp / maxHp, 0.1f, 0.01f);
            _hpLine.GetComponent<Transform>().rotation = Quaternion.identity;
            _hpLineRed.GetComponent<Transform>().position = new Vector3(_hpLine.transform.position.x, _hpLine.transform.position.y, _hpLine.transform.position.z + 0.02f);
            _hpLineRed.GetComponent<Transform>().rotation = Quaternion.identity;
        }
    }

    public void Attacked(float heroDamage)
    {
        if (hp > 0)
        {
            _hpLineRed.GetComponent<MeshRenderer>().enabled = true;
            _hpLine.GetComponent<MeshRenderer>().enabled = true;
            StartCoroutine(AttackedDelay(heroDamage));
            _hpLine.GetComponent<Transform>().localScale = new Vector3(hp / maxHp, 0.25f, 0.01f);
        }
        else StartCoroutine(DeathOnCommand());
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
            _hpLineRed.GetComponent<Transform>().localScale = _hpLine.GetComponent<Transform>().localScale;
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
        yield return new WaitForEndOfFrame();
    }
}









































