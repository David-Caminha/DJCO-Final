using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Networking;

public class Silence : NetworkBehaviour
{
    float silenceTime;
    public GameObject particlePrefab;
    private ParticleSystem particle;
    public GameObject AoePrefab;
    private GameObject Aoe;

    void Start()
    {
       particle = Instantiate(particlePrefab).GetComponent<ParticleSystem>();
       Aoe = Instantiate(AoePrefab);

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Silence");
            Explosion(new Vector2(transform.position.x, transform.position.z), 30);
        }
    }

    void Explosion(Vector2 center, float radius)
    {
        particle.transform.position = transform.position;

     //TODO: Update unity
        particle.Stop();
        particle.Clear();
        particle.Play();

        Aoe.transform.position = transform.position;
        Aoe.transform.localScale = new Vector3(radius,0.05f,radius);
        GameObject otherPlayer;
        List<GameObject> enemiesHit = new List<GameObject>();
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
