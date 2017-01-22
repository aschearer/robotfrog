using System;
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

    private float fireTimer;

    [SerializeField]
    private GameObject flyingModel;

    [SerializeField]
    private GameObject sittingModel;

    [SerializeField]
    private bool isFlying = false;

    private bool wasFlying;

    private ManualTimer moveTimer = new ManualTimer();

    internal Heading Heading { get; private set; }

    [SerializeField]

    internal Level Level { get; set; }

    internal Color Tint { get; set; }

    internal Cursor Cursor { get; set; }
    public int Column { get; internal set; }
    public int Row { get; internal set; }

    void Start () {
        moveTimer.SetTime(1.0f);
        moveTimer.IsLooping = false;
    }
    
    void Update ()
    {
        moveTimer.Tick(Time.deltaTime);

        if (isFlying != wasFlying)
        {
            wasFlying = isFlying;
            flyingModel.SetActive(isFlying);
            sittingModel.SetActive(!isFlying);
        }
    }

    public void HandleInput(InputData inputData)
    {
        if (Level.levelState == LevelState.GameOver)
        {
            return;
        }

        this.UpdateHeading(inputData.HorizontalAxis, inputData.VerticalAxis);
        if (Level.levelState == LevelState.WaitingToSpawn)
        {
            return;
        }
        if(!moveTimer.IsTicking())
        {
            Vector3 movementVector = Vector3.zero;


            movementVector.x += inputData.HorizontalAxis;
            movementVector.z += inputData.VerticalAxis;
            if (this.fireTimer <= 0 && movementVector != Vector3.zero)
            {
                Vector3 desiredLocation = this.transform.localPosition + movementVector;

                if(tileStateIsValidMove(desiredLocation))
                {
                    this.transform.localPosition = desiredLocation;
                    this.Column = (int)Mathf.Round(this.transform.localPosition.x);
                    this.Row = -(int)Mathf.Round(this.transform.localPosition.z);
            
                    moveTimer.Reset();
                }
            }
        }

        this.FireWeapon(inputData.PrimaryIsDown);

    }

    private void FireWeapon(bool isFiring)
    {
        if (isFiring)
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
