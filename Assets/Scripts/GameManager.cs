using UnityEngine;
using TMPro;

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

    [Header("Level Progression")]
    [SerializeField] private int targetScoreIncrement = 300;

    private bool gameOver = false;

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
    }

    private void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        UpdateUI();
    }

    public void AddScore(int amount)
    {
        if (gameOver) return;

        score += amount;
        UpdateUI();

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
            winPanel.SetActive(true);
        }
    }

    private void Lose()
    {
        gameOver = true;
        if (losePanel != null)
        {
            losePanel.SetActive(true);
        }
    }

    public void LoadNextLevel()
    {
        currentLevel++;
        movesLeft = 20;
        score = 0;
        targetScore += targetScoreIncrement;
        gameOver = false;

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

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

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        UpdateUI();

        BoardManager boardManager = FindFirstObjectByType<BoardManager>();
        if (boardManager != null)
        {
            boardManager.RegenerateBoard();
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
}
