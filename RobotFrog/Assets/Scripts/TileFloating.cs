using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public enum ShockPhase
{
    Peace,
    Tremor,
    Quake,
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
    private ShockPhase Phase;
    
    [SerializeField]
    private int Height;
    [SerializeField]
    private UpDown UpDownState;
    [SerializeField]
    private ManualTimer Timer;

    public int HalfPeriodCount;

    public int Steps = 5;

    void Start ()
    {
        State = DefaultState;
        Timer = new ManualTimer();
        Timer.SetTime(0.1f);
        Phase = ShockPhase.Peace;
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
            if(Height == 0)
            {
                HalfPeriodCount--;
                if(HalfPeriodCount == 0)
                {
                    Phase = ShockPhase.Peace;
                }
                else if(UpDownState == UpDown.MovingUp && Phase == ShockPhase.Quake)
                {
                    Phase = ShockPhase.Tremor;
                }
            }
        }
    }

    bool IsMoving()
    {
        return Phase != ShockPhase.Peace;
    }

    float GetAmplitude()
    {
        return Phase == ShockPhase.Quake ? 0.8f : 0.3f;
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
            if(Phase == ShockPhase.Quake && DefaultState != FlippedState)
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

        Phase = ShockPhase.Quake;
        HalfPeriodCount = 4;
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
        float VerticalOffset = (float)Height/Steps*GetAmplitude();
        TilePosition.y = VerticalOffset;
        this.transform.position = TilePosition;
        bool bVeryHigh = Phase == ShockPhase.Quake && Height > 0;
        bool bVeryLow = Phase == ShockPhase.Quake && Height < 0;
        switch(State)
        {
            case TileState.SinkingPad:
            case TileState.Spike:
                Platform.SetActive(!bVeryLow);
                break;
            case TileState.Rock:
                Platform.SetActive(!bVeryHigh);
                Platform.transform.localPosition = -Vector3.up*VerticalOffset;
                break;
        }

        foreach(Player player in TouchingPlayers)
        {
            if (player)
            {
                player.HandleSurfaceChange(!bVeryLow);
            }
        }
    }
}
