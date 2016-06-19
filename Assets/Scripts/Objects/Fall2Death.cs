using UnityEngine;
using System.Collections;

public class Fall2Death : MonoBehaviour {

    void OnTriggerEnter(Collider other)
    {
        if(other.tag.StartsWith("Team"))
        {
            other.GetComponent<ThirdPersonController>().DieFromFall();
        }
    }
}
