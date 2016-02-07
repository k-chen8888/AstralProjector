using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {
    // Describes how possessed objects work
    private static GameObject possessedObject;
    private Rigidbody rb;
    public float possessDistance = 20.0f;
    public bool possessCooldown = false; // Whether or not you can possess something at the moment
    public float activateTimer = 3.0f; // After a certain amount of time, the possessed object exhibits physics properties

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
    public float initialForceMagnitude = 10.0f; // Magnitude of the initial force

    // Out of bounds death condition
    public float floor = -2.0f,
                 forwardWall = 500.0f,
                 backwardWall = -500.0f,
                 leftWall = -500.0f,
                 rightWall = 500.0f;

    
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
                // Check if on cooldown
                if (possessCooldown == false)
                {
                    // Attempt to possess the object that was hit if it's not already possessed
                    if (possessedObject != hit.transform.gameObject)
                    {
                        Possess(hit.transform.gameObject);
                    }
                }
                else
                {
                    print("On cooldown");
                }
            }
            else
            {
                print("Nothing");
            }
        }

        // Check if the possessed object is still in play
        if (possessedObject.transform.position.y < floor ||
            possessedObject.transform.position.x > rightWall || possessedObject.transform.position.x < leftWall ||
            possessedObject.transform.position.z > forwardWall || possessedObject.transform.position.z < backwardWall
            )
        {
            DeathPause.S.PauseDead();
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
                // Reset view
                CameraController.Static.GoToFirstPerson();

                // A new object has been possessed
                possessedObject = target;

                // Adjust the camera so that its first- and third-person perspectives are relative to the new possessed object
                CameraController.Static.SetThirdPersonView(targetController.thirdPerson[0]);
                CameraController.Static.SetThirdPersonOrientation(targetController.thirdPerson[1]);
                CameraController.Static.TrackObject(target, targetController.firstPerson[0], targetController.firstPerson[1]); // Track the new object

                // Now on cooldown...
                targetController.possessCooldown = true;
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
                if (possessedObject != this.transform.gameObject)
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

            // Check if on cooldown; if so, then get ready to activate physics
            if (possessCooldown == true)
            {
                // Wait for the player to get their bearings
                yield return new WaitForSeconds(resetTimer);

                // On reaching the goal, display a victory screen and ask to restart the game
                if (transform.gameObject.tag == "Goal")
                {
                    WinPause.S.PauseWin("Level0");
                }
                else
                {
                    // Otherwise, turn on gravity and apply any forces
                    rb.useGravity = true;
                    rb.AddForce(transform.rotation * Vector3.forward * initialForceMagnitude);
                }

                // No longer on cooldown
                possessCooldown = false;
            }

            yield return null;
        }
    }
}
