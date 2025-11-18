using UnityEngine;
using System.Reflection;

public class Hitbox : MonoBehaviour
{
    [Tooltip("Referencia al script del enemigo que recibe daño. Si se deja vacío, se intenta encontrar en los padres.")]
    public MonoBehaviour owner;

    void Awake()
    {
        if (owner == null)
        {
            owner = GetComponentInParent<EnemyZombi>()
                 
                 ?? (MonoBehaviour)GetComponentInParent<EnemyZombi>();
        }
    }

    public void ApplyDamage(float damage)
    {
        if (owner == null) return;

    
        if (owner is EnemyZombi ez)   { ez.TakeDamage(damage); return; }

        // Fallback reflexión
        var t = owner.GetType();
        var m = t.GetMethod("TakeDamage", new System.Type[] { typeof(float) });
        if (m != null) m.Invoke(owner, new object[] { damage });
    }
}

