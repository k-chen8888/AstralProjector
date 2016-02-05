using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    // The currently active camera
    private Camera active;

	// Information about the GameObject being tracked
	public GameObject trace = null;
	
	// Initial positions
	private Vector3 offset;
	private Vector3 orientation = new Vector3(0.0f, 0.0f, 0.0f); // Vector3(pitch, yaw, roll)
	
	// Camera movement
	public float moveSpeed = 0.5f;
    public float jumpSpeed = 500.0f;
	[Range(0, 2 * Mathf.PI)]
	public float pitchSpeed = Mathf.PI / 4.0f;
	[Range(0, 2 * Mathf.PI)]
	public float yawSpeed = Mathf.PI / 4.0f;

    // Default location
    enum CameraMode { FirstPerson, ThirdPerson, God };
    public Vector3[] views = new Vector3[3];
    public Vector3[] orientations = new Vector3[3];
    private int moveToMode = -1;
    private int isMode = (int)CameraMode.FirstPerson; // Game starts in first person
    private float percentTravelled = 1.0f;
    private float percentRotated = 1.0f;

    // 3 cameras that the script can switch between to change views
    public Camera[] cameras = new Camera[3];

    
    /* Variables for Utilities */

    // Easing function
    [Range(0, 2)]
    public float easeFactor = 1;


    // Use this for initialization
    void Start()
	{
        // Get the currently active camera
        if (cameras[isMode] != null)
        {
            active = cameras[isMode];

            if (cameras[(int)CameraMode.ThirdPerson] != null)
            {
                cameras[(int)CameraMode.ThirdPerson].enabled = false;
            }
            if (cameras[(int)CameraMode.God] != null)
            {
                cameras[(int)CameraMode.God].enabled = false;
            }
        }
        else
        {
            active = this.GetComponent<Camera>();
        }

        // Get an offset from a tracked object
		offset = transform.position - (trace == null ? Vector3.zero : trace.transform.position);

        // Assume the camera starts out in first person
        if (views[0] == Vector3.zero)
        {
            views[0] = transform.position;
        }

        // Move the camera with arrow keys, or vertically with keybindings
        StartCoroutine(MoveCamera());

        // Rotate the camera with the mouse
        StartCoroutine(RotateCamera());

        // Smooth camera movement between pre-defined waypoints
        StartCoroutine(ModeChange());
        StartCoroutine(MoveToWaypoint());

        // Switch camera view between designated cameras
        StartCoroutine(SwitchCamera());
    }
	
	// Update is called once every frame
	void Update()
	{
        
	}
	
	// LateUpdate is called after everything else updates
	void LateUpdate()
	{
        // If the camera is tracking a player or an object, follow that object if the camera is near it (use an error of 0.5f for comparing distance)
        if (trace != null && Vector3.Distance(active.transform.position, trace.transform.position) < Vector3.Distance(offset, Vector3.zero) + 0.5f)
        {
            active.transform.position = trace.transform.position + offset;
        }
	}

    
    /* Co-Routines */

    // Defines the camera controls (keyboard, moves camera)
    IEnumerator MoveCamera()
	{
        while (true)
        {
            if (percentTravelled >= 1.0)
            {
                float moveHorizontal = Input.GetAxis("CameraHorizontal");
                float moveVertical = Input.GetAxis("CameraVertical");
                float moveUpDown = Input.GetAxis("CameraUpDown"); // Set this in the InputManager

                active.transform.position += new Vector3(moveHorizontal, moveUpDown, moveVertical) * moveSpeed;
            }

            yield return null;
        }
	}
	
	// Defines the camera controls (mouse, rotates camera on the y axis ONLY)
	IEnumerator RotateCamera()
	{
        while (true)
        {
            if (percentTravelled >= 1.0)
            {
                orientation -= new Vector3(pitchSpeed * Input.GetAxis("Mouse Y"), 0.0f, 0.0f);
                active.transform.eulerAngles = orientation;
            }

            yield return null;
        }
	}

    // Switch camera modes between "First Person," "Third Person," and "God"
    IEnumerator ModeChange()
    {
        while (true)
        {
            // Move to X Mode if not moving between modes and not currently in X mode
            if (percentTravelled >= 1.0f)
            {
                if (Input.GetAxis("FirstPersonView") > 0) // Set this in the InputManager
                {
                    moveToMode = (int)CameraMode.FirstPerson;
                }
                else if (Input.GetAxis("ThirdPersonView") > 0) // Set this in the InputManager
                {
                    moveToMode = (int)CameraMode.ThirdPerson;
                }
                else if (Input.GetAxis("GodView") > 0) // Set this in the InputManager
                {
                    moveToMode = (int)CameraMode.God;
                }
                else { }
            }

            yield return null;
        }
    }

    // Move between waypoints (First person, Third person, and God)
    IEnumerator MoveToWaypoint()
    {
        float moveDistance = 0.0f,
              rotateAngle = 0.0f;
        Vector3 startLocation = Vector3.zero,
                targetLocation = Vector3.zero,
                startRotation = Vector3.zero,
                targetRotation = Vector3.zero;

        while (true)
        {
            // Perform movement
            if (percentTravelled < 1.0f || percentRotated < 1.0f)
            {
                // Calculate easing between current and target locations
                percentTravelled += (Time.deltaTime * jumpSpeed) / moveDistance;
                percentTravelled = Mathf.Clamp01(percentTravelled);
                float easedPercent = Ease(percentTravelled);

                // Calculate new position based on easing
                Vector3 newPos = Vector3.Lerp(startLocation, targetLocation, easedPercent);

                // Move to the new position and immediately go to the next iteration
                active.transform.position = newPos;
                
                // Calculate easing between current and target rotations
                percentRotated += (Time.deltaTime * jumpSpeed) / rotateAngle;
                percentRotated = Mathf.Clamp01(percentRotated);
                float easedPercentRotated = Ease(percentRotated);

                // Calculate new rotation based on easing
                Vector3 newRot = Vector3.Lerp(startRotation, targetRotation, easedPercentRotated);

                // Move to the new position and immediately go to the next iteration
                active.transform.eulerAngles = newRot;

                // Reset variables when done moving
                if (percentTravelled >= 1 && percentRotated >= 1)
                {
                    isMode = moveToMode;
                    moveToMode = -1;
                    moveDistance = 0.0f;
                    rotateAngle = 0.0f;
                    orientation = active.transform.eulerAngles;
                }

                yield return null;
            }
            else
            {
                startLocation = active.transform.position;
                startRotation = orientation;
                
                // Change the camera mode by moving if (1) there is an intent to move, and (2) there is no camera for the current mode OR there is no camera for the desired mode
                // Allow recentering on the current view by moving if the user moves the camera away from the starting point of the view (with an error of .5f) and presses the button for the current view
                if (moveToMode > -1 && Vector3.Distance(active.transform.position, views[moveToMode]) > 0.5f && (cameras[isMode] != null || cameras[moveToMode] == null))
                {
                    // Calculate distance to new location
                    targetLocation = views[moveToMode];
                    targetRotation = orientations[moveToMode];
                    moveDistance = Vector3.Distance(active.transform.position, targetLocation);
                    rotateAngle = Vector3.Distance(orientation, targetRotation);
                    
                    // Set things in motion
                    percentTravelled = 0.0f;
                    percentRotated = 0.0f;
                }
                else
                {
                    // Can't move to a place you're already at
                    moveToMode = -1;
                }
                
                yield return null;
            }
        }
    }

    // Changes the camera to the one that represents the desired view (First person, Third person, and God)
    IEnumerator SwitchCamera()
    {
        while (true)
        {
            // Check if the switch should happen
            if (moveToMode != isMode && moveToMode > -1 && cameras[moveToMode] != null)
            {
                // Change camera view
                cameras[moveToMode].enabled = true;
                cameras[isMode].enabled = false;

                // Set active camera and its index
                active = cameras[moveToMode];
                isMode = moveToMode;

            }

            yield return null;
        }
    }


    /* Utilities */

    // Movement Easing equation: y = x^a / (x^a + (1-x)^a)
    //
    // Takes x values between 0 and 1 and maps them to y values also between 0 and 1
    //  a = 1 -> straight line
    //  This is a logistic function; as a increases, y increases faster for values of x near .5 and slower for values of x near 0 or 1
    //
    // For animation, 1 < a < 3 is pretty good
    float Ease(float x)
    {
        float a = easeFactor + 1.0f;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }
}