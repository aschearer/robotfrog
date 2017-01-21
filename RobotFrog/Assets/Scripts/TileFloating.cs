using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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

    public GameObject Platform;

    public bool IsFlipped;

    public TileState DefaultState;
    public TileState FlippedState;
    
    [SerializeField]
    private int Height;
    [SerializeField]
    private UpDown UpDownState;
    [SerializeField]
    private ManualTimer Timer;

    public int UpDownCount;

    public int Steps = 5;

    protected float Amplitude = 0.75f;

    void Start ()
    {
        State = DefaultState;
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
            if(DefaultState != FlippedState)
            {
                IsFlipped = !IsFlipped;
                State = IsFlipped ? FlippedState : DefaultState;
                Platform.transform.localRotation =  Quaternion.Euler(IsFlipped ? 180f: 0f, 0f, 0f);
                Platform.transform.localPosition = Vector3.up * (IsFlipped ? 0.95f : 0.01f);
            }
        }
    }

    public override IEnumerator HandleExplode(float delay)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        UpDownCount = 2;
        UpDownState = UpDown.MovingDown;
        yield return null;
    }


    protected override void OnPlayerTouchingAdd(Player InPlayer)
    {
        InPlayer.HandleSurfaceChange(Height < 0);
    }

    protected override void OnPlayerTouchingRemove(Player InPlayer)
    {
        InPlayer.HandleSurfaceRemove();
    }

    protected void HandleHeightChange()
    {
        Vector3 TilePosition = this.transform.position;
        float VerticalOffset = (float)Height/Steps*Amplitude;
        TilePosition.y = VerticalOffset;
        this.transform.position = TilePosition;
        switch(State)
        {
            case TileState.SinkingPad:
            case TileState.Spike:
                Platform.SetActive(Height >= 0);
                break;
            case TileState.Rock:
                Platform.SetActive(Height <= 0);
                Platform.transform.localPosition = -Vector3.up*VerticalOffset;
                break;
        }

        foreach(Player player in TouchingPlayers)
        {
            if (player)
            {
                player.HandleSurfaceChange(Height < 0);
            }
        }
    }
}
