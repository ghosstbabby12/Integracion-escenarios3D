using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("World-Space Health Bar")]
    public Canvas healthBarCanvas;
    public Image healthBarFill;
    public bool hideWhenFull = true;
    public float hideDelay = 2f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hitSound;
    public AudioClip deathSound;

    [Header("Drops")]
    public GameObject[] dropItems;
    public float dropChance = 0.5f;
    public float destroyDelay = 5f;

    private Animator anim;
    private NavMeshAgent agent;
    private bool isDead = false;
    private float lastHitTime = 0f;

    void Awake()
    {
        currentHealth = maxHealth;

        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (healthBarCanvas != null && hideWhenFull)
            healthBarCanvas.enabled = false;
    }

    void Update()
    {
        // Ocultar barra si está full
        if (healthBarCanvas && hideWhenFull && currentHealth >= maxHealth)
        {
            if (Time.time - lastHitTime > hideDelay)
                healthBarCanvas.enabled = false;
        }

        // Rotar barra hacia la cámara
        if (healthBarCanvas && healthBarCanvas.enabled && Camera.main != null)
        {
            healthBarCanvas.transform.LookAt(Camera.main.transform);
            healthBarCanvas.transform.Rotate(0, 180, 0);
        }
    }

    // ===== RECIBIR DAÑO =====
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        lastHitTime = Time.time;

        Debug.Log($"Enemy recibió {damage} daño → Salud actual: {currentHealth}");

        // Mostrar barra
        if (healthBarCanvas)
            healthBarCanvas.enabled = true;

        // Actualizar barra
        if (healthBarFill)
            healthBarFill.fillAmount = currentHealth / maxHealth;

        // Sonido HIT
        if (audioSource && hitSound)
            audioSource.PlayOneShot(hitSound);

        // Animación HIT
        if (anim && currentHealth > 0)
            anim.SetTrigger("Hit");

        // Muerte
        if (currentHealth <= 0)
            Die();
    }

    // ===== MUERTE =====
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("💀 ENEMIGO MUERTO");

        // Sonido de muerte
        if (audioSource && deathSound)
            audioSource.PlayOneShot(deathSound);

        // Detener movimiento
        if (agent)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Animación Death
        if (anim)
        {
            anim.SetTrigger("Die");
            anim.SetBool("IsDead", true);
        }

        // Desactivar colisiones
        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        // Ocultar barra
        if (healthBarCanvas)
            healthBarCanvas.enabled = false;

        DropLoot();

        // Destruir después de delay
        Destroy(gameObject, destroyDelay);
    }

    // ===== LOOT =====
    private void DropLoot()
    {
        if (dropItems.Length == 0) return;
        if (Random.value > dropChance) return;

        GameObject drop = dropItems[Random.Range(0, dropItems.Length)];
        Instantiate(drop, transform.position + Vector3.up, Quaternion.identity);
    }

    public bool IsDead() => isDead;
    public float HealthPercent() => currentHealth / maxHealth;
}
