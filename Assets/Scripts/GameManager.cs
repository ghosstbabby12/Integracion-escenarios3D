using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int score = 0;
    public int scoreToWin = 4;

    private void Awake()
    {
        instance = this;
    }

    public void AddScore(int value)
    {
        score += value;
        Debug.Log("Puntaje: " + score);

        if (score >= scoreToWin)
        {
            Debug.Log("Nivel completado ✅");
            SceneManager.LoadScene("World_2_Asylum"); // siguiente nivel
        }
    }
}
