using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class KingOfTheHill : NetworkBehaviour
{
    public Material neutralMaterial;
    public Material blueMaterial;
    public Material redMaterial;

    public GameObject[] stones; //The rocks fragments around the point that change color indicating that the point is being captured
    public int captureTime; //Time needed to capture the point in seconds
    public int explosionRadius;
    public int explosionForce;

    [HideInInspector]
    public int blueRocksOwned; //Number of full rocks on the blue team
    [HideInInspector]
    public int redRocksOwned; //Number of full rocks on the red team

    private int numberRocks; //Number of rock fragments around the point
    private int blueTeam; //Number of people from the blue team inside the point
    private int redTeam; //Number of people from the red team inside the point
    private float capturePoints; //blue is positive
    private int activeRock; //Indicates the current status of the point. Has a value in [-numberRocks; numberRocks]


    // Use this for initialization
    void Start()
    {
        blueRocksOwned = 0;
        redRocksOwned = 0;
        numberRocks = stones.Length;
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

        var rockState = Mathf.FloorToInt((capturePoints / captureTime) * numberRocks);
        //Debug.Log("rockState: " + rockState);
        setRockColors(rockState);

        CapturePoints(time);
        //Debug.Log("CAP Points: " + capturePoints);
    }

    void CapturePoints(float timePassed)
    {
        if (blueTeam > redTeam)
        {
            capturePoints += timePassed * Mathf.Min(3, blueTeam - redTeam);
            if (capturePoints >= captureTime)
            {
                //DROP ROCK
                //EXPLODE
                //RESET EVERYTHING
            }
        }
        else if (blueTeam < redTeam)
        {
            capturePoints -= timePassed * Mathf.Min(3, redTeam - blueTeam);
            if (capturePoints <= -captureTime)
            {
                //DROP ROCK
                //EXPLODE
                //RESET EVERYTHING
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
                var stoneIndex = (activeRock * -1) - 1;
                RpcChangeRockToNeutral(stoneIndex);
                activeRock = rockState;
            }
            else if (activeRock == rockState + 1) //Indicates that the stone in index rockState-1 must be changed to red
            {
                var stoneIndex = (rockState * -1) - 1;
                RpcChangeRockToRed(stoneIndex);
                activeRock = rockState;
            }
        }
        else if (rockState > 0)
        {
            if (activeRock == rockState - 1) //Indicates that the stone in index rockState-1 must be changed to blue
            {
                var stoneIndex = rockState - 1;
                RpcChangeRockToBlue(stoneIndex);
                activeRock = rockState;
            }
            else if (activeRock == rockState + 1) //Indicates that the stone at index activeRock-1 must be changed to neutral
            {
                var stoneIndex = activeRock - 1;
                RpcChangeRockToNeutral(stoneIndex);
                activeRock = rockState;
            }
        }
        else if (activeRock != 0)
        {
            RpcChangeRockToNeutral(0);
            activeRock = 0;
        }
    }

    void PushPlayers()
    {
        // TODO
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Team1"))
        {
            Debug.Log("Enter " + other.name);
            blueTeam++;
        }
        else if (other.CompareTag("Team2"))
        {
            Debug.Log("Enter " + other.name);
            redTeam++;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Team1"))
        {
            Debug.Log("Exit " + other.name);
            blueTeam--;
        }
        else if (other.CompareTag("Team2"))
        {
            Debug.Log("Exit " + other.name);
            redTeam--;
        }
    }

    [ClientRpc]
    public void RpcChangeRockToBlue(int stoneIndex)
    {
        stones[stoneIndex].GetComponent<MeshRenderer>().material = blueMaterial;
    }

    [ClientRpc]
    public void RpcChangeRockToRed(int stoneIndex)
    {
        stones[stoneIndex].GetComponent<MeshRenderer>().material = redMaterial;
    }

    [ClientRpc]
    public void RpcChangeRockToNeutral(int stoneIndex)
    {
        stones[stoneIndex].GetComponent<MeshRenderer>().material = neutralMaterial;
    }
}
