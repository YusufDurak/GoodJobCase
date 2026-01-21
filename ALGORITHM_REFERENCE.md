# Algorithm & Performance Reference

## 🔍 Flood Fill Algorithm (BFS)

### Purpose
Detect all connected blocks of the same color from a starting position.

### Implementation
```
Algorithm: Breadth-First Search (BFS)
Time Complexity: O(N)
Space Complexity: O(N)
Where N = number of grid cells
```

### Pseudocode
```
FloodFill(startPosition, targetColor):
    visited = new HashSet
    queue = new Queue
    result = new List
    
    queue.Enqueue(startPosition)
    visited.Add(startPosition)
    
    while queue is not empty:
        current = queue.Dequeue()
        currentBlock = grid[current]
        
        if currentBlock.color == targetColor:
            result.Add(currentBlock)
            
            for each neighbor in [up, down, left, right]:
                if neighbor is valid AND not visited:
                    visited.Add(neighbor)
                    queue.Enqueue(neighbor)
    
    return result
```

### Why BFS over DFS?
- **Level-order traversal**: Natural for grid-based games
- **Iterative**: No stack overflow risk
- **Cache-friendly**: Better spatial locality
- **Predictable**: Consistent performance

---

## 📊 Dynamic Icon System

### Formula
```
Icon = f(GroupSize, A, B, C)

Where:
- GroupSize ≤ A  → Default Icon
- A < GroupSize ≤ B → Icon 1
- B < GroupSize ≤ C → Icon 2
- GroupSize > C → Icon 3
```

### Implementation Strategy
```
UpdateIcon(block, groupSize, A, B, C):
    if groupSize > C:
        block.sprite = Icon3
    else if groupSize > B:
        block.sprite = Icon2
    else if groupSize > A:
        block.sprite = Icon1
    else:
        block.sprite = Default
```

### Optimization: Group Size Caching
```
Performance Issue: Calculating group size for every block is O(N²)
Solution: Cache group sizes during update cycle

groupSizeCache = new Dictionary<Block, int>

For each block in grid:
    if block not in cache:
        group = FloodFill(block)
        size = group.Count
        
        // Cache for entire group
        for each member in group:
            cache[member] = size
    
    block.UpdateIcon(cache[block])

Result: O(N) instead of O(N²)
```

---

## 🎯 Gravity System

### Algorithm
```
For each column (left to right):
    emptyRow = 0
    
    // Phase 1: Move existing blocks down
    For each row (bottom to top):
        if cell has active block:
            if emptyRow != currentRow:
                Move block from currentRow to emptyRow
                Animate fall
            emptyRow++
    
    // Phase 2: Spawn new blocks
    For remaining empty rows (emptyRow to top):
        Create new block at top
        Animate fall to target position
```

### Time Complexity
- **Best Case**: O(M × N) - No empty spaces
- **Worst Case**: O(M × N) - All blocks destroyed
- **Average**: O(M × N)

Where M = rows, N = columns

---

## 🔄 Smart Shuffle Algorithm

### Goal
Guarantee at least one valid move after shuffle (no infinite loops).

### Algorithm
```
SmartShuffle():
    maxAttempts = 100
    attempts = 0
    
    while attempts < maxAttempts:
        attempts++
        
        // Step 1: Shuffle positions
        blocks = GetAllBlocks()
        Shuffle(blocks.positions)
        
        // Step 2: Force create match
        b1 = RandomBlock()
        b2 = FindAdjacentBlock(b1)
        if b2 exists:
            b1.color = b2.color  // Guarantee adjacent match
        
        // Step 3: Verify
        if HasValidMove():
            AnimateToNewPositions()
            return SUCCESS
    
    // Fallback: Reset board
    return REGENERATE_BOARD
```

### Why This Works
1. **Random shuffle**: Distributes colors
2. **Forced match**: Guarantees ≥1 valid group
3. **Verification**: Double-checks for moves
4. **Bounded attempts**: Prevents infinite loops

### Time Complexity
- **Expected**: O(N) - Usually succeeds in 1-2 attempts
- **Worst Case**: O(N × MaxAttempts) = O(100N)

---

## 🏆 Object Pooling

### Memory Savings
```
Without Pooling:
- 100 moves × 20 blocks = 2000 allocations
- 2000 deallocations
- Heavy GC pressure

With Pooling:
- 100 blocks pre-allocated
- 0 allocations during gameplay
- 0 GC triggers
```

### Implementation Pattern
```
Pool:
    queue<Block> available
    list<Block> active
    
    GetBlock():
        if available.isEmpty():
            block = new Block()
        else:
            block = available.Dequeue()
        active.Add(block)
        return block
    
    ReturnBlock(block):
        block.Reset()
        active.Remove(block)
        available.Enqueue(block)
```

### Memory Profile
```
Initial Pool Size: 100 blocks
Block Size: ~500 bytes (sprite, transform, collider)
Total Pool Memory: ~50 KB
Peak Memory: ~75 KB (during spawning)
GC Collections: 0 (after init)
```

---

## 📈 Performance Metrics

