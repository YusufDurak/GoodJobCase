using System.Collections.Generic;
using UnityEngine;

public class BlockPool : MonoBehaviour
{
    [SerializeField] private Block blockPrefab;
    [SerializeField] private int initialPoolSize = 100;
    
    private Queue<Block> poolQueue = new Queue<Block>();
    private List<Block> activeBlocks = new List<Block>();
    private Transform poolParent;

    public void Initialize(Transform parent)
    {
        poolParent = new GameObject("PoolContainer").transform;
        poolParent.SetParent(parent);
        
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewBlock();
        }
    }

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

    public void ReturnBlocks(List<Block> blocks)
    {
        foreach (var block in blocks)
        {
            ReturnBlock(block);
        }
    }

    private Block CreateNewBlock()
    {
        Block block = Instantiate(blockPrefab, poolParent);
        block.gameObject.SetActive(false);
        poolQueue.Enqueue(block);
        return block;
    }

    public void ClearAll()
    {
        var blocksToReturn = new List<Block>(activeBlocks);
        foreach (var block in blocksToReturn)
        {
            ReturnBlock(block);
        }
    }
}
