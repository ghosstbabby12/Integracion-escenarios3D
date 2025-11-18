using UnityEngine;

public class KnifeMelee : MonoBehaviour
{
    [Header("Configuración del cuchillo")]
    public float damage = 25f;
    public float attackCooldown = 1f;
    public AudioClip swingSound;
    public AudioClip hitSound;
    public ParticleSystem hitEffect;
    public Animator animator;

    private AudioSource audioSource;
    private bool canAttack = true;
    private bool attackActive = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (LevelManager.instance != null && !LevelManager.instance.isGameActive) return;

        if (Input.GetButtonDown("Fire1") && canAttack)
        {
            StartCoroutine(PerformAttack());
        }
    }

    private System.Collections.IEnumerator PerformAttack()
    {
        canAttack = false;
        attackActive = true;

        // Reproduce animación y sonido
        if (animator != null)
            animator.SetTrigger("Attack");

        if (swingSound != null)
            audioSource.PlayOneShot(swingSound);

        // Espera el tiempo del golpe activo
        yield return new WaitForSeconds(0.3f);

        attackActive = false;

        // Espera el enfriamiento antes del siguiente golpe
        yield return new WaitForSeconds(attackCooldown - 0.3f);

        canAttack = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!attackActive) return;

        // Evitar auto-daño
        if (other.CompareTag("Player")) return;

        // Aplica daño si el objeto tiene un método TakeDamage
        var mb = other.GetComponentInParent<MonoBehaviour>();
        if (mb != null)
        {
            var t = mb.GetType();
            var m = t.GetMethod("TakeDamage", new System.Type[] { typeof(float) });
            if (m != null)
            {
                m.Invoke(mb, new object[] { damage });
                Debug.Log("Cuchillo impactó: " + other.name + " con daño " + damage);
            }
        }

        // Efectos visuales y sonido de impacto
        if (hitEffect != null)
            Instantiate(hitEffect, other.ClosestPoint(transform.position), Quaternion.identity);

        if (hitSound != null)
            audioSource.PlayOneShot(hitSound);
    }
}
