﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    [SerializeField]
    private Vector3 horizontalMovementSpeed;

    [SerializeField]
    private Vector3 verticalMovementSpeed;

    [SerializeField]
    private Missile MissilePrefab;

    private string horizontalAxisName;

    private string verticalAxisName;

    private string fireAxisName;

    private float fireTimer;

    [SerializeField]
    private GameObject flyingModel;

    [SerializeField]
    private GameObject sittingModel;

    [SerializeField]
    private bool isFlying = false;

    private bool wasFlying;

    internal Heading Heading { get; private set; }

    [SerializeField]
    internal ControllerId playerId;

    internal Level Level { get; set; }

    internal Color Tint { get; set; }

    internal Cursor Cursor { get; set; }
    public int Column { get; internal set; }
    public int Row { get; internal set; }

    void Start () {
        this.horizontalAxisName = "Horizontal-" + this.playerId;
        this.verticalAxisName = "Vertical-" + this.playerId;
        this.fireAxisName = "Fire1-" + this.playerId;
    }
    
    void Update ()
    {
        //if the check passes IE. no obstacles player moves
        float horizontal = 0;
        float vertical = 0;

        if (Level.IsGameOver)
        {
            return;
        }

        Vector3 movementVector = Vector3.zero;
        if (Input.GetButtonDown(this.horizontalAxisName) && Input.GetAxis(this.horizontalAxisName) > 0)
        {
            ++horizontal;
        }
        else if (Input.GetButtonDown(this.horizontalAxisName) && Input.GetAxis(this.horizontalAxisName) < 0)
        {
            --horizontal;
        }
        else if (Input.GetButtonDown(this.verticalAxisName) && Input.GetAxis(this.verticalAxisName) > 0)
        {
            ++vertical;
        }
        else if (Input.GetButtonDown(this.verticalAxisName) && Input.GetAxis(this.verticalAxisName) < 0)
        {
            --vertical;
        }
        
        movementVector.x += horizontal;
        movementVector.z += vertical;

        this.UpdateHeading(horizontal, vertical);

        if (this.fireTimer <= 0 && movementVector != Vector3.zero)
        {
            // adjust movementVector based on valid moves
            if (movementVector.x < 0 && !moveValid("left"))
            {
                movementVector.x = 0;
            }
            else if (movementVector.x > 0 && !moveValid("right"))
            {
                movementVector.x = 0;
            }

            if (movementVector.z < 0 && !moveValid("down"))
            {
                movementVector.z = 0;
            }
            else if (movementVector.z > 0 && !moveValid("up"))
            {
                movementVector.z = 0;
            }

            this.transform.localPosition = this.transform.localPosition + movementVector;
            this.Column = (int)Mathf.Round(this.transform.localPosition.x);
            this.Row = -(int)Mathf.Round(this.transform.localPosition.z);
            Debug.Log(string.Format("Updated column, row: {0},{1}", this.Column, this.Row));
        }

        this.FireWeapon();

        if (isFlying != wasFlying)
        {
            wasFlying = isFlying;
            flyingModel.SetActive(isFlying);
            sittingModel.SetActive(!isFlying);
        }
    }

    private void FireWeapon()
    {
        if (Input.GetButton(this.fireAxisName))
        {
            this.fireTimer += Time.deltaTime * 2f;
            this.Cursor.ShowLine(2);
        }
        else if (this.fireTimer > 0)
        {

            this.Cursor.ShowLine(-1);
            var column = (int)Mathf.Round(this.transform.localPosition.x);
            var row = (int)-Mathf.Round(this.transform.localPosition.z);

            var heading = this.Heading.ToEulerAngles();
            var distance = new Vector3(
                (int)(3 * Mathf.Sin(heading.y * Mathf.Deg2Rad)),
                0,
                (int)(3 * Mathf.Cos(heading.y * Mathf.Deg2Rad)));

            var targetColumn = column + distance.x;
            var targetRow = row - distance.z;

            var missile = GameObject.Instantiate(this.MissilePrefab.gameObject);
            missile.transform.SetParent(this.transform.parent);
            missile.transform.localPosition = this.transform.localPosition;
            var missileView = missile.GetComponent<Missile>();
            missileView.Owner = this;
            missileView.Level = this.Level;
            missileView.Target = new Vector3(targetColumn, 0, -targetRow);
            this.fireTimer = 0;
        }
    }

    private void UpdateHeading(float horizontal, float vertical)
    {
        Heading? heading = null;
        if (!Mathf.Approximately(horizontal, 0))
        {
            if (Mathf.Sign(horizontal) > 0)
            {
                heading = Heading.Left;
            }
            else if (Mathf.Sign(horizontal) < 0)
            {
                heading = Heading.Right;
            }
        }
        else if (!Mathf.Approximately(vertical, 0))
        {
            if (Mathf.Sign(vertical) > 0)
            {
                heading = Heading.Up;
            }
            else if (Mathf.Sign(vertical) < 0)
            {
                heading = Heading.Down;
            }
        }

        if (heading.HasValue && this.Heading != heading)
        {
            this.Heading = heading.Value;
            this.transform.localEulerAngles = this.Heading.ToEulerAngles();
        }
    }

    public void HandleSurfaceChange(bool bIsBelowWater)
    {
        isFlying = false;
        if(bIsBelowWater)
        {
            // die
            GameObject.Destroy(this.gameObject);
        }
    }

    public void HandleSurfaceRemove()
    {
        isFlying = true;
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(3.0f);
        this.transform.localScale = Vector3.one;
    }

    bool moveValid(string attemptedMove)
    {
        Vector3 tempPosition = transform.localPosition;
        if(attemptedMove.CompareTo("up") == 0)
        {
            ++tempPosition.z;
            return tileStateIsValidMove(tempPosition);
        }
        else if (attemptedMove.CompareTo("down") == 0)
        {
            --tempPosition.z;
            return tileStateIsValidMove(tempPosition);
        }
        else if (attemptedMove.CompareTo("right") == 0)
        {
            ++tempPosition.x;
            return tileStateIsValidMove(tempPosition);
        }
        else if (attemptedMove.CompareTo("left") == 0)
        {
            --tempPosition.x;
            return tileStateIsValidMove(tempPosition);
        }

        return false;
    }

    bool tileStateIsValidMove(Vector3 nextPosition)
    {
        int column = (int)Mathf.Round(nextPosition.x);
        int row = (int)Mathf.Round(-nextPosition.z);
        Debug.Log(string.Format("Looked at: {0},{1}", column, row));
        var tile = Level.GetTileAt(column, row);
        if (tile == null)
        {
            return false;
        }

        Debug.Log(string.Format("Investigating tile at: {0},{1} type: {2}", tile.Column, tile.Row, tile.State));

        bool isValidMove = false;
        TileState state = tile.State;
        switch (state)
        {
            case TileState.SinkingPad:
            case TileState.FloatingBox:
            case TileState.Rock:
                isValidMove = true;
                break;
            default:
                isValidMove = false;
                break;
        }

        if (isValidMove)
        {
            // Valid tile, but is it occupied?
            var player = Level.GetPlayerAt(column, row);
            isValidMove = player == null;
        }

        return isValidMove;
    }
}
