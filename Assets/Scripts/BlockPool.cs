using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object pool for blocks to optimize memory allocation
/// </summary>
public class BlockPool : MonoBehaviour
{
    [SerializeField] private Block blockPrefab;
    [SerializeField] private int initialPoolSize = 100;
    
    private Queue<Block> poolQueue = new Queue<Block>();
    private List<Block> activeBlocks = new List<Block>();
    private Transform poolParent;

    /// <summary>
    /// Initialize the pool
    /// </summary>
    public void Initialize(Transform parent)
    {
        poolParent = new GameObject("BlockPool").transform;
        poolParent.SetParent(parent);
        
        // Pre-instantiate blocks
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewBlock();
        }
    }

    /// <summary>
    /// Get a block from the pool
    /// </summary>
    public Block GetBlock()
    {
        Block block;
        
        if (poolQueue.Count > 0)
        {
            block = poolQueue.Dequeue();
        }
        else
        {
            block = CreateNewBlock();
        }
        
        activeBlocks.Add(block);
        block.gameObject.SetActive(true);
        return block;
    }

    /// <summary>
    /// Return a block to the pool
    /// </summary>
    public void ReturnBlock(Block block)
    {
        if (block == null) return;
        
        block.Deactivate();
        block.KillAllTweens();
        block.gameObject.SetActive(false);
        block.transform.SetParent(poolParent);
        
        activeBlocks.Remove(block);
        poolQueue.Enqueue(block);
    }

    /// <summary>
    /// Return multiple blocks to the pool
    /// </summary>
    public void ReturnBlocks(List<Block> blocks)
    {
        foreach (var block in blocks)
        {
            ReturnBlock(block);
        }
    }

    /// <summary>
    /// Create a new block instance
    /// </summary>
    private Block CreateNewBlock()
    {
        Block block = Instantiate(blockPrefab, poolParent);
        block.gameObject.SetActive(false);
        poolQueue.Enqueue(block);
        return block;
    }

    /// <summary>
    /// Clear all active blocks
    /// </summary>
    public void ClearAll()
    {
        // Return all active blocks to pool
        var blocksToReturn = new List<Block>(activeBlocks);
        foreach (var block in blocksToReturn)
        {
            ReturnBlock(block);
        }
    }
}
