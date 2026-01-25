using UnityEngine;

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
    public int thresholdA = 5;
    public int thresholdB = 10;
    public int thresholdC = 15;

    [Header("Animation Timings")]
    [Range(0.1f, 1f)] public float fallDuration = 0.3f;
    [Range(0f, 0.5f)] public float destroyDelay = 0.1f;
    [Range(0.1f, 0.5f)] public float spawnPunchDuration = 0.3f;
    [Range(0.1f, 0.5f)] public float clickPunchDuration = 0.2f;

    [Header("Animation Scales")]
    [Range(1f, 2f)] public float spawnPunchScale = 1.2f;
    [Range(1f, 2f)] public float clickPunchScale = 1.15f;

    [Header("Scoring")]
    public int pointsPerBlock = 10;
    public int comboMultiplier = 50;
    public int[] bonusThresholds = new int[] { 5, 10, 15, 20 };
    public int[] bonusPoints = new int[] { 50, 100, 200, 500 };

    private void OnValidate()
    {
        if (thresholdB <= thresholdA)
        {
            thresholdB = thresholdA + 1;
        }
        if (thresholdC <= thresholdB)
        {
            thresholdC = thresholdB + 1;
        }

        minimumMatchSize = Mathf.Max(2, minimumMatchSize);
        maxShuffleAttempts = Mathf.Max(1, maxShuffleAttempts);
    }

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

    public bool IsValidConfiguration()
    {
        int totalCells = rows * columns;
        
        if (totalCells < numberOfColors * 2)
        {
            return false;
        }

        return true;
    }

    public int GetRecommendedPoolSize()
    {
        return Mathf.CeilToInt(rows * columns * 1.5f);
    }
}
