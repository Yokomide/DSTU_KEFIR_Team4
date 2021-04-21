using UnityEngine;

public class MouseFollow : MonoBehaviour
{

    public Player player;

    MainHeroHp _heroStats;

    private void Start()
    {
        _heroStats = gameObject.GetComponent<MainHeroHp>();
    }
    void Update()
    {
        float hitdist = 0.0f;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane playerPlane = new Plane(Vector3.up, player.transform.position);

        if (playerPlane.Raycast(ray, out hitdist) && _heroStats.heroStats.HeroHp > 0)
        {
            Vector3 targetPoint = ray.GetPoint(hitdist);
            Quaternion targetRotation = Quaternion.LookRotation(targetPoint - player.transform.position);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, player.speed * Time.deltaTime * player.sensetivity);
            player.GetComponent<Animator>().rootRotation = player.transform.rotation;
        }
    }
}
