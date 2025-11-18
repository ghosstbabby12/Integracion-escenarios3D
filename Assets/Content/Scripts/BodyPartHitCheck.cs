using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPartHitCheck : MonoBehaviour
{
    [HideInInspector]
    public ParticipantController PLAYER;

    public string BodyName;
    public float Multiplier;
    public float LastDamage;

    void Start() { }

    void Update() { }

    public void TakeHit(float damage)
    {
        LastDamage = damage * Multiplier;

        // Previene NullReference si PLAYER no fue asignado en este hueso
        if (PLAYER != null)
        {
            this.PLAYER.TakeDamage(LastDamage);
        }
        else
        {
            Debug.LogError("PLAYER es null en " + gameObject.name + " (BodyPartHitCheck). Asigna PLAYER en SetUp del Ragdoll.");
        }

        Debug.Log(damage + " * " + Multiplier + " = " + LastDamage);
    }
}
