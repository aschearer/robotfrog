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
        RandomOffset = new Vector3(UnityEngine.Random.Range(-0.2f,0.2f),
            0.0f,
            UnityEngine.Random.Range(-0.2f,0.2f));
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 GoalPosition = this.transform.localPosition + RandomOffset;
        if(OwnerTile)
        {
            GoalPosition = OwnerTile.transform.localPosition + Vector3.up*(OwnerTile.TotalOffset + 0.1f); 
        }
        else if(OwnerPlayer)
        {
            GoalPosition = OwnerPlayer.transform.localPosition + Vector3.up*0.5f + RandomOffset; 
        }
        Vector3 Delta = GoalPosition - this.transform.localPosition;
        if(Delta.sqrMagnitude > 0.01f)
        {
            Delta.Normalize();
            this.transform.localPosition += Delta*Time.deltaTime*2.0f;
            Delta = GoalPosition - this.transform.localPosition;
            if(OwnerTile && Delta.sqrMagnitude <= 0.01f)
            {
                OwnerTile.OnTouchEnter(this);
            }
        }
    }


}
