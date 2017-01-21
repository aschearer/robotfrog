using UnityEngine;

public class Player : MonoBehaviour {

    [SerializeField]
    private Vector3 horizontalMovementSpeed;

    [SerializeField]
    private Vector3 verticalMovementSpeed;

    [SerializeField]
    private Missile MissilePrefab;

    [SerializeField]
    private ControllerId playerId;

    private string horizontalAxisName;

    private string verticalAxisName;

    private string fireAxisName;

    internal Heading Heading { get; private set; }

    void Start () {
        this.horizontalAxisName = "Horizontal-" + this.playerId;
        this.verticalAxisName = "Vertical-" + this.playerId;
        this.fireAxisName = "Fire1-" + this.playerId;
    }
	
	void Update () {
        float horizontal = Input.GetAxis(this.horizontalAxisName);
        float vertical = Input.GetAxis(this.verticalAxisName);

        Vector3 movementVector = Vector3.zero;
        movementVector += this.horizontalMovementSpeed * horizontal;
        movementVector += this.verticalMovementSpeed * vertical;

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

        this.transform.localPosition += movementVector;
        if (Input.GetButtonDown(this.fireAxisName))
        {
            var missile = GameObject.Instantiate(this.MissilePrefab.gameObject);
            missile.transform.SetParent(this.transform.parent);
            missile.transform.localPosition = this.transform.localPosition;
            var missileView = missile.GetComponent<Missile>();
            missileView.Owner = this;
        }
    }
}
