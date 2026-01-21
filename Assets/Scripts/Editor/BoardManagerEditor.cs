using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom editor for BoardManager with validation and testing tools
/// </summary>
[CustomEditor(typeof(BoardManager))]
public class BoardManagerEditor : Editor
{
    private BoardManager boardManager;

    private void OnEnable()
    {
        boardManager = (BoardManager)target;
    }

    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Testing Tools", EditorStyles.boldLabel);

        // Regenerate button
        if (GUILayout.Button("Regenerate Board", GUILayout.Height(30)))
        {
            if (Application.isPlaying)
            {
                boardManager.RegenerateBoard();
            }
            else
            {
                EditorUtility.DisplayDialog("Not Playing", 
                    "Enter Play mode to regenerate the board.", "OK");
            }
        }

        EditorGUILayout.Space(5);

        // Validation section
        EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
        ValidateSetup();

        EditorGUILayout.Space(5);

        // Performance hints
        EditorGUILayout.LabelField("Performance Hints", EditorStyles.boldLabel);
        ShowPerformanceHints();
    }

    private void ValidateSetup()
    {
        SerializedProperty blockColorsProperty = serializedObject.FindProperty("blockColors");
        SerializedProperty numberOfColorsProperty = serializedObject.FindProperty("numberOfColors");
        SerializedProperty blockPoolProperty = serializedObject.FindProperty("blockPool");
        SerializedProperty rowsProperty = serializedObject.FindProperty("rows");
        SerializedProperty columnsProperty = serializedObject.FindProperty("columns");

        bool hasErrors = false;

        // Check block pool
        if (blockPoolProperty.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("⚠️ Block Pool is not assigned!", MessageType.Error);
            hasErrors = true;
        }

        // Check block colors array
        if (blockColorsProperty.arraySize == 0)
        {
            EditorGUILayout.HelpBox("⚠️ Block Colors array is empty!", MessageType.Error);
            hasErrors = true;
        }
        else if (blockColorsProperty.arraySize < numberOfColorsProperty.intValue)
        {
            EditorGUILayout.HelpBox($"⚠️ Block Colors array size ({blockColorsProperty.arraySize}) " +
                $"is less than Number Of Colors ({numberOfColorsProperty.intValue})", MessageType.Warning);
            hasErrors = true;
        }

        // Validate each color entry
        for (int i = 0; i < Mathf.Min(blockColorsProperty.arraySize, numberOfColorsProperty.intValue); i++)
        {
            SerializedProperty colorData = blockColorsProperty.GetArrayElementAtIndex(i);
            SerializedProperty defaultSprite = colorData.FindPropertyRelative("DefaultSprite");

            if (defaultSprite.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox($"⚠️ Color {i}: Default Sprite is not assigned!", MessageType.Warning);
                hasErrors = true;
            }
        }

        // Check grid size vs colors
        int totalCells = rowsProperty.intValue * columnsProperty.intValue;
        int minCells = numberOfColorsProperty.intValue * 2;
        if (totalCells < minCells)
        {
            EditorGUILayout.HelpBox($"⚠️ Grid size ({rowsProperty.intValue}×{columnsProperty.intValue} = {totalCells} cells) " +
                $"may be too small for {numberOfColorsProperty.intValue} colors. " +
                $"Recommended: At least {minCells} cells.", MessageType.Warning);
        }

        // Check thresholds
        SerializedProperty thresholdA = serializedObject.FindProperty("thresholdA");
        SerializedProperty thresholdB = serializedObject.FindProperty("thresholdB");
        SerializedProperty thresholdC = serializedObject.FindProperty("thresholdC");

        if (thresholdB.intValue <= thresholdA.intValue)
        {
            EditorGUILayout.HelpBox($"⚠️ Threshold B ({thresholdB.intValue}) should be greater than A ({thresholdA.intValue})", 
                MessageType.Warning);
        }

        if (thresholdC.intValue <= thresholdB.intValue)
        {
            EditorGUILayout.HelpBox($"⚠️ Threshold C ({thresholdC.intValue}) should be greater than B ({thresholdB.intValue})", 
                MessageType.Warning);
        }

        // Success message
        if (!hasErrors)
        {
            EditorGUILayout.HelpBox("✓ Configuration looks good!", MessageType.Info);
        }
    }

    private void ShowPerformanceHints()
    {
        SerializedProperty rows = serializedObject.FindProperty("rows");
        SerializedProperty columns = serializedObject.FindProperty("columns");
        SerializedProperty colors = serializedObject.FindProperty("numberOfColors");

        int gridSize = rows.intValue * columns.intValue;
        int recommendedPoolSize = Mathf.CeilToInt(gridSize * 1.5f);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField($"Grid Size: {rows.intValue}×{columns.intValue} = {gridSize} cells");
        EditorGUILayout.LabelField($"Recommended Pool Size: {recommendedPoolSize}");
        
        // Performance rating
        string rating = "Good";
        Color ratingColor = Color.green;
        
        if (gridSize > 80)
        {
            rating = "May Impact Performance";
            ratingColor = Color.yellow;
        }
        else if (gridSize > 100)
        {
            rating = "Performance Warning";
            ratingColor = Color.red;
        }

        GUI.color = ratingColor;
        EditorGUILayout.LabelField($"Performance: {rating}");
        GUI.color = Color.white;

        EditorGUILayout.EndVertical();

        // Memory estimate
        float estimatedMemory = gridSize * 0.5f; // ~0.5KB per block
        EditorGUILayout.LabelField($"Estimated Memory: ~{estimatedMemory:F1} KB");
    }
}
