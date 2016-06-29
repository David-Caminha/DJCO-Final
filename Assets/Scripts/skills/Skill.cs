using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public abstract class Skill : NetworkBehaviour
{
    public abstract void Activate();

    public List<GameObject> AreaOfEffect(Vector3 center, float radius)
    {
        string[] layers = { LayerMask.LayerToName(gameObject.layer) };
        LayerMask layerMask = LayerMask.GetMask(layers);
        List<GameObject> enemiesHit = new List<GameObject>();
        Collider[] hitColliders = Physics.OverlapSphere(center, radius, layerMask);
        foreach (Collider hit in hitColliders)
        {
            GameObject otherPlayer = hit.gameObject;
            if (!otherPlayer.tag.Equals(tag))
            {
                enemiesHit.Add(otherPlayer);
            }
        }
        return enemiesHit;
    }

    public List<GameObject> ConeAreaOfEffect(Vector3 center, float radius, float angle)
    {
        string[] layers = { LayerMask.LayerToName(gameObject.layer) };
        LayerMask layerMask = LayerMask.GetMask(layers);
        List<GameObject> enemiesHit = new List<GameObject>();
        Collider[] hitColliders = Physics.OverlapSphere(center, radius, layerMask);
        foreach (Collider hit in hitColliders)
        {
            GameObject otherPlayer = hit.gameObject;
            if (!otherPlayer.tag.Equals(tag))
            {
                Vector3 enemyDirection = (otherPlayer.transform.position - transform.position).normalized;
                float dotProduct = Vector3.Dot(transform.forward, enemyDirection);

                if (dotProduct >= Mathf.Cos(angle * Mathf.Deg2Rad))
                    enemiesHit.Add(otherPlayer);
            }
        }
        print(enemiesHit);
        return enemiesHit;
    }
}
