using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {
    // Describes how possessed objects work
    private static GameObject possessedObject;
    private Rigidbody rb;
    public float possessDistance = 20.0f;

    // The camera that represents the spoopy ghost
    public Camera playerCamera;

    // Objects in range
    private RaycastHit[] inRange;

    // Layer to check for collisions
    public int possessLayer = 9;

    // Perspectives to use when this object gets possessed
    public Vector3[] firstPerson = new Vector3[2] { Vector3.zero, Vector3.zero };
    public Vector3[] thirdPerson = new Vector3[2] { Vector3.zero, Vector3.zero };

    // Initial conditions
    public float resetTimer = 2.0f; // Resets after 2 seconds of not being possessed
    private Vector3 initialPosition;
    private Vector3 initialOrientation;
    public Vector3 initialForce = Vector3.zero; // Applies a force on the object when it first gets possessed

    
	// Use this for initialization
	void Start () {
        // Before the object is possessed, it doesn't use gravity
        rb = this.GetComponent<Rigidbody>();
        rb.useGravity = false;

        // Save initial conditions
        initialPosition = transform.position;
        initialOrientation = transform.eulerAngles;

        // The object that shares a location with the camera when the game starts is the possessed object
	    if (playerCamera.transform.position == transform.position)
        {
            possessedObject = transform.gameObject;
        }

        // Object resetter
        StartCoroutine(ResetObject());
	}
	
	// Update is called once per frame
	void Update () {
        // Detect clicks and see if the object can be possessed
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxDistance: Mathf.Infinity, layerMask: 1 << possessLayer))
            {
                // Attempt to possess the object that was hit if it's not already possessed
                if (possessedObject != hit.transform.gameObject) Possess(hit.transform.gameObject);
            }
            else
            {
                print("Nothing");
            }
        }
	}


    /* Gameplay helper methods */

    // Possess target object
    void Possess(GameObject target)
    {
        RaycastHit hit;
        PlayerController targetController = target.GetComponent<PlayerController>();

        // Attempt to raycast from the currently possessed object to the target
        if (Physics.Raycast(possessedObject.transform.position, (target.transform.position - possessedObject.transform.position), out hit, possessDistance))
        {
            if (hit.transform.gameObject == target)
            {
                possessedObject = target;

                // Adjust the camera so that its first- and third-person perspectives are relative to the new possessed object
                CameraController.Static.SetThirdPersonView(targetController.thirdPerson[0]);
                CameraController.Static.SetThirdPersonOrientation(targetController.thirdPerson[1]);
                CameraController.Static.TrackObject(target, targetController.firstPerson[0], targetController.firstPerson[1]); // Track the new object

                // Turn on gravity and apply any forces
                targetController.rb.useGravity = true;
                targetController.rb.AddForce(initialForce);
            }
            else
            {
                // Notification: Something's in the way
                print("Something's in the way...");
            }
        }
        else
        {
            // Notification: Too far
            print("Too far...");
        }
    }


    /* Co-Routines */

    // Reset an object to its initial state if it hasn't been possessed in a while
    IEnumerator ResetObject()
    {
        while (true)
        {
            // Check if the object is not in its proper configuration AND it is not currently being possessed
            if (possessedObject != this && transform.position != initialPosition && transform.eulerAngles != initialOrientation)
            {
                yield return new WaitForSeconds(resetTimer);

                // If it's still not possessed at this time, reset
                if (possessedObject != this)
                {
                    // Reset physics
                    rb.useGravity = false;
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;

                    // Reset position and orientation
                    transform.position = initialPosition;
                    transform.eulerAngles = initialOrientation;
                }
            }

            yield return null;
        }
    }
}
