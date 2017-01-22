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
    private int PlatformHeight;

    [SerializeField]
    private UpDown UpDownState;

    public float canMoveTimeStamp;

    public int HalfPeriodCount;

    public int Steps = 5;

    private float WaterOffset;
    private float PlatformOffset;
    private float FlippedOffset;

    private float timeTouching = 0.0f;

    private float touchingSinkTimer = 0.8f;

    void Start ()
    {
        State = DefaultState;
        Phase = ShockPhase.Peace;
        if(State == TileState.FloatingBox)
        {
            PlatformHeight = 2;
            HandleHeightChange();
        }
    }
    
    void Update () 
    {
        float DeltaTime = Time.deltaTime;
        if(IsMoving() && Time.time > canMoveTimeStamp)
        {
            canMoveTimeStamp = Time.time + 0.1f;
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
        if(State == TileState.FloatingBox)
        {   
            if(TouchingPlayers.Count > 0)
            {
                timeTouching += Time.deltaTime;
                if(PlatformHeight == 2)
                {
                    PlatformHeight--;
                    HandleHeightChange();
                }
            }
            else
            {   
                if(PlatformHeight < 2)
                {
                    PlatformHeight++;
                }
                timeTouching = 0.0f;
            }
            if(!IsMoving())
            {
                if(timeTouching > touchingSinkTimer)
                {
                    PlatformHeight--;
                    AudioManager.Instance.PlaySound(22);
                    HandleHeightChange();
                    timeTouching = 0.01f;
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
                if(Platform)
                {
                    Platform.transform.localRotation =  Quaternion.Euler(IsFlipped ? 180f: 0f, 0f, 0f);
                    FlippedOffset = IsFlipped ? 0.95f : 0.01f;
                }
            }
        }
    }

    public override IEnumerator HandleExplode(int magnitude, float delay)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }
        ShockPhase NextPhase = magnitude >= 2 ? ShockPhase.Quake : ShockPhase.Tremor;
        if((int)Phase <= (int)NextPhase)
        {
            Phase = NextPhase;
            HalfPeriodCount = magnitude*2;
            UpDownState = UpDown.MovingDown;    
        }
        yield return null;
    }


    protected override void OnPlayerTouchingAdd(Player InPlayer)
    {
        InPlayer.HandleSurfaceChange(IsSubmerged(), TotalOffset);
    }

    protected override void OnPlayerTouchingRemove(Player InPlayer)
    {
        InPlayer.HandleSurfaceRemove();
    }

    public bool IsSubmerged()
    {

        //bool bVeryHigh = Phase == ShockPhase.Quake && Height > 0;
        bool bHigh = Height > 0;
        bool bVeryLow = Phase == ShockPhase.Quake && Height < 0;
        bool bPlatformLow = PlatformHeight <= -3;
        switch(State)
        {
            case TileState.Water:
                return true;
            case TileState.SinkingPad:
            case TileState.Spike:
                return bVeryLow;
            case TileState.Rock:
                return bHigh;
            case TileState.FloatingBox:
                return bPlatformLow;
            default:
                break;
        }
        return false;


    }



    protected void HandleHeightChange()
    {
        Vector3 TilePosition = this.transform.position;
        WaterOffset = (float)Height/Steps*GetAmplitude();
        PlatformOffset = (float)PlatformHeight/Steps*0.5f;
        if(State != TileState.Rock)
        {
            TotalOffset = WaterOffset + PlatformOffset;
        }
        else
        {
            TotalOffset = 0;
        }
        TilePosition.y = WaterOffset;
        this.transform.position = TilePosition;

        if(Platform)
        {
            Platform.SetActive(!IsSubmerged());
            if(State == TileState.Rock)
            {
                Platform.transform.localPosition = -Vector3.up*WaterOffset;
            }
            else
            {
                Platform.transform.localPosition = Vector3.up*(PlatformOffset+FlippedOffset);
            }
        }

        foreach(Player player in TouchingPlayers)
        {
            if (player)
            {
                player.HandleSurfaceChange(IsSubmerged(), TotalOffset);
            }
        }
    }
}
