using UnityEngine;
using System.Collections;
using System.Linq.Expressions;

public class Teleport : MonoBehaviour
{

    private GameObject[] teamMates;
    private int leastHealth = 100000;
    private int teamMateHealth = 0;
    private GameObject playerObjective = null;
    private Vector3 positionPlayerObjective;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            //Search which teamMate has the least health and save it on playerObjective
            teamMates = GameObject.FindGameObjectsWithTag("teammate");
            foreach (GameObject teamMate in teamMates)
            {
                teamMateHealth = teamMate.GetComponent<temp>().health;
                print("If" + teamMateHealth);
                if (teamMateHealth != 0 && teamMateHealth < leastHealth)
                {
                    
                    leastHealth = teamMate.GetComponent<temp>().health;
                    playerObjective = teamMate;
                }
            }
            positionPlayerObjective = playerObjective.transform.position;

            //ERROR de entrar nas paredes
            transform.position = positionPlayerObjective + new Vector3(0, 0, 3);
        }
    }
}
