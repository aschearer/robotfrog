using UnityEngine;
using System.Collections.Generic;

public enum HeightLevel
{
	Peak,
	Neutral,
	Valley,
}

public enum UpDown
{
	Still,
	MovingUp,
	MovingDown,
}

public class TileFloating : Tile {
    
    public HeightLevel Height;
    public UpDown UpDownState;
    public ManualTimer Timer;
    public int UpDownCount;

	void Start ()
	{
	}
	
	void Update () 
	{
        float DeltaTime = Time.deltaTime;
        if(UpDownCount > 0 || UpDownCount == -1 && Timer.Tick(DeltaTime))
        {
        	if(UpDownState == UpDown.MovingUp)
        	{
        		MoveUp();
        	}
        	if(UpDownState == UpDown.MovingDown)
        	{
        		MoveDown();
        	}
            HandleHeightChange();
	    	if(Height == HeightLevel.Neutral)
	    	{
	    		if(UpDownCount != -1)
	    		{
	    			UpDownCount--;
	    		}
	    	}
        }
    }

    void MoveUp()
    {
    	switch(Height)
    	{
    		case HeightLevel.Valley: 
    			Height = HeightLevel.Neutral;
    			break;

    		case HeightLevel.Neutral: 
    			Height = HeightLevel.Peak;
    			break;
    		default:
    		case HeightLevel.Peak: 
    			Debug.Log("bad state moveup");
    			break;
    	}

    	// reflect
    	if(Height == HeightLevel.Peak)
    	{
			UpDownState = UpDown.MovingDown;
    	}
    }

    void MoveDown()
    {
    	switch(Height)
    	{
    		case HeightLevel.Peak: 
    			Height = HeightLevel.Neutral;
    			break;

    		case HeightLevel.Neutral: 
    			Height = HeightLevel.Valley;
    			break;
    		default:
    		case HeightLevel.Valley: 
    			Debug.Log("bad state movedown");
    			break;
    	}

    	// reflect
    	if(Height == HeightLevel.Valley)
    	{
			UpDownState = UpDown.MovingUp;
    	}
    }


    protected override void HandlePlayerAdd(Player InPlayer)
    {
        //InPlayer.NotifySurface(Height == HeightLevel.Valley);
    }

    protected void HandleHeightChange()
    {
        foreach(Player dude in TouchingPlayers)
        {
            //dude.NotifySurface(Height == HeightLevel.Valley);
        }
    }
}
