using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ControllerHuman : Controller {

    private int DeviceId;
    private string horizontalAxisName;
    private string verticalAxisName;

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
        inputData.HorizontalAxis = (int)Mathf.Round(Input.GetAxis(this.horizontalAxisName));
        inputData.VerticalAxis = (int)Mathf.Round(Input.GetAxis(this.verticalAxisName));
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
