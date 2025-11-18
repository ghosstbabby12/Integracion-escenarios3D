using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Controla puertas interactivas que permiten avanzar de nivel
/// </summary>
public class DoorController : MonoBehaviour
{
    [Header("Configuración de la Puerta")]
    [Tooltip("Nombre descriptivo de la puerta")]
    public string doorName = "Puerta de Salida";

    [Tooltip("¿La puerta está abierta desde el inicio?")]
    public bool isOpenAtStart = false;

    [Tooltip("¿Se puede abrir manualmente con tecla?")]
    public bool canOpenManually = true;

    [Tooltip("Tecla para abrir la puerta")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Condiciones para Abrir")]
    [Tooltip("Requiere eliminar todos los enemigos?")]
    public bool requireAllEnemiesDead = true;

    [Tooltip("Tag de los enemigos a verificar")]
    public string enemyTag = "Enemy";

    [Tooltip("Requiere una llave específica?")]
    public bool requireKey = false;

    [Tooltip("Nombre de la llave requerida")]
    public string requiredKeyName = "Llave Maestra";

    [Tooltip("Puntuación mínima requerida")]
    public int requiredScore = 0;

    [Header("Escena de Destino")]
    [Tooltip("Nombre de la escena a cargar al atravesar la puerta")]
    public string nextSceneName = "";

    [Tooltip("Usar índice de Build Settings en lugar de nombre")]
    public bool useSceneIndex = false;

    [Tooltip("Índice de la escena en Build Settings")]
    public int nextSceneIndex = 0;

    [Header("Animación y Efectos")]
    [Tooltip("Animator de la puerta (opcional)")]
    public Animator doorAnimator;

    [Tooltip("Nombre del parámetro bool en el Animator para abrir")]
    public string openAnimationParameter = "IsOpen";

    [Tooltip("Velocidad de apertura si no hay Animator (grados por segundo)")]
    public float openSpeed = 90f;

    [Tooltip("Ángulo de apertura (en grados)")]
    public float openAngle = 90f;

    [Tooltip("Audio al abrir la puerta")]
    public AudioClip openSound;

    [Tooltip("Audio al cerrar/bloquear la puerta")]
    public AudioClip lockedSound;

    [Header("UI de Interacción")]
    [Tooltip("Texto UI para mostrar mensajes de interacción")]
    public TextMeshProUGUI interactionText;

    [Tooltip("Distancia máxima de interacción")]
    public float interactionDistance = 10f;

    [Tooltip("Mostrar UI de interacción")]
    public bool showInteractionUI = true;

    [Header("Trigger Zone")]
    [Tooltip("¿Cargar escena automáticamente al atravesar?")]
    public bool autoLoadOnEnter = true;

    [Tooltip("Delay antes de cargar la escena (segundos)")]
    public float loadSceneDelay = 0.5f;

    // Variables privadas
    private bool isOpen = false;
    private bool isOpening = false;
    private bool playerInRange = false;
    private Transform playerTransform;
    private AudioSource audioSource;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool hasInventoryKey = false;

    void Start()
    {
        // Guardar rotación inicial
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));

        // Configurar AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (openSound != null || lockedSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Abrir al inicio si está configurado
        if (isOpenAtStart)
        {
            isOpen = true;
            if (doorAnimator != null)
            {
                doorAnimator.SetBool(openAnimationParameter, true);
            }
            else
            {
                transform.rotation = openRotation;
            }
        }

