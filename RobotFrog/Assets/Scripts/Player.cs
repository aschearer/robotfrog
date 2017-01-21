using UnityEngine;

public class Player : MonoBehaviour {

    [SerializeField]
    private Vector3 horizontalMovementSpeed;

    [SerializeField]
    private Vector3 verticalMovementSpeed;

    void Start () {
	}
	
	void Update () {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float fight = Input.GetAxis("Fire1");

        Vector3 movementVector = Vector3.zero;
        movementVector += this.horizontalMovementSpeed * horizontal;
        movementVector += this.verticalMovementSpeed * vertical;

        this.transform.localPosition += movementVector;
    }
}
