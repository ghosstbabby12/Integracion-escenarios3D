using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Configuración de ataque")]
    public float attackRange = 2f;
    public int attackDamage = 20;
    public float attackCooldown = 0.8f;

    [Header("Detección")]
    public LayerMask enemyLayers;

    private float nextAttackTime = 0f;

    void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Attack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    void Attack()
    {
        Debug.Log("Ataque ejecutado 🔪");

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, enemyLayers);

        foreach (Collider enemy in hitEnemies)
        {
            EnemyHealth eh = enemy.GetComponent<EnemyHealth>();

            if (eh != null)
            {
                eh.TakeDamage(attackDamage);
                Debug.Log($"Daño aplicado a {enemy.name}: {attackDamage}");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
