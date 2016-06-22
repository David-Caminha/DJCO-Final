using System;
using UnityEngine;
using System.Collections;

public class Dash : MonoBehaviour
{
    private Vector3 forward;
    private Vector3 position;
    private CharacterController m_CharacterController;
    private float dashTime;
    private Boolean dashing;
    public GameObject trail;
    // Use this for initialization
    void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
        dashTime = 0.2f;
        dashing = false;
        trail.GetComponent<TrailRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("Dash");
            GetComponent<ThirdPersonController>().enabled = false;
            DoDash();

        }
    }

    void DoDash()
    {
        dashing = true;
        trail.GetComponent<TrailRenderer>().enabled = true;
        forward = transform.forward;
        Invoke("finishDash", dashTime);

    }
    private void FixedUpdate()
    {
        if (dashing)
            m_CharacterController.Move(forward * 1500 * Time.fixedDeltaTime);
    }

    void finishDash()
    {
        dashing = false;
        GetComponent<ThirdPersonController>().enabled = true;
        trail.GetComponent<TrailRenderer>().enabled = false;
    }
}
