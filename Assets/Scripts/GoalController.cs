using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GoalController : MonoBehaviour
{
    /* Spinning Goals */
    public float spinSpeed = 10.0f;


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Quaternion q = Quaternion.AngleAxis(Time.deltaTime * 10f, Vector3.up);
        transform.rotation = q * transform.rotation;
    }
}