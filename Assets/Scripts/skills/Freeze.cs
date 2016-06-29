using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Freeze : Skill
{
    public AudioSource skillSoundSource;
    public AudioClip freezeSound;

    public float freezeTime = 1.2f;
    public float cooldownTime = 8f;

    public float range;
    public float angle;

    public override void Activate()
    {
        CmdPlayFreezeSound();
        print("player from " + tag + "using freeze!");
        var enemiesHit = ConeAreaOfEffect(transform.position, range, angle);
        foreach (GameObject enemy in enemiesHit)
        {
            print("Sending command for " + enemy);
            CmdFreezePlayer(enemy);
        }
    }

    void Start()
    {
        range = 45;
        angle = 50;
    }

    [Command]
    void CmdFreezePlayer(GameObject enemy)
    {
        print("calling rpc for player " + enemy.name + " in " + enemy.tag);
        enemy.GetComponent<PlayerStats>().RpcFreezePlayer(freezeTime);
    }

    [Command]
    void CmdPlayFreezeSound()
    {
        RpcPlayFreezeSound();
    }

    [ClientRpc]
    void RpcPlayFreezeSound()
    {
        Invoke("PlayFreezeSound", 0.5f);
    }

    private void PlayFreezeSound()
    {
        skillSoundSource.clip = freezeSound;
        skillSoundSource.Play();
    }
}
