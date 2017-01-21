using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public enum TileState
{
	Wall,
	Water,
	SinkingPad,
	FloatingBox,
	Rock,
	Spike,
}

public class Tile : MonoBehaviour {
    

    public List<Player> TouchingPlayers; // this could be only one player

    internal int Column;

    internal int Row;

    public TileState State;

    void Start()
    {
    	State = TileState.Water;
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

    public virtual IEnumerator HandleExplode(float delay)
    {
        yield return null;
    }

    protected virtual void OnPlayerTouchingAdd(Player InPlayer)
    {
    	InPlayer.HandleSurfaceChange(true);
    }

    protected virtual void OnPlayerTouchingRemove(Player InPlayer)
    {
    	InPlayer.HandleSurfaceRemove();
    }
}
