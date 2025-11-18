using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    Transform bulletTr;
    Rigidbody bulletRb;

    public float bulletPower = 0f;
    public float lifeTime = 4f;
    private float time = 0f;
    public float bulletDamage = 1;
    Vector3 lastBulletPos;

    public LayerMask hitboxMask;

    void Start()
    {
        bulletTr = transform;
        bulletRb = GetComponent<Rigidbody>();
        bulletRb.velocity = transform.forward * bulletPower;
        lastBulletPos = bulletTr.position;
        hitboxMask = LayerMask.GetMask("Hitbox");
    }

    void FixedUpdate()
    {
        time += Time.deltaTime;
        DetectCollision();

        if (time >= lifeTime)
        {
            Destroy(this.gameObject);
        }
    }

    public void DetectCollision()
    {
        Vector3 bulletNewPos = bulletTr.position;
        Vector3 bulletDirection = bulletNewPos - lastBulletPos;
        float dist = bulletDirection.magnitude;

        if (dist <= 0.0001f)
        {
            lastBulletPos = bulletNewPos;
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(lastBulletPos, bulletDirection.normalized, out hit, dist, hitboxMask.value))
        {
            GameObject go = hit.collider.gameObject;
            BodyPartHitCheck playerBodyPart = go.GetComponent<BodyPartHitCheck>();

            if (playerBodyPart != null)
            {
                playerBodyPart.TakeHit(bulletDamage);
                Debug.Log("Disparo en " + playerBodyPart.BodyName);
            }
            else
            {
                BodyPartHitCheck parentCheck = go.GetComponentInParent<BodyPartHitCheck>();
                if (parentCheck != null)
                {
                    parentCheck.TakeHit(bulletDamage);
                    Debug.Log("Disparo en (parent) " + parentCheck.BodyName);
                }
            }

            Destroy(this.gameObject);
        }

        lastBulletPos = bulletNewPos;
    }
}
