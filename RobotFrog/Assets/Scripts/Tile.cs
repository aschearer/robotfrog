using UnityEngine;
using System.Collections.Generic;


public class Tile : MonoBehaviour {
    

    public List<Player> TouchingPlayers; // this could be only one player

    void Start()
    {

    }

    void OnCollisionEnter(Collision InCollision)
    {
        Collider OtherCollider = InCollision.collider;
        Player OtherPlayer = OtherCollider.GetComponent<Player>();
        if(OtherPlayer)
        {
            if(!TouchingPlayers.Contains(OtherPlayer))
            {
                TouchingPlayers.Add(OtherPlayer);
                OnPlayerTouchingAdd(OtherPlayer);
            }
        }
        else
        {
            //Debug.Log("Not a player?"+OtherCollider);
        }
    }

    void OnCollisionExit(Collision InCollision)
    {
        Collider OtherCollider = InCollision.collider;
        Player OtherPlayer = OtherCollider.GetComponent<Player>();
        if(OtherPlayer)
        {
            if(TouchingPlayers.Contains(OtherPlayer))
            {
                TouchingPlayers.Remove(OtherPlayer);
                OnPlayerTouchingRemove(OtherPlayer);
            }
        }
    }

    public virtual void HandleExplode()
    {
    }

    protected virtual void OnPlayerTouchingAdd(Player InPlayer)
    {

    }

    protected virtual void OnPlayerTouchingRemove(Player InPlayer)
    {

    }
}
