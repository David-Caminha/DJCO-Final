using UnityEngine;
using System.Collections;

public class KingOfTheHill : MonoBehaviour
{
    public GameObject stone1;
    public GameObject stone2;
    public GameObject stone3;
    public GameObject stone4;
    public int maxPoints;
    public int numberRocks;

    private int blueTeam;
    private int redTeam;
    private float capturePoints;//blue is positive
    private int activeRock; // [-9,9]
    private float time;


    // Use this for initialization
    void Start()
    {
        blueTeam = 0;
        redTeam = 0;
        capturePoints = 0;
        activeRock = 0;
    }

    // Update is called once per frame
    void Update()
    {
        time = Time.deltaTime;
        Debug.Log("TIMER:" + time);

        activeRock = Mathf.FloorToInt((capturePoints / maxPoints) * numberRocks);
        Debug.Log("activeRock:" + activeRock);
        CapturePoints(time);
        

    }

    void CapturePoints(float timePassed)
    {
        if (blueTeam > redTeam)
        {
            capturePoints += timePassed;
        }
        else if (blueTeam > redTeam)
        {
            capturePoints -= timePassed;
        }
        else if (blueTeam == 0 && redTeam == 0)
        {
            if (capturePoints > 0)
                capturePoints -= timePassed;
            else
                capturePoints += timePassed;
        }
    }

  
    void OnTriggerEnter(Collider collider)
    {
        Debug.Log("Enter");
        //TO DO set points;

        if (collider.gameObject.tag == "blue")
            blueTeam++;
        else
            redTeam++;

    }

    void OnTriggerExit(Collider collider)
    {
        Debug.Log("Leave");

        if (collider.gameObject.tag == "blue")
            blueTeam--;
        else
            redTeam--;
    }

}
