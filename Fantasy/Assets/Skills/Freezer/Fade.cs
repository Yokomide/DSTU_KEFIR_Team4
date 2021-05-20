using System.Collections;
using UnityEngine;

public class Fade : MonoBehaviour
{
    // attached game object for fading
    public GameObject Sphere;

    // fade speed length
    public float fadeSpeed;

    //Pause length between fades
    public int fadePause;

    void Awake()
    {
        StartCoroutine(FadeOut(fadeSpeed));
    }


    //Fade Out Coroutine
    public IEnumerator FadeOut(float fadeSpeed)
    {
        Debug.Log("here");
        Renderer rend = Sphere.transform.GetComponent<Renderer>();
        Color matColor = rend.material.color;
        float alphaValue = rend.material.color.a;


        //while loop to deincrement Alpha value until object is invisible
        while (rend.material.color.a > 0f)
        {
            alphaValue -= Time.deltaTime / fadeSpeed;
            rend.material.color = new Color(matColor.r, matColor.g, matColor.b, alphaValue);
            yield return new WaitForSeconds(fadePause);
        }
        rend.material.color = new Color(matColor.r, matColor.g, matColor.b, 0f);
    }
}