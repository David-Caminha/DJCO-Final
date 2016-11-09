using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Freeze : Skill
{
    public AudioSource skillSoundSource;
    public AudioClip freezeSound;

    public float freezeTime = 3.5f;
    public float cooldownTime = 8f;

    public float range;
    public float angle;

    public override void Activate()
    {
        CmdPlayFreezeSound();
        var enemiesHit = ConeAreaOfEffect(transform.position, range, angle);
        foreach (GameObject enemy in enemiesHit)
        {
            CmdFreezePlayer(enemy);
        }
    }

    void Start()
    {
        range = 55;
        angle = 50;
    }

    [Command]
    void CmdFreezePlayer(GameObject enemy)
    {
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
