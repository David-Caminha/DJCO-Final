using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class KingOfTheHill : NetworkBehaviour
{
    public Material neutralMaterial;
    public Material blueMaterial;
    public Material redMaterial;

    public GameObject[] blueTotems;
    public GameObject[] redTotems;
    public GameObject finalTotem;

    public GameObject[] blueTeamStones; //The rocks fragments around the point that change color indicating that the point is being captured by team blue
    public GameObject[] redTeamStones; //The rocks fragments around the point that change color indicating that the point is being captured by team red
    public GameObject finalRock; //The rock fragment that will serve as tie breaker
    public int captureTime; //Time needed to capture the point in seconds
    public int explosionRadius;
    public int explosionForce;

    [HideInInspector]
    public int blueRocksOwned; //Number of full rocks on the blue team
    [HideInInspector]
    public int redRocksOwned; //Number of full rocks on the red team

    private int numberRocks; //Number of rock fragments around the point that a team has including the final rock which is shared
    private int blueTeam; //Number of people from the blue team inside the point
    private int redTeam; //Number of people from the red team inside the point
    private int leader; // -1 for leader with read team; 0 for no leader; 1 for leader with blue team
    private float capturePoints; //blue is positive
    private int activeRock; //Indicates the current status of the point. Has a value in [-numberRocks; numberRocks]


    // Use this for initialization
    void Start()
    {
        blueRocksOwned = 0;
        redRocksOwned = 0;
        numberRocks = blueTeamStones.Length + 1;
        blueTeam = 0;
        redTeam = 0;
        capturePoints = 0;
        activeRock = 0;
    }

    // Update is called once per frame
    [Server]
    void Update()
    {
        var time = Time.deltaTime;

        CapturePoints(time);
        //Debug.Log("CAP Points: " + capturePoints);

        var capturePointsRatio = (capturePoints / captureTime) * numberRocks;
        var rockState = 0;
        if (capturePointsRatio < 0)
        {
            rockState = Mathf.CeilToInt(capturePointsRatio);
            print("RED rockState: " + rockState);
        }
        else
        {
            rockState = Mathf.FloorToInt(capturePointsRatio);
            print("BLUE rockState: " + rockState);
        }
        //Debug.Log("rockState: " + rockState);
        setRockColors(rockState);
    }

    void CapturePoints(float timePassed)
    {
        if (blueTeam > redTeam)
        {
            if (capturePoints != captureTime)
            {
                capturePoints += timePassed * (Mathf.Min(3, blueTeam - redTeam) + leader);
                if (capturePoints >= captureTime)
                {
                    capturePoints = captureTime;
                    if (blueRocksOwned < numberRocks - 1)
                        DropRock(blueTotems[blueRocksOwned], blueTeamStones[blueRocksOwned].transform);
                    else
                    {
                        DropRock(finalTotem, finalRock.transform, true);

                    }
                    blueRocksOwned++;
                    if (blueRocksOwned == numberRocks)
                    {
                        //TEAM1 WIN GAME
                        //GameObject[] team1Players = GameObject.FindGameObjectsWithTag("Team1");
                        //GameObject[] team2Players = GameObject.FindGameObjectsWithTag("Team2");
                        //foreach (GameObject player in team1Players)
                        //    player.GetComponent<PlayerStats>().WinGame();
                        //foreach (GameObject player in team2Players)
                        //    player.GetComponent<PlayerStats>().LoseGame();
                    }
                    else
                    {
                        //EXPLODE
                        Invoke("ResetKOTH", 1.5f);
                    }
                }
            }
        }
        else if (blueTeam < redTeam)
        {
            if (capturePoints != -captureTime)
            {
                capturePoints -= timePassed * (Mathf.Min(3, redTeam - blueTeam) - leader);
                if (capturePoints <= -captureTime)
                {
                    capturePoints = -captureTime;
                    if (redRocksOwned < numberRocks - 1)
                        DropRock(redTotems[redRocksOwned], redTeamStones[redRocksOwned].transform);
                    else
                        DropRock(finalTotem, finalRock.transform, false);
                    redRocksOwned++;
                    if (redRocksOwned == numberRocks)
                    {
                        //TEAM2 WIN GAME
                    }
                    else
                    {
                        //EXPLODE
                        Invoke("ResetKOTH", 1.5f);
                    }
                }
            }
        }
        else if (blueTeam == 0 && redTeam == 0)
        {
            if (capturePoints > 0)
            {
                capturePoints -= timePassed;
                if (capturePoints <= 0)
                    capturePoints = 0;
            }
            else if (capturePoints < 0)
            {
                capturePoints += timePassed;
                if (capturePoints >= 0)
                    capturePoints = 0;
            }
        }
    }

    void setRockColors(int rockState)
    {
        if (rockState < 0)
        {
            if (activeRock == rockState - 1) //Indicates that the stone at index activeRock-1 must be changed to neutral
            {
                var stoneIndex = activeRock;
                RpcChangeRockToNeutral(stoneIndex);
                activeRock = rockState;
            }
            else if (activeRock == rockState + 1) //Indicates that the stone in index rockState-1 must be changed to red
            {
                var stoneIndex = rockState * -1;
                RpcChangeRockToRed(stoneIndex);
                activeRock = rockState;
            }
        }
        else if (rockState > 0)
        {
            if (activeRock == rockState - 1) //Indicates that the stone in index rockState-1 must be changed to blue
            {
                var stoneIndex = rockState;
                RpcChangeRockToBlue(stoneIndex);
                activeRock = rockState;
            }
            else if (activeRock == rockState + 1) //Indicates that the stone at index activeRock-1 must be changed to neutral
            {
                var stoneIndex = activeRock;
                RpcChangeRockToNeutral(stoneIndex);
                activeRock = rockState;
            }
        }
        else if (activeRock != 0)
        {
            var stoneIndex = activeRock;
            RpcChangeRockToNeutral(stoneIndex);
            activeRock = 0;
        }
    }

    void DropRock(GameObject totemPrefab, Transform baseRock)
    {
        GameObject totem = (GameObject)Instantiate(totemPrefab, baseRock.position, baseRock.rotation);
        totem.transform.position += new Vector3(0, 100);
        totem.GetComponent<Totem>().rot = totem.transform.rotation;
        NetworkServer.Spawn(totem);
    }

    void DropRock(GameObject totemPrefab, Transform baseRock, bool blueTeam)
    {
        GameObject totem = (GameObject)Instantiate(totemPrefab, baseRock.position, baseRock.rotation);
        totem.transform.position += new Vector3(0, 100);
        var script = totem.GetComponent<Totem>();
        script.rot = totem.transform.rotation;
        script.finalTotem = true;
        if (blueTeam)
        {
            script.blueTotem = true;
        }
        else
        {
            script.blueTotem = false;
        }
        NetworkServer.Spawn(totem);
    }

    void PushPlayers()
    {
        // TODO
    }

    void ResetKOTH()
    {
        capturePoints = 0;
        if (activeRock > 0)
            for (int i = 1; i <= numberRocks; i++)
                RpcChangeRockToNeutral(i);
        else if (activeRock < 0)
            for (int i = -numberRocks; i <= -1; i++)
                RpcChangeRockToNeutral(i);
        activeRock = 0;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Team1"))
        {
            Debug.Log("Enter " + other.name);
            blueTeam++;
            if (other.GetComponent<PlayerStats>().Leader)
                leader++;
        }
        else if (other.CompareTag("Team2"))
        {
            Debug.Log("Enter " + other.name);
            redTeam++;
            if (other.GetComponent<PlayerStats>().Leader)
                leader--;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<PlayerStats>().Leader)
        {
            if (other.CompareTag("Team1"))
            {
                leader = 1;
            }
            else if (other.CompareTag("Team2"))
            {
                leader = -1;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Team1"))
        {
            Debug.Log("Exit " + other.name);
            blueTeam--;
            if (other.GetComponent<PlayerStats>().Leader)
                leader--;
        }
        else if (other.CompareTag("Team2"))
        {
            Debug.Log("Exit " + other.name);
            redTeam--;
            if (other.GetComponent<PlayerStats>().Leader)
                leader++;
        }
    }

    [ClientRpc]
    public void RpcChangeRockToBlue(int stoneIndex)
    {
        if (stoneIndex == numberRocks)
        {
            finalRock.GetComponent<MeshRenderer>().material = blueMaterial;
        }
        else
        {
            blueTeamStones[stoneIndex - 1].GetComponent<MeshRenderer>().material = blueMaterial;
        }
    }

    [ClientRpc]
    public void RpcChangeRockToRed(int stoneIndex)
    {
        if (stoneIndex == numberRocks)
        {
            finalRock.GetComponent<MeshRenderer>().material = redMaterial;
        }
        else
        {
            redTeamStones[stoneIndex - 1].GetComponent<MeshRenderer>().material = redMaterial;
        }
    }

    [ClientRpc]
    public void RpcChangeRockToNeutral(int stoneIndex)
    {
        if (stoneIndex == numberRocks || -stoneIndex == numberRocks)
        {
            finalRock.GetComponent<MeshRenderer>().material = neutralMaterial;
        }
        else if (stoneIndex < 0)
        {
            var index = (stoneIndex * -1) - 1;
            redTeamStones[index].GetComponent<MeshRenderer>().material = neutralMaterial;
        }
        else if (stoneIndex > 0)
        {
            blueTeamStones[stoneIndex - 1].GetComponent<MeshRenderer>().material = neutralMaterial;
        }
        else
        {
            print("THE ELSE");
            blueTeamStones[0].GetComponent<MeshRenderer>().material = neutralMaterial;
            redTeamStones[0].GetComponent<MeshRenderer>().material = neutralMaterial;
        }
    }
}
