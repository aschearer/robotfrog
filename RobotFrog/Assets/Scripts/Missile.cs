using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{

    public Player Owner { get; internal set; }

    public Level Level { get; internal set; }

    [SerializeField]
    private int movementSpeed;

    [SerializeField]
    private float lifespan;

    [SerializeField]
    private Rigidbody myBody;

    [SerializeField]
    private float arcHeight;

    private Vector3 movementSpeedInternal;

    public Collider ProjectileCollider;
    public Collider ExplodeCollider;

    // Use this for initialization
    void Start()
    {
        ProjectileCollider.enabled = true;
        ExplodeCollider.enabled = false;
        var heading = this.Owner.Heading.ToEulerAngles();
        this.movementSpeedInternal = new Vector3(
            (int)(-this.movementSpeed * Mathf.Sin(heading.y * Mathf.Deg2Rad)),
            0,
            (int)(-this.movementSpeed * Mathf.Cos(heading.y * Mathf.Deg2Rad)));

        this.StartCoroutine(this.Explode());
    }

    // Update is called once per frame
    void Update()
    {
    }

    private IEnumerator Explode()
    {
        var thetaSpeed = Mathf.PI / this.lifespan;
        var movementSpeed = this.movementSpeedInternal / this.lifespan;
        var basePosition = this.transform.localPosition;
        for (float t = 0; t < this.lifespan; t += Time.deltaTime)
        {
            var y = Mathf.Sin(t * thetaSpeed);
            var position = t * movementSpeed;
            position.y = y * this.arcHeight;
            this.transform.localPosition = basePosition + position;

            yield return null;
        }

        ProjectileCollider.enabled = false;
        ExplodeCollider.enabled = true;
        movementSpeedInternal = Vector3.zero;
        this.ExplodeTiles();
        this.CommitSuicide();
    }

    private void CommitSuicide()
    {
        ProjectileCollider.enabled = false;
        ExplodeCollider.enabled = false;
        GameObject.Destroy(this.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        ExplodeTiles();
    }

    private void ExplodeTiles()
    {
        var column = (int)(this.transform.localPosition.x);
        var row = (int)(this.transform.localPosition.z);
        var tile = this.Level.GetTileAt(column, row);

        if (tile)
        {
            this.Level.ExplodeAt(tile, 1);
        }
    }
}
