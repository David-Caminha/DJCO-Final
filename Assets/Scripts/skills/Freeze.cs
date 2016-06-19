using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Freeze : Skill
{
    float freezeTime = 1.2f;
    float cooldownTime = 8f;

    public override void Activate()
    {
        print("player from " + tag + "using freeze!");
        var enemiesHit = ConeAreaOfEffect(new Vector2(transform.position.x, transform.position.z), 20, 60);
        CmdFreezePlayer(enemiesHit);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Activate();
        }
    }

    [Command]
    void CmdFreezePlayer(List<GameObject> enemiesHit)
    {
        print("Sending comand " + enemiesHit + " with a size of " + enemiesHit.Count);
        foreach (GameObject enemy in enemiesHit)
        {
            print("rpc called for player" + enemy.name + " in " + enemy.tag);
            enemy.GetComponent<PlayerStats>().RpcFreezePlayer(freezeTime);
        }
    }
}
