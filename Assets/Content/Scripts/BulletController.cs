using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    Transform bulletTr;
    Rigidbody bulletRb;      // Rigidbody con b mayúscula

    public float bulletPower = 0f;  // ajusta según necesites
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

        // CORRECCIÓN: inicializar la posición anterior para evitar raycasts enormes
        lastBulletPos = bulletTr.position;

        // CORRECCIÓN: obtener la máscara por nombre (usa el nombre exacto de tu layer)
        // En tu proyecto usaste "Hitbox" en otros scripts, por eso aquí uso "Hitbox".
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
        // CORRECCIÓN: la dirección es desde la posición anterior hacia la nueva
        Vector3 bulletDirection = bulletNewPos - lastBulletPos;
        float dist = bulletDirection.magnitude;

        if (dist <= 0.0001f)
        {
            // sin movimiento, nada que chequear
            lastBulletPos = bulletNewPos;
            return;
        }

        RaycastHit hit;

        // CORRECCIÓN: usar la máscara en el raycast para filtrar únicamente los hitboxes
        if (Physics.Raycast(lastBulletPos, bulletDirection.normalized, out hit, dist, hitboxMask.value))
        {
            GameObject go = hit.collider.gameObject;

            // Intentamos obtener BodyPartHitCheck en el objeto impactado
            BodyPartHitCheck playerBodyPart = go.GetComponent<BodyPartHitCheck>();

            if (playerBodyPart != null)
            {
                playerBodyPart.TakeHit(bulletDamage);
                Debug.Log("Disparo en " + playerBodyPart.BodyName);
            }
            else
            {
                // si el collider está en un hijo (común), buscar hacia arriba
                BodyPartHitCheck parentCheck = go.GetComponentInParent<BodyPartHitCheck>();
                if (parentCheck != null)
                {
                    parentCheck.TakeHit(bulletDamage);
                    Debug.Log("Disparo en (parent) " + parentCheck.BodyName);
                }
            }

            // destruir la bala al impactar
            Destroy(this.gameObject);
        }

        lastBulletPos = bulletNewPos;
    }
}
