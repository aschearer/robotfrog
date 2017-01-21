using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public int BlastRadius = 1;

    public Player Owner { get; internal set; }

    public Level Level { get; internal set; }

    public Vector3 Target { get; internal set; }

    [SerializeField]
    private float lifespan;

    [SerializeField]
    private float arcHeight;

    // Use this for initialization
    void Start()
    {
        this.StartCoroutine(this.Explode());
    }

    // Update is called once per frame
    void Update()
    {
    }

    private IEnumerator Explode()
    {
        var thetaSpeed = Mathf.PI / this.lifespan;
        var logicalPosition = this.transform.localPosition;
        logicalPosition.y = 0;
        var movementSpeed = (this.Target - this.transform.localPosition);
        var basePosition = this.transform.localPosition;
        for (float t = 0; t < this.lifespan; t += Time.deltaTime)
        {
            var y = Mathf.Sin(t * thetaSpeed);
            var position = t * movementSpeed;
            position.y = y * this.arcHeight;
            this.transform.localPosition = basePosition + position;

            yield return null;
        }

        this.transform.localPosition = this.Target;

        this.ExplodeTiles();
        this.CommitSuicide();
    }

    private void CommitSuicide()
    {
        GameObject.Destroy(this.gameObject);
    }

    private void ExplodeTiles()
    {
        var column = (int)(this.transform.localPosition.x);
        var row = (int)(-this.transform.localPosition.z);
        var tile = this.Level.GetTileAt(column, row);

        if (tile)
        {
            this.Level.ExplodeAt(tile, this.BlastRadius);
        }
    }
}
