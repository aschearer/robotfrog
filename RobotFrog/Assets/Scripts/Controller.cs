using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct InputData
{
    public bool PrimaryIsDown;
    public bool PrimaryWasDown;
    public bool SecondaryIsDown;
    public bool SecondaryWasDown;
    public int HorizontalAxis;
    public int VerticalAxis;
}
public class Controller : MonoBehaviour {

    public Player Pawn;

    public bool isInTheGame = false;

    public InputData inputData;

    void Update()
    {
        SendInput();
    }

    protected virtual void SendInput()
    {
        if(Pawn)
        {
            Pawn.HandleInput(inputData);
        }
    }
}
