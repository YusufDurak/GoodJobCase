using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

[System.Serializable]
public class LevelConfig
{
    public int rows;
    public int columns;
    public int colorCount;
    public int targetScore;
    public int moves;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private int currentLevel = 1;
    private int movesLeft;
    private int score;
    private int targetScore;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Button References")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button restartButton;

    [Header("References")]
    [SerializeField] private ResponsiveCameraController cameraController;

    [Header("Level Progression")]
    [SerializeField] private int maxLevel = 4;
    [SerializeField] private LevelConfig[] levelConfigs = new LevelConfig[]
    {
        new LevelConfig { rows = 6, columns = 6, colorCount = 4, targetScore = 500, moves = 25 },
        new LevelConfig { rows = 8, columns = 8, colorCount = 5, targetScore = 1500, moves = 30 },
        new LevelConfig { rows = 9, columns = 7, colorCount = 5, targetScore = 2000, moves = 35 },
        new LevelConfig { rows = 9, columns = 9, colorCount = 6, targetScore = 3000, moves = 40 }
    };

    [Header("Animation Settings")]
    [SerializeField] private float panelAnimDuration = 0.5f;
    [SerializeField] private float buttonAnimDelay = 0.3f;
    [SerializeField] private float panelTargetScale = 1.5f;

    private bool gameOver = false;
    private RectTransform winPanelRect;
    private RectTransform losePanelRect;
    private CanvasGroup winPanelCanvasGroup;
    private CanvasGroup losePanelCanvasGroup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializePanelComponents();
    }

    private void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        InitializeLevel();
        SetupButtonListeners();
        UpdateUI();
    }

    private void InitializeLevel()
    {
        ApplyLevelConfig(currentLevel);
    }

    private void ApplyLevelConfig(int level)
    {
        if (levelConfigs == null || levelConfigs.Length == 0)
        {
            Debug.LogError("No level configurations defined!");
            return;
        }

        int configIndex = Mathf.Clamp(level - 1, 0, levelConfigs.Length - 1);
        LevelConfig config = levelConfigs[configIndex];

        movesLeft = config.moves;
        targetScore = config.targetScore;
        score = 0;

        BoardManager boardManager = FindFirstObjectByType<BoardManager>();
        if (boardManager != null)
        {
            boardManager.ConfigureBoard(config.rows, config.columns, config.colorCount);
        }
    }

    private void InitializePanelComponents()
    {
        if (winPanel != null)
        {
            winPanelRect = winPanel.GetComponent<RectTransform>();
            winPanelCanvasGroup = winPanel.GetComponent<CanvasGroup>();
            if (winPanelCanvasGroup == null)
            {
                winPanelCanvasGroup = winPanel.AddComponent<CanvasGroup>();
            }
        }

        if (losePanel != null)
        {
            losePanelRect = losePanel.GetComponent<RectTransform>();
            losePanelCanvasGroup = losePanel.GetComponent<CanvasGroup>();
            if (losePanelCanvasGroup == null)
            {
                losePanelCanvasGroup = losePanel.AddComponent<CanvasGroup>();
            }
        }
    }

    private void SetupButtonListeners()
    {
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            AddButtonHoverEffect(nextLevelButton);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
            AddButtonHoverEffect(restartButton);
        }
    }

    private void AddButtonHoverEffect(Button button)
    {
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        
        button.onClick.AddListener(() => {
            buttonRect.DOKill();
            buttonRect.DOPunchScale(Vector3.one * 0.1f, 0.2f, 5, 0.5f);
        });
    }

    public void AddScore(int amount)
    {
        if (gameOver) return;

        score += amount;
        UpdateUI();
        AnimateScoreText();

        CheckWinLoseConditions();
    }

    public void DecreaseMove()
    {
        if (gameOver) return;

        movesLeft--;
        UpdateUI();
    }

    public void CheckWinLoseConditions()
    {
        if (gameOver) return;

        if (score >= targetScore)
        {
            Win();
        }
        else if (movesLeft <= 0)
        {
            Lose();
        }
    }

    private void Win()
    {
        gameOver = true;
        if (winPanel != null)
        {
            ShowPanel(winPanel, winPanelRect, winPanelCanvasGroup, nextLevelButton);
        }
    }

    private void Lose()
    {
        gameOver = true;
        if (losePanel != null)
        {
            ShowPanel(losePanel, losePanelRect, losePanelCanvasGroup, restartButton);
        }
    }

    private void ShowPanel(GameObject panel, RectTransform panelRect, CanvasGroup canvasGroup, Button button)
    {
        panel.SetActive(true);

        if (panelRect != null && canvasGroup != null)
        {
            panelRect.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;

            Sequence panelSequence = DOTween.Sequence();
            panelSequence.Append(panelRect.DOScale(Vector3.one * panelTargetScale, panelAnimDuration).SetEase(Ease.OutBack));
            panelSequence.Join(canvasGroup.DOFade(1f, panelAnimDuration * 0.5f));

            if (button != null)
            {
                RectTransform buttonRect = button.GetComponent<RectTransform>();
                buttonRect.localScale = Vector3.zero;
                panelSequence.Append(buttonRect.DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(buttonAnimDelay));
            }
        }
    }

    private void HidePanel(GameObject panel, RectTransform panelRect, CanvasGroup canvasGroup)
    {
        if (panelRect != null && canvasGroup != null)
        {
            Sequence hideSequence = DOTween.Sequence();
            hideSequence.Append(panelRect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));
            hideSequence.Join(canvasGroup.DOFade(0f, 0.2f));
            hideSequence.OnComplete(() => panel.SetActive(false));
        }
        else
        {
            panel.SetActive(false);
        }
    }

    private void OnNextLevelClicked()
    {
        if (winPanel != null)
        {
            HidePanel(winPanel, winPanelRect, winPanelCanvasGroup);
        }

        DOVirtual.DelayedCall(0.4f, () => LoadNextLevel());
    }

    private void OnRestartClicked()
    {
        if (losePanel != null)
        {
            HidePanel(losePanel, losePanelRect, losePanelCanvasGroup);
        }

        DOVirtual.DelayedCall(0.4f, () => RestartLevel());
    }

    public void LoadNextLevel()
    {
        currentLevel++;
        
        if (currentLevel > maxLevel)
        {
            currentLevel = 1;
        }
        
        gameOver = false;
        ApplyLevelConfig(currentLevel);
        UpdateUI();

        BoardManager boardManager = FindFirstObjectByType<BoardManager>();
        if (boardManager != null)
        {
            boardManager.RegenerateBoard();
            RefreshCameraForNewBoard();
        }
    }

    public void RestartLevel()
    {
        gameOver = false;
        ApplyLevelConfig(currentLevel);
        UpdateUI();

        BoardManager boardManager = FindFirstObjectByType<BoardManager>();
        if (boardManager != null)
        {
            boardManager.RegenerateBoard();
            RefreshCameraForNewBoard();
        }
    }

    private void RefreshCameraForNewBoard()
    {
        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<ResponsiveCameraController>();
        }
        
        if (cameraController != null)
        {
            cameraController.RefreshCamera();
        }
    }

    private void AnimateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.transform.DOKill();
            scoreText.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 3, 0.5f);
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}/{targetScore}";
        }

        if (movesText != null)
        {
            movesText.text = $"Moves: {movesLeft}";
        }

        if (levelText != null)
        {
            levelText.text = $"Level {currentLevel}";
        }
    }

    public bool IsGameOver()
    {
        return gameOver;
    }

    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
