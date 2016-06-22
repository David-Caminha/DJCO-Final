using System;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerStats : NetworkBehaviour
{
    //ERRORS ir buscar damage

    public GameObject victoryScreen;
    public GameObject defeatScreen;

    // Basic stats for the character
    [SerializeField]
    [HideInInspector]
    private int _armor;
    [SerializeField]
    [HideInInspector]
    private int _health;
    [SerializeField]
    [HideInInspector]
    private int _energy;
    [SerializeField]
    [HideInInspector]
    private int _damage;
    [SerializeField]
    [HideInInspector]
    private int _movementSpeed;
    [SerializeField]
    [HideInInspector]
    private float _attackSpeed;

    // Status effects
    [SerializeField] private bool _frozen;
    [SerializeField] private bool _leader;


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
    public bool Frozen
    {
        get
        {
            return _frozen;
        }
        private set
        {
            _frozen = value;
        }
    }

    public bool Leader
    {
        get
        {
            return _leader;
        }
        set
        {
            _leader = value;
        }
    }

    // Use this for initialization
    public void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();

        Armor = 10;
        Health = 100;
        Energy = 150;
        Damage = 50;
        AttackSpeed = 0.93f;
        MovementSpeed = 100;

        Frozen = false;

        if(isLocalPlayer)
        {
            Invoke("StopReviving", 2.4f);
        }
    }

    void ToggleRenderer(bool isAlive)
    {
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = isAlive;
    }

    void ToggleControls(bool isAlive)
    {
        thirdPersonController.enabled = isAlive;
        
    }

    // Methods to be invoked by other functions
    void Respawn()
    {
        ToggleRenderer(true);

        if (isLocalPlayer)
        {
            thirdPersonController.ResetCamera();
            thirdPersonController.m_Dying = false;
            thirdPersonController.m_Revive = true;
            thirdPersonController.Invoke("PlayReviveSound", 0.7f);
            ToggleControls(true);
            Invoke("StopReviving", 2.1f);
        }
    }
    void StopReviving()
    {
        thirdPersonController.m_Revive = false;
    }

    void RemoveFreeze()
    {
        print("Unfreezing");
        Frozen = false;
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
    [Command]
    public void CmdResolveHit(int amount)
    {
        RpcResolveHit(amount);

    }

    [ClientRpc]
    public void RpcResolveHit(int amount) // Remove hp check death etc...
    {
        ChangeHealth(amount); //ERROR Ir buscar damage e aplicar

    }

    [Command]
    public void CmdPlayerDeath()
    {
        RpcPlayerDeath();
    }

    [ClientRpc]
    public void RpcPlayerDeath()
    {
        ToggleRenderer(false);

        if (isLocalPlayer) // Remove controls and Move player to the spawn point
        {
            //Transform spawn = NetworkManager.singleton.GetStartPosition();
            transform.position = new Vector3();
            transform.rotation = Quaternion.identity;
            ToggleControls(false);
        }

        Invoke("Respawn", 0.1f);
    }

    [ClientRpc]
    public void RpcFreezePlayer(float freezeTime)
    {
        if (isLocalPlayer)
            print("player from " + tag + "was frozen and i'm the local player");
        else
            print("player from " + tag + "was frozen");
        Frozen = true;
        Invoke("RemoveFreeze", freezeTime);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (isLocalPlayer)
        {
            if (other.tag.Equals("Sword"))
            {
                if (!thirdPersonController.m_Dying)
                {
                    CmdResolveHit(-other.transform.root.GetComponent<PlayerStats>().Damage);

                    Debug.Log("My health" + Health);
                    if (Health <= 0)
                    {
                        Invoke("CmdPlayerDeath", 2.58f);
                        thirdPersonController.m_Die = true;
                        thirdPersonController.PlayDeathSound(); //EM PRINCIPIO ESTA BEM SENAO FAZ UM INVOKE :)
                    }
                }
            }
            else
                return;
        }
        else
            return;
    }

    public void WinGame()
    {
        victoryScreen.SetActive(true);
    }

    public void LoseGame()
    {
        defeatScreen.SetActive(true);
    }
}
