using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    Animator playerAnim;
    Rigidbody playerBody;
    Rigidbody[] playerBones;
    ParticipantController PLAYER;

    public List<HitMultiplier> hitStats;

    void Awake()
    {
        playerAnim = GetComponent<Animator>();
        if (playerAnim == null) playerAnim = GetComponentInChildren<Animator>();

        playerBody = GetComponentInParent<Rigidbody>();
        playerBones = GetComponentsInChildren<Rigidbody>();
        PLAYER = GetComponentInParent<ParticipantController>();
    }

    void Start()
    {
        SetUp();
    }

    public void SetUp()
    {
        int layerOfHits = LayerMask.NameToLayer("Hitbox");

        foreach (Rigidbody bone in playerBones)
        {
            bone.collisionDetectionMode = CollisionDetectionMode.Continuous;
            bone.gameObject.layer = layerOfHits;

            BodyPartHitCheck partToCheck = bone.gameObject.AddComponent<BodyPartHitCheck>();
            partToCheck.PLAYER = PLAYER;
            string bName = bone.gameObject.name.ToLower();

            foreach (HitMultiplier hit in hitStats)
            {
                if (bName.Contains(hit.boneName))
                {
                    partToCheck.Multiplier = hit.multiplyBy;
                    partToCheck.BodyName = hit.boneName;
                    break;
                }
            }
        }
        Active(false);
    }

    public void Active(bool state)
    {
        foreach (Rigidbody bone in playerBones)
        {
            Collider c = bone.GetComponent<Collider>();
            if (c == null) continue;

            if (bone.useGravity != state)
            {
                c.isTrigger = !state;
                bone.isKinematic = !state;
                bone.useGravity = state;
                if (playerBody != null) bone.velocity = playerBody.velocity;
            }
        }

        if (playerAnim != null) playerAnim.enabled = !state;
        if (playerBody != null)
        {
            playerBody.useGravity = !state;
            playerBody.detectCollisions = !state;
            playerBody.isKinematic = state;
        }
    }
}

[System.Serializable]
public class HitMultiplier
{
    public string boneName = "head";
    public float multiplyBy = 1;
}
