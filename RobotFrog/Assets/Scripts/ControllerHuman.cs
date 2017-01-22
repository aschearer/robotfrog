﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ControllerHuman : Controller {

    private int DeviceId;
    private string horizontalAxisName;
    private string verticalAxisName;
    private bool horizontalIsDown;
    private bool verticalIsDown;

    public void AssignDeviceId(int id)
    {
        this.DeviceId = id;
        this.horizontalAxisName = "Horizontal-" + this.DeviceId;
        this.verticalAxisName = "Vertical-" + this.DeviceId;
    }

    protected override void SendInput()
    {
        inputData.PrimaryWasDown = inputData.PrimaryIsDown;
        inputData.SecondaryWasDown = inputData.SecondaryIsDown;

        var threshold = 0.9f;
        switch (this.DeviceId)
        {
            case 0:
            case 1:
                threshold = 0.1f;
                break;
        }

        var horizontal = Input.GetAxis(this.horizontalAxisName);
        if (Mathf.Abs(horizontal) < threshold)
        {
            this.horizontalIsDown = false;
            inputData.HorizontalAxis = 0;
        }
        else if (!this.horizontalIsDown)
        {
            this.horizontalIsDown = true;
            inputData.HorizontalAxis = (int)Mathf.Sign(horizontal);
        }
        else
        {
            inputData.HorizontalAxis = 0;
        }

        var vertical = Input.GetAxis(this.verticalAxisName);
        if (this.horizontalIsDown || Mathf.Abs(vertical) < threshold)
        {
            this.verticalIsDown = false;
            inputData.VerticalAxis = 0;
        }
        else if (!this.verticalIsDown)
        {
            this.verticalIsDown = true;
            inputData.VerticalAxis = (int)Mathf.Sign(vertical);
        }
        else
        {
            inputData.VerticalAxis = 0;
        }

        inputData.PrimaryIsDown = Input.GetKey(GetPrimaryButton());
        inputData.SecondaryIsDown = Input.GetKey(GetSecondaryButton());
        base.SendInput();
    }

    public KeyCode GetPrimaryButton()
    {
        switch(DeviceId)
        {
            default:
            case 0: return KeyCode.LeftControl;
            case 1: return KeyCode.Space;
            case 2: return KeyCode.Joystick1Button0;
            case 3: return KeyCode.Joystick2Button0;
            case 4: return KeyCode.Joystick3Button0;
            case 5: return KeyCode.Joystick4Button0;
        }
    }

    public KeyCode GetSecondaryButton()
    {
        switch(DeviceId)
        {
            default:
            case 0: return KeyCode.Q;
            case 1: return KeyCode.RightShift;
            case 2: return KeyCode.Joystick1Button1;
            case 3: return KeyCode.Joystick2Button1;
            case 4: return KeyCode.Joystick3Button1;
            case 5: return KeyCode.Joystick4Button1;
        }
    }
}
