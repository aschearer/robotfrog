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
    
    [SerializeField]
    private int Height;
    [SerializeField]
    private UpDown UpDownState;
    [SerializeField]
    private ManualTimer Timer;

    public int UpDownCount;

    public int Steps = 5;

    public float Amplitude = 0.25f;

	void Start ()
	{
        Timer = new ManualTimer();
        Timer.SetTime(0.1f);
        if(UpDownCount == -1)
        {
            UpDownState = UpDown.MovingUp;
        }
	}
	
	void Update () 
	{
        float DeltaTime = Time.deltaTime;
        if(IsMoving() && Timer.Tick(DeltaTime))
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
	    	if(Height == 0 && UpDownCount != -1)
    		{
    			UpDownCount--;
    		}
        }
    }
    bool IsMoving()
    {
        // -1 is an always moving floating tile
        return UpDownCount > 0 || UpDownCount == -1;
    }

    void MoveUp()
    {
        Height++;

    	// reflect
    	if(Height >= Steps)
    	{
			UpDownState = UpDown.MovingDown;
    	}
    }

    void MoveDown()
    {
        Height--;

        // reflect
        if(Height <= -Steps)
        {
            UpDownState = UpDown.MovingUp;
        }
    }

    public void AddBounceDown()
    {
        UpDownCount = 2;
        UpDownState = UpDown.MovingDown;
    }


    protected override void HandlePlayerAdd(Player InPlayer)
    {
        //InPlayer.NotifySurface(Height < 0);
    }

    protected void HandleHeightChange()
    {
        Vector3 TilePosition = this.transform.position;
        TilePosition.y = (float)Height/Steps*Amplitude;
        this.transform.position = TilePosition;
        foreach(Player dude in TouchingPlayers)
        {
            //dude.NotifySurface(Height < 0);
        }
    }
}
