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

    [SerializeField]
    private float movementTime = 0.6f;

    [SerializeField]
    private float arcHeight = 0.40f;

    private bool canShoot = true;

    [SerializeField]
    private float shotDelay = .00001f;

    private float canShootTimeStamp  = 0;

    private string horizontalAxisName;

    private string verticalAxisName;

    private float fireTimer;

    public GameObject flyingModel;

    public GameObject sittingModel;

    [SerializeField]
    private bool isFlying = false;

    private bool wasFlying;

    private ManualTimer landTimer = new ManualTimer();

    internal Heading Heading { get; private set; }

    [SerializeField]

    internal Level Level { get; set; }

    internal Color Tint { get; set; }

    internal Cursor Cursor { get; set; }
    public int Column { get; internal set; }
    public int Row { get; internal set; }

    void Start () {
        landTimer.SetTime(0.45f);
        landTimer.IsLooping = false;
        canShootTimeStamp = Time.time;
    }
    
    void Update ()
    {
        if(landTimer.IsTicking())
        {
            landTimer.Tick(Time.deltaTime);
        }
        if (isFlying != wasFlying)
        {
            wasFlying = isFlying;
            flyingModel.SetActive(isFlying);
            sittingModel.SetActive(!isFlying);
        }

        if(canShootTimeStamp < Time.time)
        {
            //Debug.Log("can shoot");
            canShoot = true;
        }

    }
    IEnumerator jumpAnimation(Vector3 targetLocation)
    {
        //Debug.Log("moving to " + targetLocation);

        var thetaSpeed = Mathf.PI / this.movementTime;
        var distanceToTravel = (targetLocation - this.transform.localPosition);
        var movementSpeed = distanceToTravel / this.movementTime;
        var basePosition = this.transform.localPosition;
        for (float t = 0; t < this.movementTime; t += Time.deltaTime)
        {
            var y = Mathf.Sin(t * thetaSpeed);
            var position = t * movementSpeed;
            position.y = y * arcHeight;
            this.transform.localPosition = basePosition + position;
            //Debug.Log(this.transform.localPosition + "in loop");
            yield return null;
        }

        this.transform.localPosition = targetLocation;
        landTimer.Reset();
        isFlying = false;
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
        if (isFlying == false)
        {
            if(landTimer.IsTicking())
            {
                
            }
            else
            {
                Vector3 movementVector = Vector3.zero;

                if(inputData.HorizontalAxis != 0)
                {
                    movementVector.x += inputData.HorizontalAxis;
                }
                else if(inputData.VerticalAxis != 0)
                {
                    movementVector.z += inputData.VerticalAxis;
                }
                
                if (tileStateIsValidMove(transform.localPosition + movementVector) && 
                    movementVector.sqrMagnitude > 0.1 &&
                    this.fireTimer <= 0)
                {
                    isFlying = true;
                    this.StartCoroutine(jumpAnimation(movementVector + this.transform.localPosition));
                    System.Random r = new System.Random();
                    int rInt = r.Next(0, 13);
                    GameObject.Find("Main Camera").GetComponent<AudioManager>().PlaySound(rInt);
                }
            }
            
            if(canShoot == true)
            {
                this.FireWeapon(inputData.PrimaryIsDown);
            }
            
        }
        

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
            canShoot = false;
            canShootTimeStamp = Time.time + shotDelay;
            //Debug.Log("shot fired: " + Time.time + "can shoot next -> " + canShootTimeStamp);
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
            Level.MakeElectricity(this.transform.position);
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
        //Debug.Log(string.Format("Looked at: {0},{1}", column, row));
        var tile = Level.GetTileAt(column, row);
        if (tile == null)
        {
            return false;
        }

        //Debug.Log(string.Format("Investigating tile at: {0},{1} type: {2}", tile.Column, tile.Row, tile.State));

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
