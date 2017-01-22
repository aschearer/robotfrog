using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public enum TileState
{
	Barrier,
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
    }

    public void OnTouchEnter(Player OtherPlayer)
    {
        if(!TouchingPlayers.Contains(OtherPlayer))
        {
            TouchingPlayers.Add(OtherPlayer);
            OnPlayerTouchingAdd(OtherPlayer);
        }
    }

    public void OnTouchExit(Player OtherPlayer)
    {
        if(TouchingPlayers.Contains(OtherPlayer))
        {
            TouchingPlayers.Remove(OtherPlayer);
            OnPlayerTouchingRemove(OtherPlayer);
        }
    }

    public virtual IEnumerator HandleExplode(int magnitude, float delay)
    {
        yield return null;
    }

    protected virtual void OnPlayerTouchingAdd(Player InPlayer)
    {
    	InPlayer.HandleSurfaceChange(State == TileState.Water, 0.0f);
    }

    protected virtual void OnPlayerTouchingRemove(Player InPlayer)
    {
    	InPlayer.HandleSurfaceRemove();
    }

    public void KillPlayers()
    {
        foreach(Player player in TouchingPlayers)
        {
            if (player)
            {
                player.Die();
            }
        }
    }
}
