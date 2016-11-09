using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Dash : Skill
{
    public AudioSource skillSoundSource;
    public AudioClip dashSound;

    public TrailRenderer trail;
    public Material blueMaterial;
    public Material redMaterial;

    private Vector3 forward;
    private Vector3 position;
    private CharacterController m_CharacterController;
    private float dashTime;
    private bool dashing;

    public override void Activate()
    {
        GetComponent<ThirdPersonController>().enabled = false;
        dashing = true;
        trail.enabled = true;
        forward = transform.forward;
        CmdPlayDashSound();
        CmdDash();
    }

    // Use this for initialization
    void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
        dashTime = 0.2f;
        dashing = false;
        if (CompareTag("Team1"))
            trail.material = blueMaterial;
        else if (CompareTag("Team2"))
            trail.material = redMaterial;
        trail.enabled = false;
    }

    private void FixedUpdate()
    {
        if (dashing)
            m_CharacterController.Move(forward * 1500 * Time.fixedDeltaTime);
    }

    void finishDash()
    {
        if (isLocalPlayer)
        {
            dashing = false;
            GetComponent<ThirdPersonController>().enabled = true;
        }
        trail.enabled = false;
    }

    [Command]
    void CmdPlayDashSound()
    {
        RpcPlayDashSound();
    }

    [ClientRpc]
    private void RpcPlayDashSound()
    {
        skillSoundSource.clip = dashSound;
        skillSoundSource.Play();
    }

    [Command]
    void CmdDash()
    {
        RpcDash();
    }

    [ClientRpc]
    void RpcDash()
    {
        if(isLocalPlayer)
        {
            forward = transform.forward;
            GetComponent<ThirdPersonController>().enabled = false;
            dashing = true;
        }
        trail.enabled = true;
        Invoke("finishDash", dashTime);
    }
}
