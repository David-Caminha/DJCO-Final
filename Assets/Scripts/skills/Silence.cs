using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Silence : NetworkBehaviour
{
    float silenceTime;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Explosion(new Vector2(transform.position.x, transform.position.z), 3);
        }
    }

    void Explosion(Vector2 center, float radius)
    {
        GameObject otherPlayer;
        List<GameObject> enemiesHit = new List<GameObject>;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, radius);
        int i = 0;
        while (i < hitColliders.Length)
        {
            otherPlayer = hitColliders[i].gameObject;
            if (otherPlayer.tag == "enemyplayer")
            {
                enemiesHit.Add(otherPlayer);
            }
        }
        CmdHitPlayer(enemiesHit);
    }

    [Command]
    void CmdHitPlayer(List<GameObject> enemiesHit)
    {
        foreach (GameObject enemy in enemiesHit)
            enemy.GetComponent<PlayerStats>().RpcSilencePlayer(silenceTime);
    }
}
