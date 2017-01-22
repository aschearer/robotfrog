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


    [SerializeField]
    private GameObject normalModel;

    [SerializeField]
    private GameObject shrinkModel;

    [SerializeField]
    private GameObject explodeModel;

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
        var movementSpeed = (this.Target - this.transform.localPosition) / this.lifespan;
        var basePosition = this.transform.localPosition;
        for (float t = 0; t < this.lifespan; t += Time.deltaTime)
        {
            var y = Mathf.Sin(t * thetaSpeed);
            var delta = t * movementSpeed;
            delta.y = y * this.arcHeight;
            this.transform.localPosition = basePosition + delta;

            if(t > 0.3*this.lifespan && t < 0.6*this.lifespan)
            {
                SetPhase(1);
            }
            else
            {
                SetPhase(0);
            }
            yield return null;
        }

        Debug.Log(string.Format("T:{0}, A:{1}", this.Target, this.transform.localPosition));

        var position = this.transform.localPosition;
        while (position.y > -0.5f)
        {

            SetPhase(2);
            position.y -= 2f * Time.deltaTime;
            this.transform.localPosition = position;
            yield return null;
        }

        this.ExplodeTiles();
        this.CommitSuicide();
    }

    private void SetPhase(int phase)
    {

        normalModel.SetActive(phase == 0);
        shrinkModel.SetActive(phase == 1);
        explodeModel.SetActive(phase == 2);
    }

    private void CommitSuicide()
    {
        Level.MakeSplash(this.transform.position);
        GameObject.Destroy(this.gameObject);
    }

    private void ExplodeTiles()
    {
        var column = (int)Mathf.Round(this.transform.localPosition.x);
        var row = (int)Mathf.Round(-this.transform.localPosition.z);
        var tile = this.Level.GetTileAt(column, row);

        if (tile)
        {
            this.Level.ExplodeAt(tile, this.BlastRadius);
        }
    }
}
