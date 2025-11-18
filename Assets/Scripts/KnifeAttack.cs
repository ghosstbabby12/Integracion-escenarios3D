using UnityEngine;

public class KnifeAttack : MonoBehaviour
{
    [Header("Configuración de ataque")]
    public float attackRange = 1.5f;      // Distancia máxima para golpear
    public float damage = 25f;            // Daño que inflige cada golpe
    public float attackCooldown = 0.5f;   // Tiempo entre ataques
    public LayerMask enemyLayer;          // Capa de enemigos

    private float nextAttackTime = 0f;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void Attack()
    {
        if (animator != null)
            animator.SetTrigger("Attack"); // Activa animación de golpe

        // Detecta enemigos cercanos en un radio
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            var enemy = enemyCollider.GetComponent<MonoBehaviour>();
            if (enemy == null) continue;

            // Usa reflexión para buscar "health" y restar daño
            var type = enemy.GetType();
            var field = type.GetField("health");
            if (field != null && field.FieldType == typeof(float))
            {
                float health = (float)field.GetValue(enemy);
                health -= damage;
                field.SetValue(enemy, health);
                Debug.Log($"Le hiciste {damage} de daño a {enemy.name}. Salud restante: {health}");
                continue;
            }

            var prop = type.GetProperty("health");
            if (prop != null && prop.CanWrite && prop.PropertyType == typeof(float))
            {
                float health = (float)prop.GetValue(enemy);
                health -= damage;
                prop.SetValue(enemy, health);
                Debug.Log($"Le hiciste {damage} de daño a {enemy.name}. Salud restante: {health}");
            }
        }
    }

    // Dibuja el rango de ataque en la escena
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