        // Ocultar texto de interacción
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }

        // Buscar jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            Debug.Log($"✅ DoorController: Jugador encontrado - {player.name}");
        }
        else
        {
            Debug.LogError($"❌ DoorController: No se encontró jugador con tag 'Player'!");
        }
    }

    void Update()
    {
        // Verificar distancia del jugador
        CheckPlayerDistance();

        // Manejar input de interacción
        if (playerInRange && canOpenManually && Input.GetKeyDown(interactKey))
        {
            Debug.Log($"✅ Tecla {interactKey} detectada - Intentando abrir puerta");
            TryOpenDoor();
        }

        // Debug cada 2 segundos si el jugador está cerca
        if (playerInRange && Time.frameCount % 120 == 0)
        {
            Debug.Log($"🚪 Jugador cerca de {doorName} - Presiona [{interactKey}] para interactuar");
        }

        // Actualizar rotación manual si no hay Animator
        if (isOpening && doorAnimator == null)
        {
            UpdateManualRotation();
        }
    }

    void CheckPlayerDistance()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionDistance;

        // Actualizar UI cuando el jugador entra/sale del rango
        if (playerInRange != wasInRange)
        {
            UpdateInteractionUI();
        }
    }

    void UpdateInteractionUI()
    {
        if (interactionText == null || !showInteractionUI) return;

        if (playerInRange && !isOpen)
        {
            string message = GetInteractionMessage();
            interactionText.text = message;
            interactionText.gameObject.SetActive(true);
        }
        else
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    string GetInteractionMessage()
    {
        if (!CanOpenDoor())
        {
            if (requireAllEnemiesDead && !AreAllEnemiesDead())
            {
                return "🔒 ELIMINA TODOS LOS ENEMIGOS";
            }
            else if (requireKey && !hasInventoryKey)
            {
                return $"🔒 NECESITAS: {requiredKeyName}";
            }
            else if (requiredScore > 0 && GetCurrentScore() < requiredScore)
            {
                return $"🔒 PUNTUACIÓN REQUERIDA: {requiredScore}";
            }
            return "🔒 PUERTA BLOQUEADA";
        }

        return $"[{interactKey}] ABRIR {doorName.ToUpper()}";
    }

    public void TryOpenDoor()
    {
        Debug.Log($"🚪 TryOpenDoor llamado - playerInRange: {playerInRange}");

        if (isOpen)
        {
            Debug.Log("La puerta ya está abierta");
            return;
        }

        if (!CanOpenDoor())
        {
            PlaySound(lockedSound);
            ShowMessage("⚠️ " + GetInteractionMessage());
            Debug.LogWarning($"⚠️ Puerta bloqueada: {GetInteractionMessage()}");
            return;
        }

        OpenDoor();
    }

    bool CanOpenDoor()
    {
        // Verificar enemigos
        if (requireAllEnemiesDead && !AreAllEnemiesDead())
        {
            return false;
        }

        // Verificar llave
        if (requireKey && !hasInventoryKey)
        {
            return false;
        }

        // Verificar puntuación
        if (requiredScore > 0 && GetCurrentScore() < requiredScore)
        {
            return false;
        }

        return true;
    }

    bool AreAllEnemiesDead()
    {
        if (string.IsNullOrEmpty(enemyTag)) return true;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        return enemies == null || enemies.Length == 0;
    }

    int GetCurrentScore()
    {
        if (GameManager.instance != null)
        {
            return GameManager.instance.score;
        }

        if (LevelManager.instance != null)
        {
            return LevelManager.instance.levelScore;
        }

        return 0;
    }

    public void OpenDoor()
    {
        if (isOpen) return;

        isOpen = true;
        isOpening = true;

        Debug.Log($"✅ {doorName} abierta!");

        // Reproducir sonido
        PlaySound(openSound);

        // Animar apertura
        if (doorAnimator != null)
        {
            doorAnimator.SetBool(openAnimationParameter, true);
        }

        // Ocultar UI
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }

        ShowMessage($"✅ {doorName} ABIERTA");
    }

    void UpdateManualRotation()
    {
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            openRotation,
            openSpeed * Time.deltaTime
        );

        if (Quaternion.Angle(transform.rotation, openRotation) < 0.1f)
        {
            transform.rotation = openRotation;
            isOpening = false;
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void ShowMessage(string message)
    {
        Debug.Log(message);

        // Intentar mostrar en LevelManager
        if (LevelManager.instance != null && LevelManager.instance.messageText != null)
        {
            LevelManager.instance.messageText.text = message;
            LevelManager.instance.messageText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Método público para dar una llave al jugador
    /// </summary>
    public void GiveKey(string keyName)
    {
        if (keyName == requiredKeyName)
        {
            hasInventoryKey = true;
            Debug.Log($"🔑 Llave obtenida: {keyName}");
            ShowMessage($"🔑 {keyName} OBTENIDA");
        }
    }

    // Trigger para detectar cuando el jugador atraviesa la puerta
    void OnTriggerEnter(Collider other)
    {
        if (!isOpen || !autoLoadOnEnter) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log($"Jugador atravesó {doorName} - Cargando siguiente nivel...");
            StartCoroutine(LoadNextSceneCoroutine());
        }
    }

    IEnumerator LoadNextSceneCoroutine()
    {
        yield return new WaitForSeconds(loadSceneDelay);

        if (useSceneIndex)
        {
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.LogError($"❌ Índice de escena inválido: {nextSceneIndex}");
            }
        }
        else if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("⚠️ No hay escena de destino configurada");
        }
    }

    // Visualización en editor
    void OnDrawGizmosSelected()
    {
        // Dibujar radio de interacción
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);

        // Dibujar dirección de apertura
        Gizmos.color = Color.green;
        Vector3 direction = Quaternion.Euler(0, openAngle, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, direction * 2f);
    }
}
