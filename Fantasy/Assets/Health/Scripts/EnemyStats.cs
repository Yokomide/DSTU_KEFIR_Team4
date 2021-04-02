using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    private float _maxHp = 100;
    public float hp;
    private AttackRadiusTrigger _beingAttacked;

    public GameObject hpBar;
    private GameObject _hpLine;

    public void Start()
    {
        hp = 100;
        Vector3 linePos = new Vector3(transform.position.x, transform.position.y + 4, transform.position.z);
        _hpLine = Instantiate(hpBar, linePos, Quaternion.identity);
        _hpLine.GetComponent<MeshRenderer>().enabled = false;
    }
    private void Update()
    {
        _hpLine.GetComponent<Transform>().position = new Vector3(transform.position.x, transform.position.y + 4, transform.position.z);
    }

    public void Attacked()
    {
        _hpLine.GetComponent<MeshRenderer>().enabled = true;
        hp -= Random.Range(10, 20);
        _hpLine.GetComponent<Transform>().localScale = new Vector3(2 * hp / _maxHp, 0.25f, 0.01f);
        if (hp < 0) Destroy(_hpLine);
    }
}
