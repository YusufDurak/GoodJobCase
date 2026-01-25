using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameConfig))]
public class GameConfigValidator : Editor
{
    public override void OnInspectorGUI()
    {
        GameConfig config = (GameConfig)target;

        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Configuration Analysis", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        bool isValid = config.IsValidConfiguration();
        if (isValid)
        {
            EditorGUILayout.HelpBox("✓ Configuration is valid", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("⚠️ Configuration may cause issues", MessageType.Warning);
        }

        int totalCells = config.rows * config.columns;
        int recommendedPoolSize = config.GetRecommendedPoolSize();

        EditorGUILayout.LabelField("Grid Analysis:");
        EditorGUILayout.LabelField($"  Total Cells: {totalCells}");
        EditorGUILayout.LabelField($"  Colors: {config.numberOfColors}");
        EditorGUILayout.LabelField($"  Cells per Color: {totalCells / (float)config.numberOfColors:F1}");
        EditorGUILayout.LabelField($"  Recommended Pool: {recommendedPoolSize}");

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Threshold Configuration:");
        EditorGUILayout.LabelField($"  A (Icon 1): {config.thresholdA}+");
        EditorGUILayout.LabelField($"  B (Icon 2): {config.thresholdB}+");
        EditorGUILayout.LabelField($"  C (Icon 3): {config.thresholdC}+");

        int maxPossibleGroup = totalCells;
        if (config.thresholdC > maxPossibleGroup)
        {
            EditorGUILayout.HelpBox($"⚠️ Threshold C ({config.thresholdC}) is greater than " +
                $"max possible group size ({maxPossibleGroup})", MessageType.Warning);
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Performance Rating:");

        string rating = GetPerformanceRating(config);
        Color originalColor = GUI.color;
        
        if (rating.Contains("Excellent"))
            GUI.color = Color.green;
        else if (rating.Contains("Good"))
            GUI.color = Color.yellow;
        else
            GUI.color = Color.red;

        EditorGUILayout.LabelField($"  {rating}");
        GUI.color = originalColor;

        EditorGUILayout.EndVertical();
    }

    private string GetPerformanceRating(GameConfig config)
    {
        int totalCells = config.rows * config.columns;

        if (totalCells <= 36)
            return "Excellent (6×6 or smaller)";
        else if (totalCells <= 64)
            return "Good (8×8 or smaller)";
        else if (totalCells <= 81)
            return "Fair (9×9 or smaller)";
        else
            return "May Impact Performance (10×10)";
    }
}
