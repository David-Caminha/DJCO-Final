using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerStats : NetworkBehaviour
{
    //ERRORS ir buscar damage

    // Basic stats for the character
    [SerializeField] [HideInInspector] private int _armor;
    [SerializeField] [HideInInspector] private int _health;
    [SerializeField] [HideInInspector] private int _energy;
    [SerializeField] [HideInInspector] private int _damage;
    [SerializeField] [HideInInspector] private int _movementSpeed;
    [SerializeField] [HideInInspector] private float _attackSpeed;

    // Status effects
    [SerializeField] [HideInInspector] private bool _silenced;


    public ThirdPersonController thirdPersonController;
    public Camera fpsCamera;
    public AudioListener audioListener;
    Renderer[] renderers;

    // Getters and Setters for each of the stats (values can only be set inside this class)
    public int Armor
    {
        get
        {
            return _armor;
        }
        private set
        {
            _armor = value;
        }
    }
    
    public int Health
    {
        get
        {
            return _health;
        }
        private set
        {
            _health = value;
        }
    }

    public int Energy
    {
        get
        {
            return _energy;
        }
        private set
        {
            _energy = value;
        }
    }

    public int Damage
    {
        get
        {
            return _damage;
        }
        private set
        {
            _damage = value;
        }
    }

    public int MovementSpeed
    {
        get
        {
            return _movementSpeed;
        }
        private set
        {
            _movementSpeed = value;
        }
    }

    public float AttackSpeed
    {
        get
        {
            return _attackSpeed;
        }
        private set
        {
            _attackSpeed = value;
        }
    }

    // Getters and Setters for each of the status effect
    public bool Silenced
    {
        get
        {
            return _silenced;
        }
        private set
        {
            _silenced = value;
        }
    }

    // Use this for initialization
    public void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();

        Armor = 10;
        Health = 500;
        Energy = 150;
        Damage = 43;
        AttackSpeed = 0.93f;
        MovementSpeed = 100;

        Silenced = false;
    }

    public override void OnStartLocalPlayer()
    {
        thirdPersonController.enabled = true;
        fpsCamera.gameObject.SetActive(true);
        audioListener.enabled = true;

        gameObject.name = "LOCAL Player";
        base.OnStartLocalPlayer();
    }

    void ToggleRenderer(bool isAlive)
    {
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = isAlive;
    }

    void ToggleControls(bool isAlive)
    {
        thirdPersonController.enabled = isAlive;
        fpsCamera.cullingMask = ~fpsCamera.cullingMask;
    }

    // Methods to be invoked by other functions
    void Respawn()
    {
        ToggleRenderer(true);

        if (isLocalPlayer)
            ToggleControls(true);
    }

    void RemoveSilence()
    {
        Silenced = false;
    }

    // Methods for changing the stats
    public void ChangeArmor(int amount)
    {
        Armor += amount;
    }

    public void ChangeHealth(int amount)
    {
        Health += amount;
    }

    public void ChangeEnergy(int amount)
    {
        Energy += amount;
    }

    public void ChangeDamage(int amount)
    {
        Damage += amount;
    }

    public void ChangeAttackSpeed(int amount)
    {
        AttackSpeed += amount;
    }

    public void ChangeMovementSpeed(int amount)
    {
        MovementSpeed += amount;
    }


    // Server side methods (server calls these functons on every client)
    [ClientRpc]
    public void RpcResolveHit() // Remove hp check death etc...
    {
        ChangeHealth(10); //ERROR Ir buscar damage e aplicar

        if(_health <= 0)
        {
            ToggleRenderer(false);

            if (isLocalPlayer) // Remove controls and Move player to the spawn point
            {
                Transform spawn = NetworkManager.singleton.GetStartPosition();
                transform.position = spawn.position;
                transform.rotation = spawn.rotation;

                ToggleControls(false);
            }

            Invoke("Respawn", 2f);
        }
    }

    [ClientRpc]
    public void RpcSilencePlayer(float silenceTime)
    {
        Silenced = true;
        Invoke("RemoveSilence", silenceTime);
    }

}
