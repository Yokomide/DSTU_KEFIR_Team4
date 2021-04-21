using UnityEngine;

public class cloud : MonoBehaviour
{
    void Update()
    {
        gameObject.transform.Rotate(0, 1 * Time.deltaTime, 0);
    }
}
