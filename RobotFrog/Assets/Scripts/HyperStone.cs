using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HyperStone : MonoBehaviour
{
    

    public Tile OwnerTile;
    public Player OwnerPlayer;

    private Vector3 RandomOffset;


    // Use this for initialization
    void Start()
    {
        RandomOffset = new Vector3(UnityEngine.Random.Range(-0.5f,0.5f),
            0.0f,
            UnityEngine.Random.Range(-0.5f,0.5f));
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 GoalPosition = this.transform.localPosition + RandomOffset;
        if(OwnerTile)
        {
            GoalPosition = OwnerTile.transform.localPosition + Vector3.up*(OwnerTile.TotalOffset + 0.2f); 
        }
        else if(OwnerPlayer)
        {
            GoalPosition = OwnerPlayer.transform.localPosition + Vector3.up*1.5f + RandomOffset; 
        }
        Vector3 Delta = GoalPosition - this.transform.localPosition;
        if(Delta.sqrMagnitude > 0.25f)
        {
            Delta.Normalize();
            this.transform.localPosition += Delta*Time.deltaTime;
            Delta = GoalPosition - this.transform.localPosition;
            if(OwnerTile && Delta.sqrMagnitude <= 0.25f)
            {
                OwnerTile.OnTouchEnter(this);
            }
        }
        transform.Rotate(Time.deltaTime, Time.deltaTime*0.6f, 0);
    }


}
