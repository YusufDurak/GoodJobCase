using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int movesLeft = 20;
    [SerializeField] private int score = 0;
    [SerializeField] private int targetScore = 500;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Button References")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button restartButton;

    [Header("Level Progression")]
    [SerializeField] private int targetScoreIncrement = 300;

    [Header("Animation Settings")]
    [SerializeField] private float panelAnimDuration = 0.5f;
    [SerializeField] private float buttonAnimDelay = 0.3f;

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

        SetupButtonListeners();
        UpdateUI();
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

        if (score >= targetScore)
        {
            Win();
        }
    }

    public void DecreaseMove()
    {
        if (gameOver) return;

        movesLeft--;
        UpdateUI();
        AnimateMovesText();

        if (movesLeft <= 0 && score < targetScore)
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
            panelSequence.Append(panelRect.DOScale(Vector3.one, panelAnimDuration).SetEase(Ease.OutBack));
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
        movesLeft = 20;
        score = 0;
        targetScore += targetScoreIncrement;
        gameOver = false;

        UpdateUI();

        BoardManager boardManager = FindFirstObjectByType<BoardManager>();
        if (boardManager != null)
        {
            boardManager.RegenerateBoard();
        }
    }

    public void RestartLevel()
    {
        movesLeft = 20;
        score = 0;
        gameOver = false;

        UpdateUI();

        BoardManager boardManager = FindFirstObjectByType<BoardManager>();
        if (boardManager != null)
        {
            boardManager.RegenerateBoard();
        }
    }

    private void AnimateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.transform.DOKill();
            scoreText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
        }
    }

    private void AnimateMovesText()
    {
        if (movesText != null)
        {
            movesText.transform.DOKill();
            movesText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
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
