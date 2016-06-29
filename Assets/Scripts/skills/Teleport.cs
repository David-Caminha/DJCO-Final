using UnityEngine;
using UnityEngine.Networking;

public class Teleport : Skill
{
    private GameObject[] teamMates;
    private int leastHealth = 100000;
    private int teamMateHealth = 0;
    private GameObject playerObjective = null;
    private Vector3 positionPlayerObjective;


    public AudioSource skillSoundSource;
    public AudioClip enteringPortalSound;
    public AudioClip exitingPortalSound;

    public CharacterController characterController;
    public GameObject portalAPrefab;
    public GameObject portalBPrefab;

    private bool enteringPortal;
    private bool exitingPortal;
    private GameObject teleportDestination;

    private GameObject portalA;
    private GameObject portalB;
    private Vector3 teleportStartPosition;
    private Vector3 teleportEndPosition;

    float time;

    public override void Activate()
    {
        time = 0;
        portalA = (GameObject)Instantiate(portalAPrefab, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
        CmdSpawnObject(portalA);
        Invoke("CmdUnSpawnPortalA", 1f);
        teleportStartPosition = transform.position;
        teleportEndPosition = transform.position + new Vector3(0, -26, 0);
        enteringPortal = true;
        CmdPlayEnteringSound();
    }

    // Use this for initialization
    void Start()
    {
        teleportDestination = GameObject.FindGameObjectWithTag("TeleportDestination");
    }

    // Update is called once per frame
    void Update()
    {
        if (enteringPortal)
        {
            characterController.enabled = false;
            transform.position = Vector3.Lerp(teleportStartPosition, teleportEndPosition, time * 2);
            time += Time.deltaTime;
            if (time >= 0.5)
            {
                time = 0;
                enteringPortal = false;
                exitingPortal = true;
                teleportStartPosition = teleportDestination.transform.position;
                transform.position = teleportDestination.transform.position;
                portalB = (GameObject)Instantiate(portalBPrefab, transform.position + new Vector3(0, 26f, 0), Quaternion.identity);
                CmdSpawnObject(portalB);
                Invoke("CmdUnSpawnPortalB", 1f);
                teleportEndPosition = transform.position + new Vector3(0, 26, 0);
                CmdPlayExitingSound();
            }
            return;
        }
        if (exitingPortal)
        {
            transform.position = Vector3.Lerp(teleportStartPosition, teleportEndPosition, time * 2);
            time += Time.deltaTime;
            if (time >= 0.5)
            {
                exitingPortal = false;
                characterController.enabled = true;
            }
            return;
        }
    }

    void TeleportToTeamMate()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            //Search which teamMate has the least health and save it on playerObjective
            teamMates = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject teamMate in teamMates)
            {
                teamMateHealth = teamMate.GetComponent<PlayerStats>().Health;
                print("If" + teamMateHealth);
                if (teamMateHealth != 0 && teamMateHealth < leastHealth)
                {
                    leastHealth = teamMateHealth;
                    playerObjective = teamMate;
                }
            }
            positionPlayerObjective = playerObjective.transform.position;

            //ERROR de entrar nas paredes
            transform.position = positionPlayerObjective + new Vector3(0, 0, -3);
        }
    }

    [Command]
    void CmdSpawnObject(GameObject obj)
    {
        NetworkServer.Spawn(obj);
    }

    [Command]
    void CmdUnSpawnPortalA()
    {
        NetworkServer.Destroy(portalA);
    }

    [Command]
    void CmdUnSpawnPortalB()
    {
        NetworkServer.Destroy(portalB);
    }

    [Command]
    void CmdPlayEnteringSound()
    {
        RpcPlayEnteringSound();
    }

    [Command]
    void CmdPlayExitingSound()
    {
        RpcPlayExitingSound();
    }

    [ClientRpc]
    private void RpcPlayEnteringSound()
    {
        skillSoundSource.clip = enteringPortalSound;
        skillSoundSource.Play();
    }

    [ClientRpc]
    private void RpcPlayExitingSound()
    {
        skillSoundSource.clip = exitingPortalSound;
        skillSoundSource.Play();
    }
}
