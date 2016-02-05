using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    // Check if the object that the 
    private bool isPossessed = false;

    // The camera that represents the spoopy ghost
    public Camera playerCamera;

	// Use this for initialization
	void Start () {
        // The object that shares a location with the camera when the game starts is the possessed objects
	    if (playerCamera.transform.position == transform.position)
        {
            isPossessed = true;
        }
	}
	
	// Update is called once per frame
	void Update () {
	    if (isPossessed && Input.GetAxis("PossessObject") > 0)
        {
            print("whooo");
        }
	}
}
