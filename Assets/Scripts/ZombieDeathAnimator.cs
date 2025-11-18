using UnityEngine;

/// <summary>
/// Maneja la animación de muerte del zombie
/// Se integra con EnemyHealth para activar la animación cuando muere
/// </summary>
[RequireComponent(typeof(Animator))]
public class ZombieDeathAnimator : MonoBehaviour
{
    [Header("Animación")]
    [Tooltip("Animator del zombie")]
    public Animator animator;

    [Tooltip("Nombre del trigger de muerte en el Animator")]
    public string deathTrigger = "Death";

    [Tooltip("Nombre del bool 'isDead' en el Animator")]
    public string isDeadBool = "isDead";

    [Header("Configuración")]
    [Tooltip("Tiempo antes de destruir el cuerpo (0 = nunca)")]
    public float destroyDelay = 5f;

    [Tooltip("¿Desactivar NavMeshAgent al morir?")]
    public bool disableNavMeshOnDeath = true;

    [Tooltip("¿Desactivar colliders al morir?")]
    public bool disableCollidersOnDeath = true;

    [Tooltip("¿Desactivar scripts de IA al morir?")]
    public bool disableAIScriptsOnDeath = true;

    [Header("Efectos")]
    [Tooltip("Partículas de sangre al morir (opcional)")]
    public GameObject bloodEffect;

    [Tooltip("Sonido de muerte (opcional)")]
    public AudioClip deathSound;

    [Header("Estado")]
    [Tooltip("¿Está muerto?")]
    public bool isDead = false;

    private AudioSource audioSource;
    private EnemyHealth enemyHealth;
    private UnityEngine.AI.NavMeshAgent navMeshAgent;
    private EnemyZombi enemyAI;
    private Rigidbody rb;

    void Start()
    {
        // Buscar componentes
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        enemyHealth = GetComponent<EnemyHealth>();
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        enemyAI = GetComponent<EnemyZombi>();
        rb = GetComponent<Rigidbody>();

        // Audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && deathSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
        }

        // Suscribirse al evento de muerte de EnemyHealth
        if (enemyHealth != null)
        {
            // EnemyHealth llamará a PlayDeathAnimation cuando muera
            Debug.Log($"✅ ZombieDeathAnimator inicializado en '{name}'");
        }
        else
        {
            Debug.LogWarning($"⚠️ '{name}' no tiene EnemyHealth - Las animaciones de muerte no funcionarán automáticamente");
        }
    }

    void Update()
    {
        // Verificar si el zombie murió (sin suscripción a eventos)
        if (!isDead && enemyHealth != null && enemyHealth.currentHealth <= 0)
        {
            PlayDeathAnimation();
        }
    }

    /// <summary>
    /// Reproduce la animación de muerte
    /// </summary>
    public void PlayDeathAnimation()
    {
        if (isDead) return; // Ya está muerto

        isDead = true;

        Debug.Log($"💀 Zombie '{name}' murió - Reproduciendo animación");

        // 1. Activar animación
        if (animator != null)
        {
            // Método 1: Trigger
            if (!string.IsNullOrEmpty(deathTrigger))
            {
                animator.SetTrigger(deathTrigger);
            }

            // Método 2: Bool
            if (!string.IsNullOrEmpty(isDeadBool))
            {
                animator.SetBool(isDeadBool, true);
            }

            Debug.Log($"   Animación activada: Trigger='{deathTrigger}', Bool='{isDeadBool}'");
        }
        else
        {
            Debug.LogWarning($"⚠️ '{name}' no tiene Animator - No se puede reproducir animación");
        }

        // 2. Reproducir sonido
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // 3. Reproducir efecto de sangre
        if (bloodEffect != null)
        {
            GameObject effect = Instantiate(bloodEffect, transform.position + Vector3.up, Quaternion.identity);
            Destroy(effect, 3f);
        }

        // 4. Desactivar NavMeshAgent
        if (disableNavMeshOnDeath && navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
            Debug.Log($"   NavMeshAgent desactivado");
        }

        // 5. Desactivar colliders (opcional - permite que el jugador atraviese el cuerpo)
        if (disableCollidersOnDeath)
        {
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }
            Debug.Log($"   {colliders.Length} collider(s) desactivados");
        }

        // 6. Desactivar scripts de IA
        if (disableAIScriptsOnDeath)
        {
            if (enemyAI != null)
            {
                enemyAI.enabled = false;
                Debug.Log($"   EnemyZombi script desactivado");
            }
        }

        // 7. Desactivar Rigidbody (evitar que se caiga raro)
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // 8. Destruir después de un tiempo (opcional)
        if (destroyDelay > 0)
        {
            Destroy(gameObject, destroyDelay);
            Debug.Log($"   Zombie será destruido en {destroyDelay} segundos");
        }

        // 9. Cambiar tag para que no sea targeteable
        gameObject.tag = "Dead";
    }

    /// <summary>
    /// Forzar muerte inmediata (para testing)
    /// </summary>
    public void ForceDeath()
    {
        PlayDeathAnimation();
    }

    /// <summary>
    /// Resetear zombie (para testing)
    /// </summary>
    public void ResetZombie()
    {
        isDead = false;

        if (animator != null && !string.IsNullOrEmpty(isDeadBool))
        {
            animator.SetBool(isDeadBool, false);
        }

        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = true;
        }

        if (enemyAI != null)
        {
            enemyAI.enabled = true;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }

        gameObject.tag = "Enemy";

        Debug.Log($"🔄 Zombie '{name}' reseteado");
    }
}
