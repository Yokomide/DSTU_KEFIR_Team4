using System.Collections;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public float maxHp = 100;
    public float damage = 15;
    public float hp;
    private AttackRadiusTrigger _beingAttacked;

    public GameObject hpBar;
    Transform linePos;
    private GameObject _hpLine;

    [HideInInspector]
    public bool isAlive = true;
    public void Awake()
    {
        hp = 100;
        linePos = transform;
        _hpLine = Instantiate(hpBar, linePos);
        _hpLine.GetComponent<MeshRenderer>().enabled = false;
    }
    private void Update()
    {
        if (isAlive)
        {
            _hpLine.GetComponent<Transform>().position = new Vector3(transform.position.x, transform.position.y + 4, transform.position.z);
            _hpLine.GetComponent<Transform>().localScale = new Vector3( hp / maxHp, 0.1f,0.01f);
            _hpLine.GetComponent<Transform>().rotation = Quaternion.identity;
        }
    }

    public void Attacked(float heroDamage)
    {
        _hpLine.GetComponent<MeshRenderer>().enabled = true;
       // Vector3 pos = new Vector3(_hpLine.transform.position.x, _hpLine.transform.position.y, _hpLine.transform.position.z+0.02f);
       // GameObject _hpLineRed = Instantiate(_hpLine, linePos);
        //_hpLineRed.transform.position = pos;
        StartCoroutine(AttackedDelay(heroDamage));
        //_hpLineRed.GetComponent<MeshRenderer>().material.color = Color.red;

        _hpLine.GetComponent<Transform>().localScale = new Vector3(hp / maxHp, 0.25f, 0.01f);
        if (hp<=0)
        {
            Destroy(_hpLine);
            //Destroy(_hpLineRed);
            isAlive = false;
        }
    }

    IEnumerator AttackedDelay(/*GameObject redLine,*/float heroDamage)
    {
        hp -= (Random.Range(10, 20)+heroDamage);
        yield return new WaitForSeconds(2f);
        //Destroy(redLine);
    }
}
