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
    private Rigidbody myBody;

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

    void Start () {
        this.horizontalAxisName = "Horizontal-" + this.playerId;
        this.verticalAxisName = "Vertical-" + this.playerId;
        this.fireAxisName = "Fire1-" + this.playerId;
    }
    
    void Update ()
    {
        float horizontal = Input.GetAxis(this.horizontalAxisName);
        float vertical = Input.GetAxis(this.verticalAxisName);

        Vector3 movementVector = Vector3.zero;
        movementVector += this.horizontalMovementSpeed * horizontal;
        movementVector += this.verticalMovementSpeed * vertical;

        this.UpdateHeading(horizontal, vertical);
        
        this.myBody.MovePosition(this.transform.position + (30 * movementVector * Time.deltaTime));

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
        }
        else if (this.fireTimer > 0)
        {
            var missile = GameObject.Instantiate(this.MissilePrefab.gameObject);
            missile.transform.SetParent(this.transform.parent);
            missile.transform.localPosition = this.transform.localPosition;
            var missileView = missile.GetComponent<Missile>();
            missileView.Owner = this;
            missileView.Level = this.Level;
            missileView.MovementSpeed = Mathf.Clamp((int)this.fireTimer, 1, 5);
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
            this.transform.localScale = 0.1f*Vector3.one;

            this.StartCoroutine(this.Respawn());
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
}
