using UnityEngine;
using TMPro;

/// <summary>
/// Performance monitoring tool for debugging
/// Shows FPS, memory usage, and object counts
/// </summary>
public class PerformanceMonitor : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private bool showStats = true;
    [SerializeField] private float updateInterval = 0.5f;
    
    private float deltaTime = 0.0f;
    private float timer = 0.0f;
    private int frameCount = 0;
    private float fps = 0.0f;

    private void Update()
    {
        if (!showStats || statsText == null) return;

        // Calculate FPS
        frameCount++;
        deltaTime += Time.unscaledDeltaTime;
        timer += Time.unscaledDeltaTime;

        if (timer >= updateInterval)
        {
            fps = frameCount / deltaTime;
            frameCount = 0;
            deltaTime = 0.0f;
            timer = 0.0f;

            UpdateStatsDisplay();
        }
    }

    private void UpdateStatsDisplay()
    {
        // Memory info
        long totalMemory = System.GC.GetTotalMemory(false);
        float memoryMB = totalMemory / (1024f * 1024f);

        // Object counts
        
        int totalObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;
        int activeBlocks = FindObjectsByType<Block>(FindObjectsSortMode.None).Length;

        // Build stats string
        string stats = $"FPS: {fps:F1}\n";
        stats += $"Memory: {memoryMB:F2} MB\n";
        stats += $"Objects: {totalObjects}\n";
        stats += $"Active Blocks: {activeBlocks}";

        statsText.text = stats;

        // Color code FPS
        if (fps >= 55)
            statsText.color = Color.green;
        else if (fps >= 30)
            statsText.color = Color.yellow;
        else
            statsText.color = Color.red;
    }

    /// <summary>
    /// Toggle stats display with F1 key
    /// </summary>
    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showStats = !showStats;
            if (statsText != null)
            {
                statsText.gameObject.SetActive(showStats);
            }
        }
    }
}
