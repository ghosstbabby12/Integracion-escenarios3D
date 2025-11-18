using UnityEngine;
using UnityEngine.UI;

public class Knife : MonoBehaviour
{
    [Header("Configuración de ataque")]
    public Transform attackOrigin;
    public float attackRange = 2f;
    public float damage = 25f;
    public LayerMask attackMask;

    [Header("Feedback visual")]
    public Image damageFlash;
    public Color flashColor = new Color(1, 0, 0, 0.4f);
    public float flashDuration = 0.2f;

    [Header("Audio")]
    public AudioClip attackSound;
    public AudioSource audioSource;

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Attack();
        }
    }

    private void Attack()
    {
        Debug.Log("🔪 Ataque ejecutado (Knife)");

        if (attackSound && audioSource)
            audioSource.PlayOneShot(attackSound);

        if (damageFlash != null)
            StartCoroutine(FlashDamage());

        RaycastHit hit;
        if (Physics.Raycast(attackOrigin.position, attackOrigin.forward, out hit, attackRange, attackMask))
        {
            Debug.Log("✅ Golpeó: " + hit.collider.name);

            EnemyZombi ez = hit.collider.GetComponentInParent<EnemyZombi>();
            if (ez != null)
            {
                ez.TakeDamage(damage);
                Debug.Log($"🩸 Daño aplicado al zombi ({damage} de daño)");
            }
        }
        else
        {
            Debug.Log("❌ No impactó nada");
        }
    }

    private System.Collections.IEnumerator FlashDamage()
    {
        if (damageFlash == null) yield break;

        damageFlash.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        damageFlash.color = new Color(1, 0, 0, 0);
    }
}
