using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public EnemyZombi enemy; // referencia al enemigo
    public Slider slider;
    public Text valueText;

    void Start()
    {
        if (enemy == null)
            enemy = GetComponentInParent<EnemyZombi>();

        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
        }

        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (enemy == null || slider == null) return;

        float normalized = Mathf.Clamp01(enemy.currentHealth / enemy.maxHealth);
        slider.value = normalized;

        if (valueText != null)
        {
            int hpInt = Mathf.CeilToInt(enemy.currentHealth);
            valueText.text = hpInt.ToString();
        }
    }
}
