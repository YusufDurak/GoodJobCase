using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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

    private Block[,] grid;
    private bool isProcessing = false;
    
    private List<Block> matchedBlocks = new List<Block>(100);
    private Queue<GridPosition> bfsQueue = new Queue<GridPosition>(100);
    private HashSet<GridPosition> visited = new HashSet<GridPosition>();
    private List<Block> tempBlockList = new List<Block>(100);
    private Dictionary<Block, int> groupSizeCache = new Dictionary<Block, int>(100);

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        blockPool.Initialize(transform);
        GenerateBoard();
        UpdateAllBlockIcons();
    }

    public Vector2 GetBoardDimensions()
    {
        float width = columns * (blockSize + blockSpacing) - blockSpacing;
        float height = rows * (blockSize + blockSpacing) - blockSpacing;
        return new Vector2(width, height);
    }

    private void GenerateBoard()
    {
        grid = new Block[rows, columns];

        float totalWidth = columns * (blockSize + blockSpacing) - blockSpacing;
        float totalHeight = rows * (blockSize + blockSpacing) - blockSpacing;
        
        float centerOffsetX = ((columns - 1) * (blockSize + blockSpacing)) / 2f;
        float centerOffsetY = ((rows - 1) * (blockSize + blockSpacing)) / 2f;
        
        Vector3 startPos = new Vector3(-centerOffsetX, -centerOffsetY, 0);

        ResizeBoardFrame(totalWidth, totalHeight);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                CreateBlockAt(row, col, startPos);
            }
        }
    }

    private void ResizeBoardFrame(float gridWidth, float gridHeight)
    {
        Transform frameTransform = transform.Find("boardFrame");
        if (frameTransform == null)
        {
            foreach (Transform child in transform)
            {
                if (child.name.ToLower() == "boardframe")
                {
                    frameTransform = child;
                    break;
                }
            }
        }

        if (frameTransform != null)
        {
            SpriteRenderer boardFrame = frameTransform.GetComponent<SpriteRenderer>();
            if (boardFrame != null)
            {
                float framePadding = 0.25f;
                float frameWidth = gridWidth + (framePadding * 2f);
                float frameHeight = gridHeight + (framePadding * 2f);

                boardFrame.size = new Vector2(frameWidth, frameHeight);
                boardFrame.sortingOrder = -1;
            }
        }
    }

    private void CreateBlockAt(int row, int col, Vector3 startPos)
    {
        Block block = blockPool.GetBlock();
        block.transform.SetParent(transform);

        Vector3 worldPos = startPos + new Vector3(
            col * (blockSize + blockSpacing),
            row * (blockSize + blockSpacing),
            0
        );
        block.transform.localPosition = worldPos;

        int colorID = Random.Range(0, Mathf.Min(numberOfColors, blockColors.Length));
        GridPosition gridPos = new GridPosition(row, col);
        
        block.Initialize(colorID, gridPos, blockColors[colorID]);
        grid[row, col] = block;
    }

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

    private Block GetBlockAtPosition(Vector2 worldPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            return hit.collider.GetComponent<Block>();
        }
        return null;
    }

    private void OnBlockClicked(Block block)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver())
            return;

        matchedBlocks.Clear();
        FloodFill(block.GridPos, block.ColorID, matchedBlocks);

        if (matchedBlocks.Count >= 2)
        {
            block.PlayClickFeedback();
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayPop();
            
            if (GameManager.Instance != null)
                GameManager.Instance.DecreaseMove();
            
            StartCoroutine(DestroyMatchedBlocksSequence(matchedBlocks));
        }
    }

    private void FloodFill(GridPosition startPos, int targetColor, List<Block> result)
    {
        visited.Clear();
        bfsQueue.Clear();

        bfsQueue.Enqueue(startPos);
        visited.Add(startPos);

        int[] dRow = { -1, 1, 0, 0 };
        int[] dCol = { 0, 0, -1, 1 };

        while (bfsQueue.Count > 0)
        {
            GridPosition current = bfsQueue.Dequeue();
            Block currentBlock = grid[current.Row, current.Column];

            if (currentBlock != null && currentBlock.IsActive && currentBlock.ColorID == targetColor)
            {
                result.Add(currentBlock);

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

    private bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < rows && col >= 0 && col < columns;
    }

    private IEnumerator DestroyMatchedBlocksSequence(List<Block> blocksToDestroy)
    {
        isProcessing = true;

        if (blocksToDestroy.Count > 5 && mainCamera != null)
        {
            Vector3 originalCameraPos = mainCamera.transform.position;
            mainCamera.transform.DOShakePosition(0.4f, 0.15f, 30)
                .OnComplete(() => {
                    mainCamera.transform.position = originalCameraPos;
                });
        }

        yield return new WaitForSeconds(destroyDelay);

        if (GameManager.Instance != null)
        {
            int scoreAmount = blocksToDestroy.Count * 10;
            GameManager.Instance.AddScore(scoreAmount);
        }

        foreach (Block block in blocksToDestroy)
        {
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.PlayExplosion(block.transform.position, block.CurrentColor);
            }
    
            grid[block.GridPos.Row, block.GridPos.Column] = null;
        }
        blockPool.ReturnBlocks(blocksToDestroy);

        yield return StartCoroutine(ApplyGravity());

        UpdateAllBlockIcons();

        if (IsDeadlocked())
        {
            yield return StartCoroutine(SmartShuffle());
            UpdateAllBlockIcons();
        }

        isProcessing = false;
    }

    private IEnumerator ApplyGravity()
    {
        bool anyMovement = false;

        for (int col = 0; col < columns; col++)
        {
            int emptyRow = 0;

            for (int row = 0; row < rows; row++)
            {
                if (grid[row, col] != null && grid[row, col].IsActive)
                {
                    if (emptyRow != row)
                    {
                        Block block = grid[row, col];
                        grid[row, col] = null;
                        grid[emptyRow, col] = block;
                        block.SetGridPosition(new GridPosition(emptyRow, col));

                        Vector3 targetPos = GetWorldPosition(emptyRow, col);
                        block.AnimateFall(targetPos, fallDuration);
                        anyMovement = true;
                    }
                    emptyRow++;
                }
            }

            for (int row = emptyRow; row < rows; row++)
            {
                Block newBlock = blockPool.GetBlock();
                newBlock.transform.SetParent(transform);

                int colorID = Random.Range(0, Mathf.Min(numberOfColors, blockColors.Length));
                GridPosition gridPos = new GridPosition(row, col);

                Vector3 startPos = GetWorldPosition(rows, col);
                newBlock.transform.localPosition = startPos;

                newBlock.Initialize(colorID, gridPos, blockColors[colorID]);
                grid[row, col] = newBlock;

                Vector3 targetPos = GetWorldPosition(row, col);
                newBlock.AnimateFall(targetPos, fallDuration);
                anyMovement = true;
            }
        }

        if (anyMovement)
        {
            yield return new WaitForSeconds(fallDuration);
        }
    }

    private Vector3 GetWorldPosition(int row, int col)
    {
        float centerOffsetX = ((columns - 1) * (blockSize + blockSpacing)) / 2f;
        float centerOffsetY = ((rows - 1) * (blockSize + blockSpacing)) / 2f;
        
        Vector3 startPos = new Vector3(-centerOffsetX, -centerOffsetY, 0);

        return startPos + new Vector3(
            col * (blockSize + blockSpacing),
            row * (blockSize + blockSpacing),
            0
        );
    }

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
                        tempBlockList.Clear();
                        FloodFill(block.GridPos, block.ColorID, tempBlockList);
                        int groupSize = tempBlockList.Count;

                        foreach (Block b in tempBlockList)
                        {
                            groupSizeCache[b] = groupSize;
                        }
                    }

                    int size = groupSizeCache[block];
                    block.UpdateSpriteForGroupSize(size, thresholdA, thresholdB, thresholdC);
                }
            }
        }
    }

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
                        return false;
                    }
                }
            }
        }
        return true;
    }

    private IEnumerator SmartShuffle()
    {
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

        int attempts = 0;
        const int maxAttempts = 100;
        bool validShuffleFound = false;

        while (!validShuffleFound && attempts < maxAttempts)
        {
            attempts++;

            for (int i = allBlocks.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                
                Block blockA = allBlocks[i];
                Block blockB = allBlocks[j];
                
                GridPosition tempPos = blockA.GridPos;
                blockA.SetGridPosition(blockB.GridPos);
                blockB.SetGridPosition(tempPos);
                
                grid[blockA.GridPos.Row, blockA.GridPos.Column] = blockA;
                grid[blockB.GridPos.Row, blockB.GridPos.Column] = blockB;
            }

            if (allBlocks.Count >= 2)
            {
                int idx1 = Random.Range(0, allBlocks.Count);
                Block b1 = allBlocks[idx1];
                Block b2 = FindAdjacentBlock(b1);
                
                if (b2 != null)
                {
                    b1.Initialize(b2.ColorID, b1.GridPos, blockColors[b2.ColorID]);
                }
            }

            if (!IsDeadlocked())
            {
                validShuffleFound = true;
            }
        }

        foreach (Block block in allBlocks)
        {
            Vector3 targetPos = GetWorldPosition(block.GridPos.Row, block.GridPos.Column);
            block.AnimateFall(targetPos, fallDuration);
        }

        yield return new WaitForSeconds(fallDuration);
    }

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
        DOTween.KillAll();
    }
}
