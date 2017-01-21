using UnityEngine;
using System.Collections.Generic;


public class Tile : MonoBehaviour {
    

    public List<Player> TouchingPlayers; // this could be only one player


    void OnTriggerEnter(Collider InCollider)
    {
    	Player InPlayer = InCollider.GetComponent<Player>();
    	if(InPlayer)
    	{
	    	if(!TouchingPlayers.Contains(InPlayer))
	    	{
	    		TouchingPlayers.Add(InPlayer);
	    	}
	    }
	    else
	    {
	    	Debug.Log("Not a player?"+InCollider);
	    }
    }

    void OnTriggerExit(Collider InCollider)
    {
    	Player InPlayer = InCollider.GetComponent<Player>();
    	if(InPlayer)
    	{
	    	if(TouchingPlayers.Contains(InPlayer))
	    	{
	    		TouchingPlayers.Remove(InPlayer);
	    	}
	    }
	    else
	    {
	    	Debug.Log("Not a player?"+InCollider);
	    }
    }

    protected virtual void HandlePlayerAdd(Player InPlayer)
    {

    }
}
