using UnityEngine;

public class EventManager : MonoBehaviour
{
    public delegate void StatsChange();
    public static event StatsChange OnStatsChange;

    public static void OnGetAttack()
    {
        if (OnStatsChange != null)
        {
            OnStatsChange();
        }
    }
}
