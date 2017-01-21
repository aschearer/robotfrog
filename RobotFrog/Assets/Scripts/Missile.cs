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

    public Collider ProjectileCollider;
    public Collider ExplodeCollider;

    // Use this for initialization
    void Start () {
        ProjectileCollider.enabled = true;
        ExplodeCollider.enabled = false;
        var heading = this.Owner.Heading.ToEulerAngles();
        this.movementSpeedInternal = new Vector3(
            -this.movementpeed * Mathf.Sin(heading.y * Mathf.Deg2Rad), 
            0,
            -this.movementpeed * Mathf.Cos(heading.y * Mathf.Deg2Rad));

        this.StartCoroutine(this.Explode());
	}

    // Update is called once per frame
    void Update () {
        this.transform.localPosition += this.movementSpeedInternal * Time.deltaTime;
    }

    private IEnumerator Explode()
    {
        yield return new WaitForSeconds(this.lifespan);

        ProjectileCollider.enabled = false;
        ExplodeCollider.enabled = true;
        movementSpeedInternal = Vector3.zero;
        this.StartCoroutine(this.CommitSuicide());
    }

    private IEnumerator CommitSuicide()
    {
        yield return new WaitForSeconds(this.lifespan);

        ProjectileCollider.enabled = false;
        ExplodeCollider.enabled = false;
        GameObject.Destroy(this.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        Tile tile = other.GetComponent<Tile>();
        if(tile)
        {
            tile.HandleExplode();
        }
    }
}
