using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class KillSelf : MonoBehaviour {

  

  	public float Duration;
  	
    IEnumerator Start ()
    {
        yield return new WaitForSeconds(Duration);
        GameObject.Destroy(this.gameObject);
    }
}
