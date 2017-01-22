using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class Automove : MonoBehaviour {

	public bool bTwoWay;
  	public Vector3 Delta;
  	public float Duration;


    IEnumerator Start ()
    {
    	float t = 0.0f;
    	if(bTwoWay)
    	{
    		float HalfDuration = Duration/2.0f;
	    	while(t < HalfDuration)
	    	{
	    		this.transform.position = this.transform.position + Time.deltaTime*Delta/HalfDuration;
	    		t += Time.deltaTime;
	    		yield return null;
	    	}
	    	while(t < Duration)
	    	{
	    		this.transform.position = this.transform.position - Time.deltaTime*Delta/HalfDuration;
	    		t += Time.deltaTime;
	    		yield return null;
	    	}
	    }
	    else
	    {
	    	while(t < Duration)
	    	{
	    		this.transform.position = this.transform.position + Time.deltaTime*Delta/Duration;
	    		t += Time.deltaTime;
	    		yield return null;
	    	}
	    }
    }
}
