using UnityEngine;

/// <summary>
/// ScriptableObject for game configuration
/// Allows creating multiple configurations for different levels or difficulty settings
/// Create via: Assets > Create > Game Config
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Game Config", order = 1)]
public class GameConfig : ScriptableObject
{
    [Header("Grid Configuration")]
    [Range(2, 10)] public int rows = 8;
    [Range(2, 10)] public int columns = 8;
    public float blockSize = 1f;
    public float blockSpacing = 0.1f;

    [Header("Gameplay Settings")]
    [Range(1, 6)] public int numberOfColors = 5;
    [Range(2, 20)] public int minimumMatchSize = 2;
    public bool enableShuffle = true;
    public int maxShuffleAttempts = 100;

    [Header("Dynamic Icon Thresholds")]
    [Tooltip("Group size threshold for Icon 1")]
    public int thresholdA = 5;
    
    [Tooltip("Group size threshold for Icon 2")]
    public int thresholdB = 10;
    
    [Tooltip("Group size threshold for Icon 3")]
    public int thresholdC = 15;

    [Header("Animation Timings")]
    [Range(0.1f, 1f)] public float fallDuration = 0.3f;
    [Range(0f, 0.5f)] public float destroyDelay = 0.1f;
    [Range(0.1f, 0.5f)] public float spawnPunchDuration = 0.3f;
    [Range(0.1f, 0.5f)] public float clickPunchDuration = 0.2f;

    [Header("Animation Scales")]
    [Range(1f, 2f)] public float spawnPunchScale = 1.2f;
    [Range(1f, 2f)] public float clickPunchScale = 1.15f;

    [Header("Scoring (Optional)")]
    public int pointsPerBlock = 10;
    public int comboMultiplier = 50;
    public int[] bonusThresholds = new int[] { 5, 10, 15, 20 };
    public int[] bonusPoints = new int[] { 50, 100, 200, 500 };

    /// <summary>
    /// Validate configuration values
    /// </summary>
    private void OnValidate()
    {
        // Ensure thresholds are in ascending order
        if (thresholdB <= thresholdA)
        {
            thresholdB = thresholdA + 1;
        }
        if (thresholdC <= thresholdB)
        {
            thresholdC = thresholdB + 1;
        }

        // Ensure minimum values
        minimumMatchSize = Mathf.Max(2, minimumMatchSize);
        maxShuffleAttempts = Mathf.Max(1, maxShuffleAttempts);
    }

    /// <summary>
    /// Get bonus points for a given group size
    /// </summary>
    public int GetBonusPoints(int groupSize)
    {
        for (int i = bonusThresholds.Length - 1; i >= 0; i--)
        {
            if (groupSize >= bonusThresholds[i])
            {
                return bonusPoints[i];
            }
        }
        return 0;
    }

    /// <summary>
    /// Check if grid size is valid for the number of colors
    /// </summary>
    public bool IsValidConfiguration()
    {
        int totalCells = rows * columns;
        
        // Need at least 2 cells per color for matches to be possible
        if (totalCells < numberOfColors * 2)
        {
            Debug.LogWarning($"Grid too small ({rows}x{columns}) for {numberOfColors} colors. " +
                           $"Need at least {numberOfColors * 2} cells.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Get recommended pool size based on grid
    /// </summary>
    public int GetRecommendedPoolSize()
    {
        // Pool should be larger than grid to handle spawning
        return Mathf.CeilToInt(rows * columns * 1.5f);
    }
}
