using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour {

    public Player Owner { get; internal set; }

    [SerializeField]
    private float movementpeed;

    [SerializeField]
    private float lifespan;

    private Vector3 movementSpeedInternal;

    // Use this for initialization
    void Start () {
        var heading = this.Owner.Heading.ToEulerAngles();
        this.movementSpeedInternal = new Vector3(
            -this.movementpeed * Mathf.Sin(heading.y * Mathf.Deg2Rad), 
            0,
            -this.movementpeed * Mathf.Cos(heading.y * Mathf.Deg2Rad));

        this.StartCoroutine(this.CommitSuicide());
	}

    // Update is called once per frame
    void Update () {
        this.transform.localPosition += this.movementSpeedInternal * Time.deltaTime;
    }

    private IEnumerator CommitSuicide()
    {
        yield return new WaitForSeconds(this.lifespan);
        GameObject.Destroy(this.gameObject);
    }
}
