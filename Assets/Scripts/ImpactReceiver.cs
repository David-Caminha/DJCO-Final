using UnityEngine;
using System.Collections;

public class ImpactReceiver : MonoBehaviour {
    
    Vector3 impact = Vector3.zero;
    private CharacterController characterControler;

    void Start()
    {
        characterControler = GetComponent<CharacterController>();
    }

    // call this function to add an impact force: 
    void AddImpact(Vector3 dir, float force)
    {
        dir.Normalize();
        if (dir.y < 0)
            dir.y = -dir.y; // reflect down force on the ground 
        impact += dir.normalized * force;
    }

    void Update()
    {
        // apply the impact force: 
        if (impact.magnitude > 0.2)
            characterControler.Move(impact * Time.deltaTime);
        // consumes the impact energy each cycle: 
        impact = Vector3.Lerp(impact, Vector3.zero, 5*Time.deltaTime); }
    }
