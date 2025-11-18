using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform playerCamera;
    public float shotDistance = 25f;
    public float impactForce = 5f;
    public LayerMask shotMask;

    [Header("Effects")]
    public GameObject destroyEffect;
    public ParticleSystem shootParticles;
    public GameObject hitEffect;
    public GameObject bloodEffectEnemy;

    [Header("Audio")]
    public AudioClip hitSound;
    public AudioSource audioSource;

    private RaycastHit hit;

    void Update()
    {
        Debug.DrawRay(playerCamera.position, playerCamera.forward * shotDistance, Color.red);

        if (Input.GetButtonDown("Fire1"))
            Shoot();
    }

    private void Shoot()
    {
        if (shootParticles != null)
            shootParticles.Play();

        // ---------------------------------------------------------------------
        // RAYCAST → Detecta SIEMPRE incluso con MeshCollider NO convex
        // ---------------------------------------------------------------------
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, shotDistance, shotMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("Shot hit: " + hit.collider.name);

            // Impact effect (pared, suelo, objetos)
            if (hitEffect != null)
                Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));

            // Force push
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddForce(-hit.normal * impactForce, ForceMode.Impulse);

            // Play hit sound
            if (hitSound != null && audioSource != null)
                audioSource.PlayOneShot(hitSound);

            // ---------------------------------------------------------------------
            //                       DETECTAR ENEMIGO
            // ---------------------------------------------------------------------
            EnemyZombi enemy = hit.collider.GetComponentInParent<EnemyZombi>();

            if (enemy != null)
            {
                Debug.Log("🩸 Enemy HIT detected!");

                // Sangre
                if (bloodEffectEnemy != null)
                {
                    Instantiate(
                        bloodEffectEnemy,
                        hit.point,
                        Quaternion.LookRotation(hit.normal)
                    );
                }

                // Aplicar daño
                enemy.TakeDamage(50f);

                return; // ← Para no seguir procesando
            }

            // ---------------------------------------------------------------------
            //                  SI ES UN BARRIL
            // ---------------------------------------------------------------------
            if (hit.collider.CompareTag("Barrel"))
            {
                if (destroyEffect != null)
                    Instantiate(destroyEffect, hit.point, Quaternion.LookRotation(hit.normal));

                if (LevelManager.instance != null)
                    LevelManager.instance.levelScore++;

                Destroy(hit.collider.gameObject);
            }
        }
    }
}
