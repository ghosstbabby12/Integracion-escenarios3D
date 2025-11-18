using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealtBar : MonoBehaviour
{
    public ParticipantController thePlayer;
    public Image lifebarFill;
    private RectTransform reactTransform;

    void Start()
    {
        reactTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        float healthPercentage = thePlayer.currentHealth / thePlayer.maxHealth;
        reactTransform.localScale = new Vector3(healthPercentage, 1, 1);
        lifebarFill.color = Color.Lerp(Color.red, Color.green, healthPercentage);
    }
}
