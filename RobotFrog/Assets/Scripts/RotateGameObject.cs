using UnityEngine;
using System.Collections;

public class RotateGameObject : MonoBehaviour
{
    public float TimePer360Degrees = 1;

    private Vector3 basePosition;

    public void Start()
    {
        this.basePosition = this.transform.localPosition;
    }

    public void Update()
    {
        ////var deltaAngle = this.TimePer360Degrees * Mathf.PI * 2 * Time.deltaTime;
        ////var angles = this.transform.localEulerAngles;
        ////angles.y += deltaAngle;
        ////this.transform.localEulerAngles = angles;

        var newPosition = this.basePosition;
        newPosition.y += 0.2f * Mathf.Cos(Time.time);
        this.transform.localPosition = newPosition;
    }
}