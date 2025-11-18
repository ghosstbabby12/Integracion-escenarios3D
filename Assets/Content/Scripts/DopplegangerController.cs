using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class DopplegangerController : MonoBehaviour
{
    public bool Player = false;
    public bool Active = true;

    // Referencias
    Transform selfTr;
    Rigidbody enemyRb;
    Animator enemyAnim;
    NavMeshAgent agent;

    // Vida
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    // Movimiento / Persecución
    public Transform player; // arrástralo o usa tag "Player"
    ParticipantController playerCtrl;
    public float playerSpeed = 3.5f;
    public float detectionRange = 12f;
    public float stopDistance = 1.6f;

    // Ataque por contacto
    public float touchDamage = 10f;
    public float touchCooldown = 0.8f;
    public float touchRange = 1.9f;
    float lastTouchTime = -99f;

    // Animator
    static readonly int ANIM_RUN = Animator.StringToHash("Run Forward"); // Cambia el nombre si tu parámetro es diferente

    void Start()
    {
        selfTr = transform;
        enemyRb = GetComponent<Rigidbody>();
        enemyAnim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();

        // Resolver jugador
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }
        if (player != null)
        {
            playerCtrl =
                player.GetComponent<ParticipantController>() ??
                player.GetComponentInChildren<ParticipantController>() ??
                player.GetComponentInParent<ParticipantController>();
        }

        // Configurar agente
        if (agent != null)
        {
            agent.stoppingDistance = stopDistance;
            agent.speed = Mathf.Max(agent.speed, playerSpeed);
            agent.angularSpeed = Mathf.Max(agent.angularSpeed, 720f);
            agent.acceleration = Mathf.Max(agent.acceleration, 8f);
        }

        if (currentHealth <= 0f) currentHealth = maxHealth;
        Active = true;
    }

    void Update()
    {
        if (!Active) return;

        // Verificar jugador válido
        if (player == null || playerCtrl == null || !playerCtrl.Active)
        {
            StopMove();
            return;
        }

        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        Vector3 toPlayer = player.position - selfTr.position;
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;

        // Perseguir
        if (distance <= detectionRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            if (enemyAnim != null) enemyAnim.SetBool(ANIM_RUN, true);
        }
        else
        {
            StopMove();
        }

        // Atacar por contacto
        if (distance <= touchRange && Time.time >= lastTouchTime + touchCooldown)
        {
            playerCtrl.TakeDamage(touchDamage);
            lastTouchTime = Time.time;
        }
    }

    void StopMove()
    {
        if (agent != null && agent.enabled) agent.isStopped = true;
        if (enemyAnim != null) enemyAnim.SetBool(ANIM_RUN, false);
    }

    public void TakeDamage(float damage)
    {
        if (!Active) return;

        currentHealth -= damage;
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    void Die()
    {
        Active = false;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Trigger de muerte en Animator (opcional)
        // if (enemyAnim != null) enemyAnim.SetTrigger(Animator.StringToHash("Die"));

        Destroy(gameObject, 5f);
    }
}
