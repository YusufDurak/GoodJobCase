using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Main board manager handling grid generation, matching, gravity, and shuffle
/// Optimized for performance with object pooling and efficient algorithms
/// </summary>
public class BoardManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField, Range(2, 10)] private int rows = 8;
    [SerializeField, Range(2, 10)] private int columns = 8;
    [SerializeField] private float blockSize = 1f;
    [SerializeField] private float blockSpacing = 0.1f;

    [Header("Block Colors")]
    [SerializeField, Range(1, 6)] private int numberOfColors = 5;
    [SerializeField] private BlockColorData[] blockColors;

    [Header("Dynamic Icon Thresholds")]
    [SerializeField] private int thresholdA = 5;
    [SerializeField] private int thresholdB = 10;
    [SerializeField] private int thresholdC = 15;

    [Header("Animation Settings")]
    [SerializeField] private float fallDuration = 0.3f;
    [SerializeField] private float destroyDelay = 0.1f;

    [Header("Components")]
    [SerializeField] private BlockPool blockPool;
    [SerializeField] private Camera mainCamera;

    // Grid data structure
    private Block[,] grid;
    private bool isProcessing = false;
    
    // Reusable collections to minimize GC
    private List<Block> matchedBlocks = new List<Block>(100);
    private Queue<GridPosition> bfsQueue = new Queue<GridPosition>(100);
    private HashSet<GridPosition> visited = new HashSet<GridPosition>();
    private List<Block> tempBlockList = new List<Block>(100);

    // Cache for group size calculations
    private Dictionary<Block, int> groupSizeCache = new Dictionary<Block, int>(100);

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Initialize pool
        blockPool.Initialize(transform);
        
        // Generate initial board
        GenerateBoard();
        
        // Update all icon states
        UpdateAllBlockIcons();
    }

    /// <summary>
    /// Get the board dimensions in world space (for camera fitting)
    /// </summary>
    public Vector2 GetBoardDimensions()
    {
        float width = columns * (blockSize + blockSpacing) - blockSpacing;
        float height = rows * (blockSize + blockSpacing) - blockSpacing;
        return new Vector2(width, height);
    }

    /// <summary>
    /// Generate the initial game board
    /// </summary>
    private void GenerateBoard()
    {
        // Initialize grid array
        grid = new Block[rows, columns];

        // Calculate board centering
        float totalWidth = columns * (blockSize + blockSpacing) - blockSpacing;
        float totalHeight = rows * (blockSize + blockSpacing) - blockSpacing;
        
        // Fix: Center the grid properly by accounting for block positions
        // Blocks go from index 0 to (columns-1), center should be at the midpoint
        float centerOffsetX = ((columns - 1) * (blockSize + blockSpacing)) / 2f;
        float centerOffsetY = ((rows - 1) * (blockSize + blockSpacing)) / 2f;
        
        Vector3 startPos = new Vector3(-centerOffsetX, -centerOffsetY, 0);

        // Generate blocks
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                CreateBlockAt(row, col, startPos);
            }
        }
    }

    /// <summary>
    /// Create a block at the specified grid position
    /// </summary>
    private void CreateBlockAt(int row, int col, Vector3 startPos)
    {
        Block block = blockPool.GetBlock();
        block.transform.SetParent(transform);

        // Calculate world position
        Vector3 worldPos = startPos + new Vector3(
            col * (blockSize + blockSpacing),
            row * (blockSize + blockSpacing),
            0
        );
        block.transform.localPosition = worldPos;

        // Random color
        int colorID = Random.Range(0, Mathf.Min(numberOfColors, blockColors.Length));
        GridPosition gridPos = new GridPosition(row, col);
        
        block.Initialize(colorID, gridPos, blockColors[colorID]);
        grid[row, col] = block;
    }

    /// <summary>
    /// Handle input for block selection
    /// </summary>
    private void Update()
    {
        if (isProcessing) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Block clickedBlock = GetBlockAtPosition(worldPos);

            if (clickedBlock != null && clickedBlock.IsActive)
            {
                OnBlockClicked(clickedBlock);
            }
        }
    }

    /// <summary>
    /// Get block at world position using raycast
    /// </summary>
    private Block GetBlockAtPosition(Vector2 worldPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            return hit.collider.GetComponent<Block>();
        }
        return null;
    }

    /// <summary>
    /// Handle block click event
    /// </summary>
    private void OnBlockClicked(Block block)
    {
        // Find connected group using flood fill
        matchedBlocks.Clear();
        FloodFill(block.GridPos, block.ColorID, matchedBlocks);

        // Check if group is valid (2 or more)
        if (matchedBlocks.Count >= 2)
        {
            // Play feedback on clicked block
            block.PlayClickFeedback();
            AudioManager.Instance.PlayPop();
            
            // Start destruction sequence
            StartCoroutine(DestroyMatchedBlocksSequence(matchedBlocks));
        }
    }

    /// <summary>
    /// Efficient BFS flood fill algorithm to find connected blocks
    /// </summary>
    private void FloodFill(GridPosition startPos, int targetColor, List<Block> result)
    {
        visited.Clear();
        bfsQueue.Clear();

        bfsQueue.Enqueue(startPos);
        visited.Add(startPos);

        // Directions: up, down, left, right
        int[] dRow = { -1, 1, 0, 0 };
        int[] dCol = { 0, 0, -1, 1 };

        while (bfsQueue.Count > 0)
        {
            GridPosition current = bfsQueue.Dequeue();
            Block currentBlock = grid[current.Row, current.Column];

            if (currentBlock != null && currentBlock.IsActive && currentBlock.ColorID == targetColor)
            {
                result.Add(currentBlock);

                // Check all 4 neighbors
                for (int i = 0; i < 4; i++)
                {
                    int newRow = current.Row + dRow[i];
                    int newCol = current.Column + dCol[i];

                    if (IsValidPosition(newRow, newCol))
                    {
                        GridPosition neighborPos = new GridPosition(newRow, newCol);
                        
                        if (!visited.Contains(neighborPos))
                        {
                            visited.Add(neighborPos);
                            bfsQueue.Enqueue(neighborPos);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Check if position is within grid bounds
    /// </summary>
    private bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < rows && col >= 0 && col < columns;
    }

    /// <summary>
    /// Sequence to destroy matched blocks and handle gravity
    /// </summary>
    private IEnumerator DestroyMatchedBlocksSequence(List<Block> blocksToDestroy)
    {
        isProcessing = true;

        // Wait for destruction delay
        yield return new WaitForSeconds(destroyDelay);

        // Remove blocks from grid and play particle effects
        foreach (Block block in blocksToDestroy)
        {
            // Play explosion particle effect (if VFXManager exists)
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.PlayExplosion(
                    block.transform.position, 
                    block.CurrentColor  // Use cached color instead of GetComponent
                );
            }
    
            grid[block.GridPos.Row, block.GridPos.Column] = null;
        }
        blockPool.ReturnBlocks(blocksToDestroy);

        // Apply gravity and wait for animations
        yield return StartCoroutine(ApplyGravity());

        // Update all block icons after gravity
        UpdateAllBlockIcons();

        // Check for deadlock
        if (IsDeadlocked())
        {
            Debug.Log("Deadlock detected! Shuffling board...");
            yield return StartCoroutine(SmartShuffle());
            UpdateAllBlockIcons();
        }

        isProcessing = false;
    }

    /// <summary>
    /// Apply gravity: make blocks fall and spawn new ones
    /// </summary>
    private IEnumerator ApplyGravity()
    {
        bool anyMovement = false;
        List<Tween> activeTweens = new List<Tween>();

        // Process each column
        for (int col = 0; col < columns; col++)
        {
            int emptyRow = 0;

            // Move existing blocks down
            for (int row = 0; row < rows; row++)
            {
                if (grid[row, col] != null && grid[row, col].IsActive)
                {
                    if (emptyRow != row)
                    {
                        // Move block down
                        Block block = grid[row, col];
                        grid[row, col] = null;
                        grid[emptyRow, col] = block;
                        block.SetGridPosition(new GridPosition(emptyRow, col));

                        // Animate fall
                        Vector3 targetPos = GetWorldPosition(emptyRow, col);
                        Tween tween = block.AnimateFall(targetPos, fallDuration);
                        activeTweens.Add(tween);
                        anyMovement = true;
                    }
                    emptyRow++;
                }
            }

            // Spawn new blocks from top
            for (int row = emptyRow; row < rows; row++)
            {
                Block newBlock = blockPool.GetBlock();
                newBlock.transform.SetParent(transform);

                int colorID = Random.Range(0, Mathf.Min(numberOfColors, blockColors.Length));
                GridPosition gridPos = new GridPosition(row, col);

                // Start position above the grid
                Vector3 startPos = GetWorldPosition(rows, col);
                newBlock.transform.localPosition = startPos;

                newBlock.Initialize(colorID, gridPos, blockColors[colorID]);
                grid[row, col] = newBlock;

                // Animate fall
                Vector3 targetPos = GetWorldPosition(row, col);
                Tween tween = newBlock.AnimateFall(targetPos, fallDuration);
                activeTweens.Add(tween);
                anyMovement = true;
            }
        }

        // Wait for all animations to complete
        if (anyMovement)
        {
            yield return new WaitForSeconds(fallDuration);
        }
    }

    /// <summary>
    /// Get world position for a grid cell
    /// </summary>
    private Vector3 GetWorldPosition(int row, int col)
    {
        // Use the same centering calculation as GenerateBoard()
        float centerOffsetX = ((columns - 1) * (blockSize + blockSpacing)) / 2f;
        float centerOffsetY = ((rows - 1) * (blockSize + blockSpacing)) / 2f;
        
        Vector3 startPos = new Vector3(-centerOffsetX, -centerOffsetY, 0);

        return startPos + new Vector3(
            col * (blockSize + blockSpacing),
            row * (blockSize + blockSpacing),
            0
        );
    }

    /// <summary>
    /// Update all block sprites based on their group sizes
    /// </summary>
    private void UpdateAllBlockIcons()
    {
        groupSizeCache.Clear();

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Block block = grid[row, col];
                if (block != null && block.IsActive)
                {
                    if (!groupSizeCache.ContainsKey(block))
                    {
                        // Calculate group size
                        tempBlockList.Clear();
                        FloodFill(block.GridPos, block.ColorID, tempBlockList);
                        int groupSize = tempBlockList.Count;

                        // Cache for all blocks in group
                        foreach (Block b in tempBlockList)
                        {
                            groupSizeCache[b] = groupSize;
                        }
                    }

                    // Update sprite
                    int size = groupSizeCache[block];
                    block.UpdateSpriteForGroupSize(size, thresholdA, thresholdB, thresholdC);
                }
            }
        }
    }

    /// <summary>
    /// Check if the board is in a deadlock state (no valid moves)
    /// </summary>
    private bool IsDeadlocked()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Block block = grid[row, col];
                if (block != null && block.IsActive)
                {
                    tempBlockList.Clear();
                    FloodFill(block.GridPos, block.ColorID, tempBlockList);
                    if (tempBlockList.Count >= 2)
                    {
                        return false; // Found valid move
                    }
                }
            }
        }
        return true; // No valid moves
    }

    /// <summary>
    /// Smart shuffle algorithm that guarantees at least one valid move
    /// Uses a strategic approach instead of random shuffling
    /// </summary>
    private IEnumerator SmartShuffle()
    {
        // Collect all active blocks
        List<Block> allBlocks = new List<Block>();
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (grid[row, col] != null && grid[row, col].IsActive)
                {
                    allBlocks.Add(grid[row, col]);
                }
            }
        }

        // Strategy: Create guaranteed matches by placing same-colored blocks adjacent
        int attempts = 0;
        const int maxAttempts = 100;
        bool validShuffleFound = false;

        while (!validShuffleFound && attempts < maxAttempts)
        {
            attempts++;

            // Shuffle block positions
            for (int i = allBlocks.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                
                // Swap grid positions
                Block blockA = allBlocks[i];
                Block blockB = allBlocks[j];
                
                GridPosition tempPos = blockA.GridPos;
                blockA.SetGridPosition(blockB.GridPos);
                blockB.SetGridPosition(tempPos);
                
                grid[blockA.GridPos.Row, blockA.GridPos.Column] = blockA;
                grid[blockB.GridPos.Row, blockB.GridPos.Column] = blockB;
            }

            // Force create at least one match by swapping colors
            if (allBlocks.Count >= 2)
            {
                int idx1 = Random.Range(0, allBlocks.Count);
                int idx2 = Random.Range(0, allBlocks.Count);
                
                // Ensure they're adjacent or close
                Block b1 = allBlocks[idx1];
                Block b2 = FindAdjacentBlock(b1);
                
                if (b2 != null)
                {
                    int tempColor = b1.ColorID;
                    b1.Initialize(b2.ColorID, b1.GridPos, blockColors[b2.ColorID]);
                }
            }

            // Verify at least one valid move exists
            if (!IsDeadlocked())
            {
                validShuffleFound = true;
            }
        }

        // Animate blocks to their new positions
        List<Tween> tweens = new List<Tween>();
        foreach (Block block in allBlocks)
        {
            Vector3 targetPos = GetWorldPosition(block.GridPos.Row, block.GridPos.Column);
            Tween tween = block.AnimateFall(targetPos, fallDuration);
            tweens.Add(tween);
        }

        yield return new WaitForSeconds(fallDuration);
    }

    /// <summary>
    /// Find an adjacent block to the given block
    /// </summary>
    private Block FindAdjacentBlock(Block block)
    {
        int[] dRow = { -1, 1, 0, 0 };
        int[] dCol = { 0, 0, -1, 1 };

        for (int i = 0; i < 4; i++)
        {
            int newRow = block.GridPos.Row + dRow[i];
            int newCol = block.GridPos.Column + dCol[i];

            if (IsValidPosition(newRow, newCol))
            {
                Block adjacent = grid[newRow, newCol];
                if (adjacent != null && adjacent.IsActive)
                {
                    return adjacent;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Regenerate entire board (for testing or restart)
    /// </summary>
    public void RegenerateBoard()
    {
        if (isProcessing) return;

        StopAllCoroutines();
        blockPool.ClearAll();
        GenerateBoard();
        UpdateAllBlockIcons();
    }

    private void OnDestroy()
    {
        // Kill all active tweens
        DOTween.KillAll();
    }
}