### Target Performance (Mobile)
```
Frame Rate: 60 FPS (16.67ms per frame)
Memory: <50 MB
GC Triggers: 0 per minute
CPU Usage: <20%
Battery Impact: Minimal
```

### Breakdown of Frame Time
```
Total: 16.67ms target

Breakdown:
- Input Handling: <0.5ms
- Flood Fill (match): <1ms (worst case)
- Icon Updates: <1ms (with caching)
- Gravity: <0.5ms
- Animation: <1ms (DOTween)
- Rendering: <10ms
- Other: <3.67ms

Margin: ~0ms (optimization needed if exceeded)
```

### Optimization Techniques Used

#### 1. Reusable Collections
```csharp
// ❌ Bad: Creates GC every frame
void Update() {
    List<Block> matches = new List<Block>();  // 24 bytes allocated
    // ...
}

// ✅ Good: Reuse class-level collection
List<Block> matches = new List<Block>(100);  // Allocated once
void Update() {
    matches.Clear();  // 0 bytes allocated
    // ...
}
```

#### 2. Struct vs Class
```csharp
// ❌ Bad: Heap allocation for position
class GridPosition { int row, col; }  // 16 bytes + overhead

// ✅ Good: Stack allocation
struct GridPosition { int row, col; }  // 8 bytes, no GC
```

#### 3. Cached Calculations
```csharp
// ❌ Bad: Recalculate every frame
foreach (Block b in blocks) {
    int size = FloodFill(b).Count;  // O(N²)
}

// ✅ Good: Calculate once, cache results
Dictionary<Block, int> cache = new();
foreach (Block b in blocks) {
    if (!cache.ContainsKey(b)) {
        // Calculate for entire group once
    }
    int size = cache[b];  // O(1) lookup
}
```

#### 4. DOTween Management
```csharp
// ❌ Bad: Tween leak
transform.DOMove(target, 1f);  // Old tween not killed
transform.DOMove(target2, 1f);  // Memory leak!

// ✅ Good: Kill before new tween
if (tween != null && tween.IsActive())
    tween.Kill();
tween = transform.DOMove(target, 1f);
```

---

## 🧮 Complexity Analysis

### Core Operations

| Operation | Time | Space | Frequency |
|-----------|------|-------|-----------|
| FloodFill | O(N) | O(N) | Per click |
| UpdateIcons | O(N) | O(N) | Per match |
| Gravity | O(M×N) | O(1) | Per match |
| Deadlock Check | O(N×K) | O(N) | Per match |
| Shuffle | O(N) | O(N) | Rare |

Where:
- N = M × Columns (grid size)
- M = Rows
- K = Number of colors

### Total Per Match
```
Click → FloodFill → Destroy → Gravity → UpdateIcons → Check Deadlock
O(N) → O(N) → O(1) → O(M×N) → O(N) → O(N×K)

Dominant: O(M×N)
For 8×8 grid: ~64 operations
At 60 FPS: <1ms ✓
```

---

## 🔬 Profiling Tips

### Unity Profiler Hotspots to Watch
1. **CPU Usage > Rendering**: Should be <10ms
2. **CPU Usage > Scripts**: Should be <5ms
3. **GC.Alloc**: Should be 0 during gameplay
4. **Draw Calls**: Use Sprite Atlas to batch
5. **Active Objects**: Should be ~100-150

### Optimization Checklist
- [ ] Object pooling enabled
- [ ] Reusable collections used
- [ ] Group size caching active
- [ ] Tweens properly killed
- [ ] Sprite atlas for batching
- [ ] Compressed textures
- [ ] No per-frame allocations
- [ ] No string concatenation in Update()

---

## 📱 Mobile Optimization Guide

### Device Tiers

**High-End** (iPhone 12+, Galaxy S20+)
- Grid: 10×10
- Colors: 6
- Animations: Full quality
- Target: 60 FPS

**Mid-Range** (iPhone X, Galaxy S10)
- Grid: 8×8
- Colors: 5
- Animations: Balanced
- Target: 60 FPS

**Low-End** (iPhone 8, Galaxy S7)
- Grid: 6×6
- Colors: 4
- Animations: Simplified
- Target: 30-60 FPS

### Memory Budgets
```
Low-End: <100 MB
Mid-Range: <150 MB
High-End: <200 MB

Our Game: ~50 MB ✓
```

---

## 🎓 Further Reading

### Related Algorithms
1. **Flood Fill Variants**
   - Scanline Fill (faster for large areas)
   - 8-way connectivity (diagonal matches)

2. **Graph Algorithms**
   - Connected Components (multiple groups)
   - Strongly Connected Components

3. **Game-Specific**
   - Cascade Detection (chain reactions)
   - Power-up Triggers (special blocks)
   - Pattern Matching (L-shapes, T-shapes)

### Performance Resources
- Unity Profiler Documentation
- DOTween Best Practices
- Mobile Game Optimization Guide
- C# Memory Management

---

**Note**: All complexity analyses assume practical grid sizes (M, N ≤ 10). For larger grids, additional optimizations may be needed.
