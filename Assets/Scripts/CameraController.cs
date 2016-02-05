using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
	// Information about the GameObject being tracked
	public GameObject trace = null;
	
	// Initial positions
	private Vector3 offset;
	private float pitch = 0.0f;
	
	// Camera movement
	public float moveSpeed = 0.5f;
    public float jumpSpeed = 10.0f;
	[Range(0, 2 * Mathf.PI)]
	public float pitchSpeed = Mathf.PI / 4.0f;
	[Range(0, 2 * Mathf.PI)]
	public float yawSpeed = Mathf.PI / 4.0f;


    // Default location
    enum CameraMode { FirstPerson, ThirdPerson, God };
    public Vector3 firstPerson;
	public Vector3 thirdPerson;
    public Vector3 god;
    private int moveToMode = -1;
    private int isMode = (int)CameraMode.FirstPerson;
    private float percentTravelled = 1.0f;


    /* Variables for Utilities */

    // Easing function
    [Range(0, 2)]
    public float easeFactor = 1;


    // Use this for initialization
    void Start()
	{
		firstPerson = offset = transform.position - (trace == null ? Vector3.zero : trace.transform.position);

        // Assume the camera starts out in first person
        firstPerson = transform.position;

        // Move the camera with arrow keys, or vertically with keybindings
        StartCoroutine(MoveCamera());

        // Rotate the camera with the mouse
        StartCoroutine(RotateCamera());

        // Smooth camera movement between pre-defined waypoints
        StartCoroutine(ModeChange());
        StartCoroutine(MoveToWaypoint());
    }
	
	// Update is called once every frame
	void Update()
	{
        
	}
	
	// LateUpdate is called after everything else updates
	void LateUpdate()
	{
		// If the camera is tracking a player or an object, follow that object
		if (trace != null) transform.position = trace.transform.position + offset;
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

                transform.position += new Vector3(moveHorizontal, moveUpDown, moveVertical) * moveSpeed;
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
                pitch -= pitchSpeed * Input.GetAxis("Mouse Y");
                transform.eulerAngles = new Vector3(pitch, 0.0f, 0.0f);
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
        float moveDistance = 0.0f;
        Vector3 startLocation = Vector3.zero,
                targetLocation = Vector3.zero;

        while (true)
        {
            // Perform movement
            if (percentTravelled < 1.0f)
            {
                // Calculate easing between current and target locations
                percentTravelled += (Time.deltaTime * jumpSpeed) / moveDistance;
                percentTravelled = Mathf.Clamp01(percentTravelled);
                float easedPercent = Ease(percentTravelled);

                // Calculate new position based on easing
                Vector3 newPos = Vector3.Lerp(startLocation, targetLocation, easedPercent);

                // Move to the new position and immediately go to the next iteration
                transform.position = newPos;

                // Reset variables when done moving
                if (percentTravelled >= 1)
                {
                    isMode = moveToMode;
                    moveToMode = -1;
                    moveDistance = 0.0f;

                    // God Mode looks down on the plebs
                    if (isMode == (int)CameraMode.God)
                    {
                        pitch = 90.0f;
                    }
                    else
                    {
                        pitch = 0.0f;
                    }
                }

                yield return null;
            }
            else
            {
                startLocation = offset;
                // Calculate distance to destination
                if (isMode != moveToMode && moveToMode == (int)CameraMode.ThirdPerson)
                {
                    targetLocation = thirdPerson;
                    moveDistance = Vector3.Distance(transform.position, thirdPerson);
                    
                    // Set things in motion
                    percentTravelled = 0.0f;
                }
                else if (isMode != moveToMode && moveToMode == (int)CameraMode.God)
                {
                    targetLocation = god;
                    moveDistance = Vector3.Distance(transform.position, god);
                    
                    // Set things in motion
                    percentTravelled = 0.0f;
                }
                else if (isMode != moveToMode && moveToMode == (int)CameraMode.FirstPerson)
                {
                    targetLocation = firstPerson;
                    moveDistance = Vector3.Distance(transform.position, firstPerson);
                    
                    // Set things in motion
                    percentTravelled = 0.0f;
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