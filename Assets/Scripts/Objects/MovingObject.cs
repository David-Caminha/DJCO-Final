using UnityEngine;
using System.Collections;

public class MovingObject : MonoBehaviour
{
    public Transform[] positions;
    public float secondsForMove;
    public float delay;

    private float time;
    private bool goingBack;

    private int currentPosition;

    // Use this for initialization
    void Start()
    {
        goingBack = false;
        currentPosition = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!goingBack)
        {
            transform.position = Vector3.Lerp(positions[currentPosition].position, positions[currentPosition + 1].position, time / secondsForMove);
            time += Time.deltaTime;
            if (time >= secondsForMove && currentPosition < positions.Length - 2)
            {
                time = 0;
                currentPosition++;
            }
            else if (time >= delay + secondsForMove)
            {
                currentPosition++;
                goingBack = true;
                time = 0;
            }
        }
        else
        {
            transform.position = Vector3.Lerp(positions[currentPosition].position, positions[currentPosition - 1].position, time / secondsForMove);
            time += Time.deltaTime;
            if (time >= secondsForMove && currentPosition > 1)
            {
                time = 0;
                currentPosition--;
            }
            else if (time >= delay + secondsForMove)
            {
                currentPosition--;
                goingBack = false;
                time = 0;
            }
        }
    }
}
