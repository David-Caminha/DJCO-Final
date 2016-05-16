using UnityEngine;
using System.Collections;

public class Silence : MonoBehaviour {

	// Use this for initialization
    private GameObject otherPlayer;
	void Start () {
	
	}
    void Explosion(Vector2 center, float radius)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, radius);
        int i = 0;
        while (i < hitColliders.Length)
        {
            otherPlayer = hitColliders[i].gameObject;
            if (otherPlayer.tag == "enemyplayer")
                otherPlayer.GetComponent<temp>().silence = true;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {     
            Explosion(new Vector2(transform.position.x, transform.position.z), 3);
        }
    }
}
