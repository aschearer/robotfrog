using System;
using UnityEngine;

internal static class HeaderExtensions
{
    public static Vector3 ToEulerAngles(this Heading heading)
    {
        switch (heading)
        {
            case Heading.Up:
                return new Vector3(0, 0);
            case Heading.Down:
                return new Vector3(0, 180);
            case Heading.Left:
                return new Vector3(0, 90);
            case Heading.Right:
                return new Vector3(0, 270);
            default:
                throw new ArgumentException("Unknown heading");
        }
    }
}
