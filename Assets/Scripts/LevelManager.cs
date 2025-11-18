using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Reflection;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    public int levelScore;
    public float levelTimer = 10f;

    [Header("Secuencia de escenas")]
    [Tooltip("Lista ordenada de escenas (índice 0: Nivel 1, índice 1: Nivel 2, resto: genéricos)")]
    public string[] levelNames = new string[0];

    [Header("Estado del juego")]
    public bool isGameActive = false;
    public GameObject mainMenuPanel;

    [Header("Mensajes")]
    public TextMeshProUGUI messageText;
    public float messageDuration = 2f;

    [Header("Configuración de detección")]
    [Tooltip("Tag utilizado para identificar enemigos en la escena")]
    public string enemyTag = "Enemy";

    private static bool hasSessionStarted = false;
    private bool levelEndTriggered = false;

    void Awake()
    {
        // singleton ligero
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);
    }

    void Start()
    {
        levelScore = 0;
        levelEndTriggered = false;

        if (messageText != null)
        {
            messageText.text = string.Empty;
            messageText.gameObject.SetActive(false);
        }

        if (!hasSessionStarted)
        {
            ShowMainMenu();
        }
        else
        {
            BeginGameplayState();
        }
    }

    void BeginGameplayState()
    {
        isGameActive = true;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
    }

    void Update()
    {
        if (!isGameActive) return;

        int currentIndex = GetLevelIndex(SceneManager.GetActiveScene().name);
        if (currentIndex == 0)
            HandleLevel1();
        else if (currentIndex == 1)
            HandleLevel2();
        else if (currentIndex >= 2)
            HandleGenericLevel();
    }

    public void StartGame()
    {
        hasSessionStarted = true;
        isGameActive = true;

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Recargar la escena actual para reiniciar todo (enemigos incluidos)
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ShowMainMenu()
    {
        isGameActive = false;
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // -------------------------
    // Handlers por tipo nivel
    // -------------------------
    private void HandleLevel1()
    {
        if (levelEndTriggered) return;

        if (levelScore < 4)
        {
            if (levelTimer > 0f)
                levelTimer -= Time.deltaTime;
            else
            {
                levelEndTriggered = true;
                ShowMessage("Game Over");
                StartCoroutine(ReloadAfterDelay(GetLevelName(0), messageDuration));
            }
        }
        else
        {
            levelEndTriggered = true;
            StartCoroutine(LoadNextOrMenuAfterDelay(0.25f));
        }
    }

    private void HandleLevel2()
    {
        if (levelEndTriggered) return;

        // criterio: si no hay enemigos con el tag enemyTag, nivel completado
        bool anyEnemy = AnyEnemyPresent();
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        bool playerDead = IsPlayerDead(playerObj);

        if (!anyEnemy)
        {
            levelEndTriggered = true;
            StartCoroutine(LoadNextOrMenuAfterDelay(0.25f));
        }
        else if (playerDead)
        {
            levelEndTriggered = true;
            ShowMessage("Game Over");
            StartCoroutine(ReloadAfterDelay(GetLevelName(0), messageDuration));
        }
    }

    private void HandleGenericLevel()
    {
        if (levelEndTriggered) return;

        bool anyEnemy = AnyEnemyPresent();
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        bool playerDead = IsPlayerDead(playerObj);

        if (!anyEnemy)
        {
            levelEndTriggered = true;
            StartCoroutine(LoadNextOrMenuAfterDelay(0.25f));
        }
        else if (playerDead)
        {
            levelEndTriggered = true;
            ShowMessage("Game Over");
            StartCoroutine(ReloadAfterDelay(GetLevelName(0), messageDuration));
        }
    }

    // -------------------------
    // Utilidades
    // -------------------------
    private bool AnyEnemyPresent()
    {
        if (string.IsNullOrEmpty(enemyTag)) return false;
        var enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        return enemies != null && enemies.Length > 0;
    }

    private bool IsPlayerDead(GameObject playerObj)
    {
        if (playerObj == null) return false;
        // intenta obtener un componente que represente vida
        var mb = playerObj.GetComponent<MonoBehaviour>();
        if (mb == null) return false;

        // Intenta varias formas de leer un valor de vida (campo/propiedad)
        float val;
        if (TryGetHealthValueFromComponent(mb, out val))
        {
            return val <= 0f;
        }

        // fallback: si hay un componente PlayerHealth con currentHealth público
        var ph = playerObj.GetComponent("PlayerHealth") as MonoBehaviour;
        if (ph != null && TryGetHealthValueFromComponent(ph, out val))
            return val <= 0f;

        return false;
    }

    // intenta leer "health", "currentHealth" o "currentHP" desde cualquier componente vía reflexión
    private bool TryGetHealthValueFromComponent(MonoBehaviour component, out float healthOut)
    {
        healthOut = 0f;
        if (component == null) return false;

        var t = component.GetType();

        // buscar campos
        var fieldsToTry = new string[] { "health", "currentHealth", "currentHP", "hp" };
        foreach (var fname in fieldsToTry)
        {
            var f = t.GetField(fname, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (f != null)
            {
                var v = f.GetValue(component);
                if (TryConvertToFloat(v, out healthOut)) return true;
            }
        }

        // buscar propiedades
        foreach (var pname in fieldsToTry)
        {
            var p = t.GetProperty(pname, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p != null && p.CanRead)
            {
                var v = p.GetValue(component, null);
                if (TryConvertToFloat(v, out healthOut)) return true;
            }
        }

        return false;
    }

    private bool TryConvertToFloat(object v, out float f)
    {
        f = 0f;
        if (v == null) return false;
        if (v is float) { f = (float)v; return true; }
        if (v is int) { f = (int)v; return true; }
        if (v is double) { f = (float)(double)v; return true; }
        var s = v as string;
        if (!string.IsNullOrEmpty(s) && float.TryParse(s, out f)) return true;
        return false;
    }

    // -------------------------
    // Mensajes y carga de escenas
    // -------------------------
    private IEnumerator ShowLevelCompletedThenMenu()
    {
        ShowMessage("Nivel completado");
        yield return new WaitForSeconds(messageDuration);
        ShowMainMenu();
    }

    private void ShowMessage(string msg)
    {
        // Auto-localiza el Text si no está asignado
        if (messageText == null)
        {
            var texts = GameObject.FindObjectsOfType<TextMeshProUGUI>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                var t = texts[i];
                if (t != null && t.name.ToLowerInvariant().Contains("message"))
                {
                    messageText = t;
                    break;
                }
            }
        }

        if (messageText != null)
        {
            // Asegura que el Canvas/jerarquía estén activos
            var canvas = messageText.GetComponentInParent<Canvas>(true);
            if (canvas != null) canvas.gameObject.SetActive(true);

            messageText.text = msg;
            messageText.gameObject.SetActive(true);
            StartCoroutine(HideMessageAfterDelay());
        }
        else
        {
            Debug.LogWarning("LevelManager: 'messageText' no está asignado ni pudo autolocalizarse. No se mostrará el mensaje en UI.");
        }
        Debug.Log(msg);
    }

    private string[] GetLevelSequence()
    {
        System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
        if (levelNames != null)
        {
            for (int i = 0; i < levelNames.Length; i++)
            {
                var s = levelNames[i];
                if (!string.IsNullOrEmpty(s)) list.Add(s);
            }
        }
        return list.ToArray();
    }

    private int GetLevelIndex(string sceneName)
    {
        var seq = GetLevelSequence();
        for (int i = 0; i < seq.Length; i++)
        {
            if (seq[i] == sceneName) return i;
        }
        return -1;
    }

    private string GetNextLevelName()
    {
        string current = SceneManager.GetActiveScene().name;
        var seq = GetLevelSequence();
        int idx = GetLevelIndex(current);
        if (idx >= 0 && idx + 1 < seq.Length) return seq[idx + 1];
        return null; // No hay siguiente
    }

    private string GetLevelName(int index)
    {
        var seq = GetLevelSequence();
        if (index >= 0 && index < seq.Length) return seq[index];
        return SceneManager.GetActiveScene().name;
    }

    private IEnumerator LoadNextOrMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        string next = GetNextLevelName();
        if (!string.IsNullOrEmpty(next))
            SceneManager.LoadScene(next);
        else
            ShowMainMenu();
    }

    private IEnumerator ReloadAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator LoadAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }
}
