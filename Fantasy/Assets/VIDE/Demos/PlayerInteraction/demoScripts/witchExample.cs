using UnityEngine;
using System.Collections;

public class witchExample : MonoBehaviour {

    Color[] colors = new Color[] { Color.red, Color.black, Color.white, Color.blue, Color.yellow };

	public void ChangeColor(int color) //Change color to the one specified
    {
        if (color < colors.Length && color > 0)
        {
            GetComponent<Renderer>().material.color = colors[color];
        } else
        {
            GetComponent<Renderer>().material.color = colors[2];
        }
		GetComponent<VIDE_Assign>().overrideStartNode = 16;
    }
	 
    public void Move() //Move
    {
        transform.position += transform.forward * -5;
    }

    public void GoInvisible(bool visible)// GO invisible and change override start point
    {
        GetComponent<Renderer>().enabled = visible;

        if (!visible)
        {
            GetComponent<VIDE_Assign>().overrideStartNode = 10;
        } else
        {
            GetComponent<VIDE_Assign>().overrideStartNode = 0;
        }
    }


}
