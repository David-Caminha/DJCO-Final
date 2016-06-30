using Prototype.NetworkLobby;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerStats : NetworkBehaviour
{
    //ERRORS ir buscar damage

    public GameObject netManager;
    public SkinnedMeshRenderer mesh;
    public Material[] blueTeam;
    public Material[] redTeam;

    private GameObject victoryScreen;
    private GameObject defeatScreen;
    [HideInInspector]
    public GameObject healthBar;
    [HideInInspector]
    public GameObject manaBar;
    [HideInInspector]
    public Text healthText;
    [HideInInspector]
    public Text manaText;

    public RectTransform floatingHealthBar;
    public RectTransform floatingManaBar;

    [SyncVar(hook = "OnTeamChange")]
    public string team;

    // Basic stats for the character
    [SerializeField]
    [HideInInspector]
    private int _armor;
    [SerializeField]
    [HideInInspector]
    private int _health;
    [SerializeField]
    [HideInInspector]
    private int _maxHealth;
    [SerializeField]
    [HideInInspector]
    private int _mana;
    [SerializeField]
    [HideInInspector]
    private int _maxMana;
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
    [SerializeField]
    private bool _frozen;
    [SerializeField]
    private bool _leader;


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

    public int MaxHealth
    {
        get
        {
            return _maxHealth;
        }
        private set
        {
            _maxHealth = value;
        }
    }

    public int Mana
    {
        get
        {
            return _mana;
        }
        private set
        {
            _mana = value;
        }
    }

    public int MaxMana
    {
        get
        {
            return _maxMana;
        }
        private set
        {
            _maxMana = value;
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
        Health = 500;
        Mana = 150;
        MaxHealth = 500;
        MaxMana = 150;
        Damage = 50;
        AttackSpeed = 0.93f;
        MovementSpeed = 75;

        Frozen = false;
        Leader = false;

        tag = team;
        
        if (team.Equals("Team1"))
        {
            mesh.materials = blueTeam;
        }
        else if (team.Equals("Team2"))
        {
            mesh.materials = redTeam;
        }

        if (isLocalPlayer)
        {
            floatingHealthBar.parent.gameObject.SetActive(false);

            victoryScreen = GameObject.FindGameObjectWithTag("VictoryScreen");
            defeatScreen = GameObject.FindGameObjectWithTag("DefeatScreen");
            healthBar = GameObject.FindGameObjectWithTag("HealthBar");
            manaBar = GameObject.FindGameObjectWithTag("ManaBar");
            healthText = GameObject.FindGameObjectWithTag("HealthText").GetComponent<Text>();
            manaText = GameObject.FindGameObjectWithTag("ManaText").GetComponent<Text>();
            healthBar.transform.parent.gameObject.SetActive(false);
            victoryScreen.SetActive(false);
            defeatScreen.SetActive(false);
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
            Health = 500;
            Invoke("StopReviving", 2.1f);
        }
    }

    void StopReviving()
    {
        thirdPersonController.m_Revive = false;
    }

    void RemoveFrenzy()
    {
        MaxHealth = 500;
        if (Health > 500)
            Health = 500;
        Damage = 50;

        var healthPercent = (float)Health / MaxHealth;
        var manaPercent = (float)Mana / MaxMana;

        healthPercent = Mathf.Clamp(healthPercent, 0, 1);
        manaPercent = Mathf.Clamp(manaPercent, 0, 1);

        floatingHealthBar.localScale = new Vector3(healthPercent, 1, 1);
        floatingManaBar.localScale = new Vector3(manaPercent, 1, 1);
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

        var healthPercent = (float)Health / MaxHealth;
        var manaPercent = (float)Mana / MaxMana;

        healthPercent = Mathf.Clamp(healthPercent, 0, 1);
        manaPercent = Mathf.Clamp(manaPercent, 0, 1);

        floatingHealthBar.localScale = new Vector3(healthPercent, 1, 1);
        floatingManaBar.localScale = new Vector3(manaPercent, 1, 1);
    }

    public void ChangeMana(int amount)
    {
        Mana += amount;
    }

    public void ChangeMaxHealth(int newValue)
    {
        MaxHealth = newValue;
    }

    public void ChangeMaxMana(int newValue)
    {
        MaxMana = newValue;
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
    public void RpcResolveHit(int amount)
    {
        ChangeHealth(amount);
        if (isLocalPlayer && Health <= 0)
        {
            Invoke("Die", 2.58f);
            thirdPersonController.m_Die = true;
            thirdPersonController.PlayDeathSound();
        }
    }

    void Die()
    {
        CmdPlayerDeath(gameObject);
    }

    [Command]
    public void CmdPlayerDeath(GameObject deadPlayer)
    {
        deadPlayer.GetComponent<PlayerStats>().RpcPlayerDeath();
    }

    [ClientRpc]
    public void RpcPlayerDeath()
    {
        ToggleRenderer(false);

        if (isLocalPlayer) // Remove controls and Move player to the spawn point
        {
            Transform spawn = NetworkManager.singleton.GetStartPosition();
            transform.position = spawn.position;
            transform.rotation = spawn.rotation;
            ToggleControls(false);
        }
        Invoke("Respawn", 0.1f);
    }

    //Command and Rpc calls for spells
    [Command]
    public void CmdActivateFrenzy(float duration)
    {
        RpcActivateFrenzy(duration);
    }

    [ClientRpc]
    void RpcActivateFrenzy(float duration)
    {
        MaxHealth = 1500;
        Health = 1500;
        Damage = 100;

        var healthPercent = (float)Health / MaxHealth;
        var manaPercent = (float)Mana / MaxMana;

        healthPercent = Mathf.Clamp(healthPercent, 0, 1);
        manaPercent = Mathf.Clamp(manaPercent, 0, 1);

        floatingHealthBar.localScale = new Vector3(healthPercent, 1, 1);
        floatingManaBar.localScale = new Vector3(manaPercent, 1, 1);

        Invoke("RemoveFrenzy", 1.7f + duration);
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
            if (other.CompareTag("Sword") && !other.transform.root.CompareTag(tag))
            {
                if (!thirdPersonController.m_Dying)
                {
                    CmdResolveHit(-other.transform.root.GetComponent<PlayerStats>().Damage);
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
        if (isLocalPlayer)
        {
            print("WON");
            victoryScreen.SetActive(true);
            Invoke("Disconnect", 3f);
        }
    }

    public void LoseGame()
    {
        if (isLocalPlayer)
        {
            print("LOST");
            defeatScreen.SetActive(true);
            Invoke("Disconnect", 3f);
        }
    }

    void Disconnect()
    {
        thirdPersonController.m_MouseLook.SetCursorLock(false);
        if (isServer)
        {
            netManager.GetComponent<LobbyManager>().StopHostClbk();
        }
        else
        {
            netManager.GetComponent<LobbyManager>().StopClientClbk();
        }
    }

    //SyncVar hooks
    private void OnTeamChange(string newTeam)
    {
        tag = newTeam;
    }
}
