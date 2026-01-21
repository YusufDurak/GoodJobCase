using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Optional UI manager for testing and gameplay feedback
/// Displays score, moves, and provides restart button
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private Button restartButton;
    
    [Header("References")]
    [SerializeField] private BoardManager boardManager;
    
    [Header("Scoring")]
    [SerializeField] private int pointsPerBlock = 10;
    [SerializeField] private int comboMultiplier = 50;
    
    private int currentScore = 0;
    private int moveCount = 0;
    private int currentCombo = 0;

    private void Start()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }
        
        UpdateUI();
    }

    /// <summary>
    /// Call this when blocks are destroyed
    /// </summary>
    public void OnBlocksDestroyed(int blockCount)
    {
        moveCount++;
        currentCombo++;
        
        // Calculate score: base points + combo bonus
        int baseScore = blockCount * pointsPerBlock;
        int comboBonus = currentCombo * comboMultiplier;
        int totalPoints = baseScore + comboBonus;
        
        currentScore += totalPoints;
        UpdateUI();
        
        // Show combo feedback
        if (currentCombo > 1 && comboText != null)
        {
            comboText.text = $"COMBO x{currentCombo}!";
            comboText.gameObject.SetActive(true);
            Invoke(nameof(HideComboText), 1f);
        }
    }

    /// <summary>
    /// Reset combo when no cascades happen
    /// </summary>
    public void ResetCombo()
    {
        currentCombo = 0;
    }

    private void HideComboText()
    {
        if (comboText != null)
        {
            comboText.gameObject.SetActive(false);
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
        
        if (movesText != null)
        {
            movesText.text = $"Moves: {moveCount}";
        }
    }

    private void OnRestartClicked()
    {
        currentScore = 0;
        moveCount = 0;
        currentCombo = 0;
        
        if (boardManager != null)
        {
            boardManager.RegenerateBoard();
        }
        
        UpdateUI();
    }
}
