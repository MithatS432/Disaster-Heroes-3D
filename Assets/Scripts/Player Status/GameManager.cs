using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public System.Action OnEarthquakeStart;

    public Image healthBar;
    private float maxHealth = 100f;
    private float currentHealth;

    public TextMeshProUGUI pointText;
    private int points = 0;
    private int collectiblesCollected = 0;
    public bool allCollectiblesCollected { get; private set; }
    public GameObject collectibleCompleteEffect;
    public AudioSource audioSource;
    [SerializeField] private AudioClip collectibleCompleteSound;

    public TextMeshProUGUI timeText;
    [SerializeField] private float timeLimit = 900f;

    public TextMeshProUGUI gameExplainText;
    public TextMeshProUGUI quizStartText;
    public TextMeshProUGUI earthquakeStartText;


    [SerializeField] private float earthquakeStart = 10f;
    [SerializeField] private float earthquakeDuration = 60f;
    private bool isPlayerInSafeZone = false;
    [SerializeField] private float earthquakeDamageInterval = 1f;
    [SerializeField] private float earthquakeDamage = 5f;
    private bool isearthquakeActive = false;


    public GameObject tutorialPanel;
    public bool isGameStarted = false;
    private int tutorialIndex = 0;
    private bool tutorialActive = true;
    private Coroutine tutorialRoutine;
    private Coroutine typingRoutine;
    private string[] tutorialSteps =
{
    "Hoş geldin simülasyona.\n\nBaşlamak için hazırlanıyorsun...",

    "Öncelikle 6 adet eşya toplaman gerekiyor.",

    "Ve bu eşyaları topladıktan sonra odaları gezip bazı sorularla karşılaşacaksın.\n\nSoruları doğru cevaplayarak puan kazanabilirsin.",

    "Soruları cevaplarken dikkatli olmalısın, yanlış cevaplar canını ve puanını azaltır.",

    "Soruları cevapladıktan 60 saniye sonra deprem başlayacak.\n\nDoğru tekniği uygulamalısın.",

    "En uygun yere geçince 112'yi arayarak yardım isteyebilirsin ve bu çok önemli çünkü deprem sırasında yardım gelmesi zaman alır.",

    "Sol üst köşede can ve puanın gösterilir.\nEşya topladıkça puan kazanırsın.",

    "Üst kısımda zaman sayacı bulunur ve birazdan başlayacaktır.",

    "Zaman dolmadan tüm eşyaları toplamalısın.",

    "Kapıları açmak ve eşyaları almak için E tuşunu kullan.",

    "Nesnelere yaklaştığında ortadaki imleç yeşil renk olur ve etkileşim mümkün olur.",

    "Tab tuşu ile panelleri açıp kapatabilirsin.",

    "Sol alt köşede bulunduğun oda konumu gösterilir.",

    "Odaları iyice inceleyin ve saklanma tekniğini öğrenin.\n\nDeprem sırasında doğru tekniği uygulamalısın.",

    "Rastgele toplamda 4 farklı bir saklanma tekniği vardır.\n\nDoğru tekniği bulup oraya geçip deprem boyunca çıkmamalısın.",

    "Hazırsan simülasyon başlayacak..."
};

    [Header("Game Over")]
    [SerializeField] private GameObject healthPanel;
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private AudioClip loseSound;
    [SerializeField] private AudioClip damageWarningSound;
    private bool isGameOver = false;


    [Header("Results Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultPointText;
    [SerializeField] private TextMeshProUGUI resultHealthText;
    [SerializeField] private AudioClip completeSimulationSound;



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.fillAmount = currentHealth / maxHealth;

        UpdateHealthBar();
        UpdateUI();

        isGameStarted = false;
        tutorialActive = true;

        tutorialRoutine = StartCoroutine(TutorialFlow());

        if (QuizManager.Instance != null)
        {
            QuizManager.Instance.OnAllQuizzesCompleted -= StartEarthquakeSequence;
            QuizManager.Instance.OnAllQuizzesCompleted += StartEarthquakeSequence;
        }
    }


    #region Singleton and Scene Management
    void Update()
    {
        if (!isGameStarted) return;

        timeLimit = Mathf.Max(0, timeLimit - Time.deltaTime);

        UpdateTimerUI();
    }
    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timeLimit / 60f);
        int seconds = Mathf.FloorToInt(timeLimit % 60f);

        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (timeLimit <= 0f && !isGameOver)
        {
            StartCoroutine(GameOverSequence());
        }
    }
    private IEnumerator GameOverSequence()
    {
        isGameOver = true;
        isGameStarted = false;

        Time.timeScale = 0f;

        if (audioSource != null && loseSound != null)
            audioSource.PlayOneShot(loseSound);

        if (fadeCanvas != null)
        {
            float t = 0f;

            if (fadeCanvas != null)
            {
                fadeCanvas.gameObject.SetActive(true);
                fadeCanvas.alpha = 0f;
            }

            while (t < 1f)
            {
                t += Time.unscaledDeltaTime * 0.8f;
                fadeCanvas.alpha = t;
                yield return null;
            }
        }

        yield return new WaitForSecondsRealtime(2f);

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindSceneReferences();
        ResetGameState();
    }
    private void BindSceneReferences()
    {
        healthBar = GameObject.Find("HealthBar")?.GetComponent<Image>();
        pointText = GameObject.Find("PointText")?.GetComponent<TextMeshProUGUI>();
        timeText = GameObject.Find("TimeText")?.GetComponent<TextMeshProUGUI>();
        gameExplainText = GameObject.Find("ExplainText")?.GetComponent<TextMeshProUGUI>();
        tutorialPanel = GameObject.Find("TutorialPanel");
    }
    private void ResetGameState()
    {
        if (tutorialRoutine != null)
        {
            StopCoroutine(tutorialRoutine);
        }

        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
        }

        isGameStarted = false;
        isGameOver = false;
        tutorialIndex = 0;
        tutorialActive = true;

        points = 0;
        collectiblesCollected = 0;
        allCollectiblesCollected = false;
        currentHealth = maxHealth;

        timeLimit = 1200f;

        UpdateHealthBar();
        UpdateUI();

        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 0f;
            fadeCanvas.gameObject.SetActive(false);
        }

        tutorialRoutine = StartCoroutine(TutorialFlow());
    }
    #endregion





    #region Tutorial Management
    private IEnumerator TutorialFlow()
    {
        tutorialIndex = 0;

        while (tutorialIndex < tutorialSteps.Length)
        {
            string step = tutorialSteps[tutorialIndex];

            if (typingRoutine != null)
                StopCoroutine(typingRoutine);

            yield return typingRoutine = StartCoroutine(TypeText(step));

            yield return new WaitForSeconds(2f);

            tutorialIndex++;
        }

        EndTutorial();
    }
    private IEnumerator TypeText(string fullText, float charDelay = 0.05f)
    {
        gameExplainText.text = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            gameExplainText.text = fullText.Substring(0, i + 1);
            yield return new WaitForSeconds(charDelay);
        }
    }
    private void EndTutorial()
    {
        tutorialActive = false;
        isGameStarted = true;

        gameExplainText.gameObject.SetActive(false);
        tutorialPanel.SetActive(false);

        if (tutorialRoutine != null)
        {
            StopCoroutine(tutorialRoutine);
            tutorialRoutine = null;
        }
    }
    #endregion







    #region Score and Health Management
    public void GetScore(int collectibleScore, int collectibles)
    {
        points += collectibleScore;
        collectiblesCollected += collectibles;
        Invoke("UpdateUI", 2f);
        if (collectiblesCollected == 6 && !allCollectiblesCollected)
        {
            allCollectiblesCollected = true;
            Invoke(nameof(CollectibleCompleteEffect), 3f);
        }
    }
    public void GetQuestionScore(int questionScore)
    {
        points += questionScore;
        points = Mathf.Max(0, points);
        UpdateUI();
    }
    private void CollectibleCompleteEffect()
    {
        if (quizStartText != null)
        {
            quizStartText.gameObject.SetActive(true);
            Invoke(nameof(HideQuizText), 10f);
        }
        audioSource.PlayOneShot(collectibleCompleteSound);
        Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 2f;
        Quaternion rot = Quaternion.LookRotation(Camera.main.transform.forward);

        GameObject completeEffect = Instantiate(collectibleCompleteEffect, spawnPos, rot);
        Destroy(completeEffect, 4f);
    }
    private void HideQuizText()
    {
        if (quizStartText != null)
            quizStartText.gameObject.SetActive(false);
    }
    private void UpdateUI()
    {
        pointText.text = $"Toplam Puan: {points}";
    }

    public void GetDamage(float damage)
    {
        currentHealth -= damage;
        UpdateHealthBar();
    }
    private void UpdateHealthBar()
    {
        if (healthBar != null)
            healthBar.fillAmount = currentHealth / maxHealth;

        if (currentHealth <= 0f && !isGameOver)
        {
            StartCoroutine(GameOverSequence());
        }
    }
    #endregion





    #region Earthquake Management
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (QuizManager.Instance != null)
            QuizManager.Instance.OnAllQuizzesCompleted -= StartEarthquakeSequence;

        StopAllCoroutines();
    }

    public void StartEarthquakeSequence()
    {
        Debug.Log("Earthquake sequence triggered");
        StartCoroutine(EarthquakeDelay());
    }

    private IEnumerator EarthquakeDelay()
    {
        earthquakeStartText.gameObject.SetActive(true);

        yield return new WaitForSeconds(earthquakeStart);

        earthquakeStartText.gameObject.SetActive(false);

        yield return new WaitForSeconds(earthquakeDuration);

        OnEarthquakeStart?.Invoke();

        isearthquakeActive = true;

        StartCoroutine(EarthquakeDamageLoop());

        yield return new WaitForSeconds(10f);

        isearthquakeActive = false;

        ShowResultPanel();
    }
    private IEnumerator EarthquakeDamageLoop()
    {
        while (!isGameOver && isearthquakeActive)
        {
            yield return new WaitForSeconds(earthquakeDamageInterval);

            if (!isPlayerInSafeZone)
            {
                GetDamage(earthquakeDamage);

                TriggerWarningPulse();
            }
            else
            {
                if (healthPanel != null)
                    healthPanel.SetActive(false);
            }
        }
    }
    private void TriggerWarningPulse()
    {
        if (healthPanel != null)
        {
            StartCoroutine(FlashOnce());
        }

        if (audioSource != null && damageWarningSound != null)
        {
            audioSource.PlayOneShot(damageWarningSound);
        }
    }
    private IEnumerator FlashOnce()
    {
        if (healthPanel == null) yield break;

        healthPanel.SetActive(true);

        yield return new WaitForSeconds(0.15f);

        if (healthPanel != null)
            healthPanel.SetActive(false);
    }
    public void SetPlayerInSafeZone(bool value)
    {
        isPlayerInSafeZone = value;
    }
    private void ShowResultPanel()
    {
        if (resultPanel == null)
            return;

        resultPanel.SetActive(true);

        audioSource.PlayOneShot(completeSimulationSound);

        resultPointText.text =
            $"Toplam Puan: {points}";

        resultHealthText.text =
            $"Kalan Can: {Mathf.RoundToInt(currentHealth)}";
    }
    #endregion
}