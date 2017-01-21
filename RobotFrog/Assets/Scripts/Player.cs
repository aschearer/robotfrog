using UnityEngine;

public class Player : MonoBehaviour {
    
	void Start () {
	}
	
	void Update () {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float fight = Input.GetAxis("Fire1");

        Debug.Log(string.Format("H: {0}. V: {1}. F: {2}", horizontal, vertical, fight));
    }
}
