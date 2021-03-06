﻿using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.CrossPlatformInput;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    public Skill[] skills;

    [SerializeField]
    private Transform cameraRelative2Player;
    [SerializeField]
    private Transform cameraOverview;
    [SerializeField]
    [HideInInspector]
    private PlayerStats m_PlayerStats;
    [SerializeField]
    public ThirdPersonMouseLook m_MouseLook;
    [SerializeField]
    private float m_ForwardSpeed;   // Speed modifier when walking forwards
    [SerializeField]
    private float m_BackwardSpeed;  // Speed modifier when walking backwards
    [SerializeField]
    private float m_StrafeSpeed;    // Speed modifier when walking sideways
    [SerializeField]
    private float m_JumpSpeed;
    [SerializeField]
    private float m_StickToGroundForce;
    [SerializeField]
    private float m_GravityMultiplier;
    [SerializeField]
    private float m_StepInterval;
    [SerializeField]
    private AudioClip[] m_LeftFootstepSounds; // an array of footstep sounds that will be randomly selected from.
    [SerializeField]
    private AudioClip[] m_RightFootstepSounds; // an array of footstep sounds that will be randomly selected from.
    [SerializeField]
    private AudioClip m_JumpSound; // the sound played when character leaves the ground.
    [SerializeField]
    private AudioClip m_LandSound; // the sound played when character touches back on ground.
    [SerializeField]
    private AudioClip m_Swing0Sound; // the sound played when character attacks at state 0.
    [SerializeField]
    private AudioClip m_Swing1Sound; // the sound played when character attacks at state 1.
    [SerializeField]
    private AudioClip m_Swing2Sound; // the sound played when character attacks at state 2.
    [SerializeField]
    private AudioClip m_Swing3Sound; // the sound played when character attacks at state 3.
    [SerializeField]
    private AudioClip m_ReviveSound; // the sound played when character uses freeze.
    [SerializeField]
    private AudioClip m_DeathSound; // the sound played when character uses freeze.
    [SerializeField]
    private AudioClip m_DeathByFallSound; // the sound played when character uses freeze.
    [SerializeField]
    private Transform groundCheck;
    [SerializeField]
    private LayerMask whatIsGround;
    float groundRadius = 1f;
    bool grounded = false;

    private int stepIndex;
    private bool leftStep;
    private bool dyingFromFall;
    private Animator m_Animator;
    private Camera m_Camera;
    private bool m_Jump;
    private float m_YRotation;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private float m_StepCycle;
    private float m_NextStep;
    private bool m_Jumping;

    private bool m_Attacking;
    private bool m_Attack;
    private int m_attackState;
    private float m_attackTimeOut;

    public bool m_Dying;
    public bool m_Die;
    public bool m_Revive;

    public bool m_Freeze;
    public bool m_Frenzy;
    public bool m_Teleport;
    public bool m_Teleporting;
    public bool m_UnTeleport;
    public bool m_UnTeleporting;
    private GameObject teleportTo;

    public GameObject teleportA;
    public GameObject teleportB;
    Vector3 teleportStartPosition;
    Vector3 teleportEndPosition;

    private AudioSource m_AudioSource;
    AnimatorStateInfo animState;

    public float time;
    // Use this for initialization
    private void Start()
    {
        GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>().localPlayer = this;
        teleportTo = GameObject.FindGameObjectWithTag("TeleportDestination");
        leftStep = false;
        cameraOverview = GameObject.FindGameObjectWithTag("OverviewCamera").transform;
        stepIndex = 0;
        dyingFromFall = false;
        m_Animator = GetComponent<Animator>();
        m_CharacterController = GetComponent<CharacterController>();
        m_Camera = Camera.main;
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle / 2f;
        m_Jumping = false;
        m_AudioSource = GetComponent<AudioSource>();
        m_MouseLook.Init(transform, m_Camera.transform);
        m_PlayerStats = GetComponent<PlayerStats>();
        m_Attacking = false;
        m_attackTimeOut = 2.5f;
        m_Dying = false;
        m_Die = false;
        m_Revive = true;
        Invoke("PlayReviveSound", 0.7f);
        m_PlayerStats.Invoke("StopReviving", 2.4f);
    }


    // Update is called once per frame
    private void Update()
    {
        UpdateUI();

        if (m_PlayerStats.Frozen)
            return;

        //States verification
        if (dyingFromFall)
        {
            m_Camera.transform.LookAt(transform);
            return;
        }
        if (m_Dying)
        {
            m_Animator.SetBool("Die", m_Die);
            return;
        }
        if (m_Revive)
        {
            return;
        }
        if (m_Die)
        {
            m_Dying = true;
            m_Animator.SetBool("Dying", m_Dying);
            m_Animator.SetBool("Die", m_Die);
            m_Die = false;
        }

        if (m_Teleporting)
        {
            m_CharacterController.enabled = false;
            transform.position = Vector3.Lerp(teleportStartPosition, teleportEndPosition, time * 2);
            time += Time.deltaTime;
            if (time >= 0.5)
            {
                time = 0;
                m_Teleporting = false;
                m_UnTeleporting = true;
                teleportStartPosition = teleportTo.transform.position;
                transform.position = teleportTo.transform.position;
                var portalB = (GameObject) Instantiate(teleportB, transform.position + new Vector3(0, 26f, 0), Quaternion.identity);
                Destroy(portalB, 1);
                teleportEndPosition = transform.position + new Vector3(0, 26, 0);
            }
            return;
        }
        if (m_UnTeleporting)
        {
            transform.position = Vector3.Lerp(teleportStartPosition, teleportEndPosition, time * 2);
            time += Time.deltaTime;
            if (time >= 0.5)
            {
                m_UnTeleporting = false;
                m_CharacterController.enabled = true;
            }
            return;
        }

        //StartUpdate functions
        animState = m_Animator.GetCurrentAnimatorStateInfo(0);

        if (m_attackTimeOut >= 0)
            m_attackTimeOut -= Time.deltaTime;
        else
            m_attackState = 3;
        RotateView();

        // State readings for animations
        if (grounded)
        {
            if (!m_Attacking)
                m_Attack = CrossPlatformInputManager.GetButton("Fire1");
            m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            m_Frenzy = Input.GetKeyDown(KeyCode.Alpha1);
            if (m_Frenzy)
            {
                if (skills[0].enabled)
                    skills[0].Activate();
                else
                    m_Frenzy = false;
            }
            m_Freeze = Input.GetKeyDown(KeyCode.Alpha2);
            if (m_Freeze && skills[1].enabled)
            {
                if (skills[1].enabled)
                    skills[1].Activate();
                else
                    m_Freeze = false;
            }
            m_Teleport = Input.GetKeyDown(KeyCode.Alpha3);
            if (m_Teleport && skills[2].enabled)
            {
                skills[2].Activate();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4) && skills[3].enabled)
            {
                skills[3].Activate();
            }
        }
        if (m_Attacking)
        {
            if (animState.normalizedTime >= 1)
                m_Attacking = false;
        }

        if (!m_PreviouslyGrounded && grounded)
        {
            PlayLandingSound();
            m_MoveDir.y = 0f;
            m_Jumping = false;
        }
        if (!grounded && !m_Jumping && m_PreviouslyGrounded)
        {
            m_MoveDir.y = 0f;
        }
        m_PreviouslyGrounded = grounded;
    }

    private void UpdateUI()
    {
        var healthPercent = (float)m_PlayerStats.Health / m_PlayerStats.MaxHealth;
        var manaPercent = (float)m_PlayerStats.Mana / m_PlayerStats.MaxMana;

        healthPercent = Mathf.Clamp(healthPercent, 0, 1);
        manaPercent = Mathf.Clamp(manaPercent, 0, 1);

        m_PlayerStats.healthBar.transform.localScale = new Vector3(healthPercent, 1, 1);
        m_PlayerStats.manaBar.transform.localScale = new Vector3(manaPercent, 1, 1);

        m_PlayerStats.healthText.text = m_PlayerStats.Health + " / " + m_PlayerStats.MaxHealth;
        m_PlayerStats.manaText.text = m_PlayerStats.Mana + " / " + m_PlayerStats.MaxMana;
    }

    private void PlayLandingSound()
    {
        m_AudioSource.clip = m_LandSound;
        m_AudioSource.Play();
        m_NextStep = m_StepCycle + .5f;
    }

    private void PlaySwingSound(int state)
    {
        switch (state)
        {
            case 0:
                m_AudioSource.clip = m_Swing0Sound;
                break;
            case 1:
                m_AudioSource.clip = m_Swing1Sound;
                break;
            case 2:
                m_AudioSource.clip = m_Swing2Sound;
                break;
        }
        m_AudioSource.Play();
    }

    private void PlayThirdSwingSound()
    {
        m_AudioSource.clip = m_Swing3Sound;
        m_AudioSource.Play();
    }

    private void PlayReviveSound()
    {
        m_AudioSource.clip = m_ReviveSound;
        m_AudioSource.Play();
    }

    public void PlayDeathSound()
    {
        m_AudioSource.clip = m_DeathSound;
        m_AudioSource.Play();
    }

    private void PlayDeathByFallSound()
    {
        m_AudioSource.clip = m_DeathByFallSound;
        m_AudioSource.Play();
    }

    private void FixedUpdate()
    {
        //State verifications
        if (dyingFromFall)
        {
            m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);
            return;
        }
        if (m_Dying || m_Revive || m_PlayerStats.Frozen)
            return;

        //FixedUpdate functions
        grounded = Physics.CheckSphere(groundCheck.position, groundRadius, whatIsGround, QueryTriggerInteraction.Ignore);

        float speed;
        GetInput(out speed);
        // always move along the camera forward as it is the direction that it being aimed at
        Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

        // get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                            m_CharacterController.height / 2f, ~0, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        m_MoveDir.x = desiredMove.x * speed;
        m_MoveDir.z = desiredMove.z * speed;


        if (grounded)
        {
            m_MoveDir.y = -m_StickToGroundForce;

            if (m_Jump)
            {
                m_MoveDir.y = m_JumpSpeed;
                PlayJumpSound();
                m_Jump = false;
                m_Jumping = true;
            }
            if (m_Attack)
            {
                Debug.Log("Attack");
                m_Attack = false;
                Attack();
            }
        }
        else
        {
            m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
        }
        m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

        ProgressStepCycle(speed);

        m_MouseLook.UpdateCursorLock();

        UpdateAnimator();
    }

    private void Attack()
    {
        m_Attacking = true;
        switch (m_attackState)
        {
            case 0:
                m_attackTimeOut = 1.5f;
                PlaySwingSound(1);
                m_attackState = 1;
                break;
            case 1:
                m_attackTimeOut = 1.5f;
                PlaySwingSound(2);
                m_attackState = 2;
                break;
            case 2:
                m_attackTimeOut = 2.5f;
                int specialAttack = Random.Range(0, 2);
                if (specialAttack == 0)
                {
                    m_attackState = 0;
                    PlaySwingSound(0);
                }
                else
                {
                    m_attackState = 3;
                    Invoke("PlayThirdSwingSound", 0.38f);
                }
                break;
            case 3:
                m_attackTimeOut = 1.5f;
                PlaySwingSound(0);
                m_attackState = 0;
                break;
        }

    }

    private void PlayJumpSound()
    {
        m_AudioSource.clip = m_JumpSound;
        m_AudioSource.Play();
    }

    private void ProgressStepCycle(float speed)
    {
        if (speed > 0 && (m_Input.x != 0 || m_Input.y != 0))
        {
            m_StepCycle += (speed) * Time.fixedDeltaTime;
        }

        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        m_NextStep = m_StepCycle + m_StepInterval;

        PlayFootStepAudio();
    }

    private void PlayFootStepAudio()
    {
        if (!grounded)
        {
            return;
        }
        if (leftStep)
        {
            m_AudioSource.clip = m_LeftFootstepSounds[stepIndex];
            leftStep = false;
        }
        else
        {
            m_AudioSource.clip = m_RightFootstepSounds[stepIndex];
            leftStep = true;
            stepIndex++;
            if (stepIndex == m_RightFootstepSounds.Length)
                stepIndex = 0;
        }
        m_AudioSource.PlayOneShot(m_AudioSource.clip);
    }

    private void GetInput(out float speed)
    {
        // Read input
        float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
        float vertical = CrossPlatformInputManager.GetAxis("Vertical");

        m_Input = new Vector2(horizontal, vertical);

        // set the desired speed
        speed = 0; //default speed is 0
        if (m_Input.x > 0 || m_Input.x < 0)
        {
            //strafe
            speed = m_StrafeSpeed * m_PlayerStats.MovementSpeed;
        }
        if (m_Input.y < 0)
        {
            //backwards
            speed = m_BackwardSpeed * m_PlayerStats.MovementSpeed;
        }
        else if (m_Input.y > 0)
        {
            //forwards
            //handled last as if strafing and moving forward at the same time forwards speed should take precedence
            speed = m_ForwardSpeed * m_PlayerStats.MovementSpeed;
        }

        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }
    }

    private void RotateView()
    {
        m_MouseLook.LookRotation(transform, m_Camera.transform);
    }

    void UpdateAnimator()
    {
        // update the animator parameters
        m_Animator.SetFloat("Forward", m_Input.y, 0.1f, Time.deltaTime);
        m_Animator.SetFloat("Strafe", m_Input.x, 0.1f, Time.deltaTime);
        m_Animator.SetBool("Crouch", false);
        m_Animator.SetBool("OnGround", grounded);
        m_Animator.SetBool("Attacking", m_Attacking);
        m_Animator.SetInteger("AttackState", m_attackState);
        m_Animator.SetBool("Dying", m_Dying);
        m_Animator.SetBool("Frenzy", m_Frenzy);
        m_Animator.SetBool("Freeze", m_Freeze);
        if (!grounded)
        {
            m_Animator.SetFloat("Jump", m_MoveDir.y);
        }

        // calculate which leg is behind, so as to leave that leg trailing in the jump animation
        // (This code is reliant on the specific run cycle offset in our animations,
        // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
        float runCycle =
            Mathf.Repeat(
                m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + 0.2f, 1);
        float jumpLeg = (runCycle < 0.5f ? 1 : -1) * m_Input.y;
        if (grounded)
        {
            m_Animator.SetFloat("JumpLeg", jumpLeg);
        }
    }

    public void DieFromFall()
    {
        dyingFromFall = true;
        m_Camera.transform.parent = null;
        PlayDeathByFallSound();
        Invoke("FinishFall", 2f);
    }

    public void FinishFall()
    {
        m_Camera.transform.position = cameraOverview.position;
        m_Camera.transform.rotation = cameraOverview.rotation;
        dyingFromFall = false;
        m_PlayerStats.CmdPlayerDeath(gameObject);
    }

    public void ResetCamera()
    {
        m_Camera.transform.parent = transform;
        m_Camera.transform.position = cameraRelative2Player.position;
        m_Camera.transform.rotation = cameraRelative2Player.rotation;
        m_MouseLook.Init(transform, m_Camera.transform);
    }
    
    // Used to add force upon collision between character controller and a rigidbody
    //private void OnControllerColliderHit(ControllerColliderHit hit)
    //{
    //    Rigidbody body = hit.collider.attachedRigidbody;
    //    //dont move the rigidbody if the character is on top of it
    //    if (m_CollisionFlags == CollisionFlags.Below)
    //    {
    //        return;
    //    }

    //    if (body == null || body.isKinematic)
    //    {
    //        return;
    //    }
    //    body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
    //}
}
