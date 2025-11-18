using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyZombi : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip walkClip;
    public AudioClip attackClip;
    public AudioClip hitClip;
    public AudioClip deathClip;

    [Header("Objetivos y combate")]
    public Transform player;
    public float detectionRange = 15f;
    public float attackRange = 2f;
    public float attackCooldown = 0.8f;
    public float damage = 10f;

    [Header("Salud")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider enemyHealthSlider;

    [Header("Efectos visuales")]
    public SkinnedMeshRenderer bodyRenderer;
    public Color hitColor = Color.red;
    public float flashDuration = 0.2f;
    public ParticleSystem bloodEffect;

    private Color originalColor;
    private Animator anim;
    private NavMeshAgent agent;
    private PlayerHealth targetPlayerHealth;
    private bool isDead = false;
    private float lastAttackTime = 0f;

    [Header("Depuración y movimiento")]
    public bool debugAnimation = false;
    public float fallbackMoveSpeed = 1.2f;
    public float animMinMoveSpeedParam = 1f;

    void Awake()
    {
        anim = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        if (anim != null) anim.applyRootMotion = false;

        var rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Start()
    {
        currentHealth = maxHealth;

        if (bodyRenderer != null)
            originalColor = bodyRenderer.material.color;

        if (agent != null)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
                transform.position = hit.position;

            agent.enabled = true;
            agent.speed = Mathf.Max(agent.speed, 3.5f);
            agent.acceleration = Mathf.Max(agent.acceleration, 8f);
            agent.angularSpeed = Mathf.Max(agent.angularSpeed, 120f);
            agent.stoppingDistance = Mathf.Max(0.06f, attackRange * 0.6f);
        }

        if (player == null)
        {
            var found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }

        targetPlayerHealth = player?.GetComponent<PlayerHealth>() ??
                             player?.GetComponentInChildren<PlayerHealth>() ??
                             player?.GetComponentInParent<PlayerHealth>();

        if (enemyHealthSlider != null)
        {
            enemyHealthSlider.maxValue = maxHealth;
            enemyHealthSlider.value = currentHealth;
        }
    }

    void Update()
    {
        if (isDead || player == null || targetPlayerHealth == null) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;

        bool inDetection = distance <= detectionRange;
        bool inAttack = distance <= attackRange;

        float speed = 0f;
        if (agent != null && agent.enabled)
        {
            float v = agent.velocity.magnitude;
            float dv = agent.desiredVelocity.magnitude;
            speed = v > 0.05f ? v : dv;
            if (inDetection && !inAttack)
                speed = Mathf.Max(speed, animMinMoveSpeedParam);
        }
        else if (inDetection && !inAttack)
            speed = fallbackMoveSpeed;

        anim?.SetFloat("Speed", speed);

        // ---------- ATAQUE ----------
        if (inAttack)
        {
            if (agent != null && agent.enabled) agent.isStopped = true;
            anim?.SetBool("IsAttacking", true);

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                targetPlayerHealth.TakeDamage(damage);

                if (attackClip != null)
                    audioSource.PlayOneShot(attackClip);

                lastAttackTime = Time.time;
            }

            StopWalkSound();
        }
        // ---------- PERSECUCIÓN ----------
        else if (inDetection)
        {
            if (agent != null && agent.enabled)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }

            anim?.SetBool("IsAttacking", false);

            PlayWalkSound();
        }
        // ---------- IDLE ----------
        else
        {
            if (agent != null && agent.enabled) agent.isStopped = true;

            anim?.SetBool("IsAttacking", false);

            StopWalkSound();
        }
    }

    // ---------------- SONIDOS ------------------

    private void PlayWalkSound()
    {
        if (walkClip != null && !audioSource.isPlaying)
        {
            audioSource.clip = walkClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void StopWalkSound()
    {
        if (audioSource.isPlaying && audioSource.clip == walkClip)
        {
            audioSource.Stop();
        }
    }

    // ---------------- DAÑO ------------------

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (enemyHealthSlider != null)
            enemyHealthSlider.value = currentHealth;

        if (hitClip != null)
            audioSource.PlayOneShot(hitClip);

        anim?.SetTrigger("Hit");

        if (bloodEffect != null)
            bloodEffect.Play();

        if (bodyRenderer != null)
            StartCoroutine(FlashRed());

        if (currentHealth <= 0f)
            Die();
    }

    private System.Collections.IEnumerator FlashRed()
    {
        bodyRenderer.material.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        bodyRenderer.material.color = originalColor;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        anim?.SetTrigger("Die");

        if (deathClip != null)
            audioSource.PlayOneShot(deathClip);

        if (agent != null && agent.enabled)
            agent.isStopped = true;

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (enemyHealthSlider != null)
            enemyHealthSlider.value = 0;

        Destroy(gameObject, 5f);
    }
}
