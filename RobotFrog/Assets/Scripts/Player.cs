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

    private bool canShoot = false;

    [SerializeField]
    private float shotDelay = .00001f;

    private float canShootTimeStamp  = 0;

    private string horizontalAxisName;

    private string verticalAxisName;

    private float fireTimer;

    private int fireRange;

    public GameObject flyingModel;

    public GameObject sittingModel;

    public float playerHeight = 1.0f;

    private float tileHeight = 0.0f;

    [SerializeField]
    private bool isFlying = false;

    private bool canJump = true;

    private bool wasFlying;

    private bool canPound = true;

    private float canPoundTimeStamp  = 0;

    private bool isPounding = false;

    private bool bPoundingUp = false;

    public bool aimSoundPlaying = true;

    private float PoundFloatHeight = 0.0f;

    private float PoundUpSpeed = 2.0f;
    private float PoundDownSpeed = 4.0f;
    private float PoundPeakHeight = 0.8f;


    internal Heading Heading { get; private set; }

    [SerializeField]

    internal Level Level { get; set; }

    internal Color Tint { get; set; }

    public int JumpSound { get; internal set; }

    internal Cursor Cursor { get; set; }
    public int Column { get; internal set; }
    public int Row { get; internal set; }

    public int Strength = 0;

    void Start () {
        canPound = false;
        canPoundTimeStamp = Time.time + 0.25f;
        canShoot = false;
        canShootTimeStamp = Time.time + 0.25f;
    }
    
    void Update ()
    {
        
        if(canPoundTimeStamp < Time.time)
        {
            canPound = true;
        }
        if(isPounding)
        {
            if(bPoundingUp)
            {
                PoundFloatHeight += Time.deltaTime*PoundUpSpeed;
                if(PoundFloatHeight > PoundPeakHeight)
                {
                    bPoundingUp = false;
                }
            }
            else
            {
                PoundFloatHeight -= Time.deltaTime*PoundDownSpeed;
            }
        }
        if (isFlying != wasFlying)
        {
            wasFlying = isFlying;
            flyingModel.SetActive(isFlying);
            sittingModel.SetActive(!isFlying);
        }

        if(canShootTimeStamp < Time.time)
        {
            canShoot = true;
        }

    }

    public void GameOver()
    {
        this.Cursor.ShowLine(-1);
    }

    IEnumerator jumpAnimation(Vector3 targetLocation)
    {
        //Debug.Log("moving to " + targetLocation);

        isFlying = true;
        canJump = false;
        var lastTile = this.Level.GetTileAt(Column, Row);
        lastTile.OnTouchExit(this);
        if(lastTile.State == TileState.SinkingPad || lastTile.State == TileState.FloatingBox)
        {
            Level.ExplodeAt(lastTile, 0, 1);
        }

        this.Column = (int)Mathf.Round(targetLocation.x);
        this.Row = (int)Mathf.Round(-targetLocation.z);

        var nextTile = this.Level.GetTileAt(Column, Row);
        //Debug.Log(string.Format("C:{0},R:{1}", this.Column, this.Row));

        var thetaSpeed = Mathf.PI / this.movementTime;
        var distanceToTravel = (targetLocation - this.transform.localPosition);
        var movementSpeed = distanceToTravel / this.movementTime;
        var basePosition = this.transform.localPosition;
        for (float t = 0; t < this.movementTime; t += Time.deltaTime)
        {
            var y = Mathf.Sin(t * thetaSpeed);
            var position = t * movementSpeed;
            position.y = y * arcHeight;
            if(t > this.movementTime*0.5f)
            {
                position.y += nextTile.TotalOffset;
            }
            this.transform.localPosition = basePosition + position;
            //Debug.Log(this.transform.localPosition + "in loop");
            yield return null;
        }

        this.transform.localPosition = targetLocation;
        isFlying = false;
        nextTile.OnTouchEnter(this);
        yield return new WaitForSeconds(0.15f);
        canJump = true;
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
        if (canJump)
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
            
            if (movementVector.sqrMagnitude > 0.1 &&
                this.fireTimer <= 0 &&
                !isPounding &&
                tileStateIsValidMove(transform.localPosition + movementVector))
            {
                this.StartCoroutine(jumpAnimation(movementVector + this.transform.localPosition));
                AudioManager.Instance.PlaySound(JumpSound);
            }
        }
        if(canShoot == true)
        {
            this.FireWeapon(inputData.PrimaryIsDown);
            this.PoundWeapon(inputData.SecondaryIsDown, inputData.SecondaryWasDown && !inputData.SecondaryIsDown);
        }
    }
    
    private void FireWeapon(bool isFiring)
    {
        if (isFiring)
        {
            this.fireTimer += Time.deltaTime * 2;
            this.fireRange = 1 + Strength + ((int)this.fireTimer % 4);
            this.Cursor.ShowLine(fireRange);
            if(!aimSoundPlaying)
            {
                AudioManager.Instance.PlaySound(23);
                aimSoundPlaying = true;
            }
        }
        else if (this.fireTimer > 0)
        {
            aimSoundPlaying = false;
            this.Cursor.ShowLine(-1);
            var column = (int)Mathf.Round(this.transform.localPosition.x);
            var row = (int)-Mathf.Round(this.transform.localPosition.z);
            var heading = this.Heading.ToEulerAngles();
            int distanceTiles = fireRange;
            var distance = new Vector3(
                (int)(distanceTiles * Mathf.Sin(heading.y * Mathf.Deg2Rad)),
                0,
                (int)(distanceTiles * Mathf.Cos(heading.y * Mathf.Deg2Rad)));

            var targetColumn = column + distance.x;
            var targetRow = row - distance.z;

            var missile = GameObject.Instantiate(this.MissilePrefab.gameObject);
            missile.transform.SetParent(this.transform.parent);
            missile.transform.localPosition = this.transform.localPosition;
            var missileView = missile.GetComponent<Missile>();
            missileView.Owner = this;
            missileView.Level = this.Level;
            missileView.Target = new Vector3(targetColumn, 0, -targetRow);
            missileView.BlastRadius = 1 + this.Strength;
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

    public void HandleSurfaceChange(bool bIsBelowWater, float tileHeight)
    {
        isFlying = false;
        this.tileHeight = tileHeight;
        if(!isPounding && !isFlying)
        {
            Vector3 nextPosition = this.transform.localPosition;
            nextPosition.y = this.playerHeight + this.tileHeight;
            this.transform.localPosition = nextPosition;
        }
        if(bIsBelowWater)
        {
            Die();
        }
    }

    public void Die()
    {
        Level.MakeElectricity(this.transform.position);
        // die
        AudioManager.Instance.PlaySound(16);
        GameObject.Destroy(this.gameObject);
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

    void PoundWeapon(bool bIsDown, bool bIsReleased)
    {
        if(bIsDown && !isPounding && canPound)
        {
            AudioManager.Instance.PlaySound(19);
            isPounding = true;
            bPoundingUp = true;
            PoundFloatHeight = 0.5f;
        }
        if(isPounding)
        {
            this.Cursor.ShowLine(0);
            if(!bPoundingUp && PoundFloatHeight < 0.0f)
            {
                PoundFloatHeight = 0.0f;

                var tile = this.Level.GetTileAt(Column, Row);
                this.Level.ExplodeAt(tile, 1+Strength, 2, false);
                canPound = false;
                canPoundTimeStamp = Time.time + shotDelay;
                this.Cursor.ShowLine(-1);
                isPounding = false;
            }
            Vector3 nextPosition = this.transform.localPosition;
            nextPosition.y = this.playerHeight + this.tileHeight + this.PoundFloatHeight;
            this.transform.localPosition = nextPosition;
        }       
    }
}
