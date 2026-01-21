# 🎮 Unity Collapse/Blast Game - Technical Case Study

## Overview
A high-performance mobile tile-matching game built for Unity with emphasis on optimization and clean architecture.

## ✨ Features

### Core Gameplay
- ✅ Configurable M×N grid (2-10 rows/columns)
- ✅ K color types (1-6 colors)
- ✅ Tap to match groups of 2+ connected blocks
- ✅ Gravity system with smooth animations
- ✅ Dynamic icon system based on group size
- ✅ Smart deadlock detection and shuffle

### Technical Highlights
- 🚀 **Object Pooling**: Zero runtime allocation for blocks
- 🔍 **BFS Flood Fill**: Efficient O(N) matching algorithm
- 🎬 **DOTween Integration**: Smooth, performant animations
- 📊 **Memory Optimized**: Reusable collections, minimal GC
- 🎯 **Smart Shuffle**: Guarantees valid moves without brute force

## 🏗️ Architecture

```
BoardManager (Singleton pattern)
├── Grid Management
├── Input Handling
├── Match Detection (BFS)
├── Gravity System
└── Deadlock Detection

BlockPool (Object Pool)
├── Block Creation
├── Block Reuse
└── Memory Management

Block (Entity)
├── Visual State
├── Animation Control
└── Grid Position
```

## 📊 Performance Metrics

### Memory
- **Object Pooling**: ~100 pre-allocated blocks
- **Zero GC** during gameplay (after initialization)
- **Reusable Collections**: Lists, Queues, HashSets

### CPU
- **BFS Complexity**: O(N) where N = grid size
- **Update Icons**: O(N) with caching
- **Deadlock Check**: O(N × K) worst case

### Optimizations
1. Struct-based GridPosition (value type)
2. Class-level collections avoid per-frame allocation
3. Group size caching prevents redundant calculations
4. Tween management prevents memory leaks

## 🎯 Adjustable Parameters

| Parameter | Range | Description |
|-----------|-------|-------------|
| M (Rows) | 2-10 | Grid height |
| N (Columns) | 2-10 | Grid width |
| K (Colors) | 1-6 | Number of block types |
| A (Threshold) | Any | Icon1 group size |
| B (Threshold) | Any | Icon2 group size |
| C (Threshold) | Any | Icon3 group size |

## 📁 File Structure

```
Assets/
├── Scripts/
│   ├── BlockData.cs         # Data structures
│   ├── Block.cs             # Block entity
│   ├── BlockPool.cs         # Object pooling
│   └── BoardManager.cs      # Core game logic
├── Prefabs/
│   └── Block.prefab         # Block prefab
└── Sprites/
    └── [Color folders]      # Sprite assets
```

## 🚀 Quick Start

See **UNITY_SETUP_GUIDE.md** for detailed setup instructions.

### Quick Setup:
1. Install DOTween (Package Manager)
2. Import all scripts to `Assets/Scripts/`
3. Create Block prefab with SpriteRenderer + BoxCollider2D
4. Create GameBoard GameObject with BoardManager + BlockPool
5. Assign references in Inspector
6. Press Play!

## 🎮 Gameplay Rules

1. **Matching**: Click any block to select its connected group
2. **Destruction**: Groups of 2+ blocks are destroyed
3. **Gravity**: Blocks fall down to fill empty spaces
4. **Spawning**: New blocks spawn from the top
5. **Dynamic Icons**: Icons change based on group size thresholds
6. **Deadlock**: Board auto-shuffles when no moves are available

## 🧪 Algorithm Details

### Flood Fill (BFS)
```csharp
// Efficient breadth-first search
// Time: O(N), Space: O(N)
// Where N = number of grid cells
```

### Smart Shuffle
```csharp
// Strategic shuffle with guaranteed valid moves
// 1. Randomize positions
// 2. Force create at least one match
// 3. Verify with deadlock check
// Max attempts: 100
```

### Group Size Calculation
```csharp
// Cached per update cycle
// Prevents redundant BFS calls
// Invalidated after gravity/shuffle
```

## 📱 Mobile Considerations

- **Target**: 60 FPS on mid-range devices
- **Resolution**: Adaptive (uses orthographic camera)
- **Input**: Touch-friendly (large tap targets)
- **Memory**: <50MB heap allocation
- **Battery**: Minimal CPU usage during idle

## 🎨 Visual Design

### Icon System
- **Default**: Group ≤ A
- **Icon 1**: Group > A
- **Icon 2**: Group > B  
- **Icon 3**: Group > C

### Animations (DOTween)
- **Spawn**: Scale punch (0 → 1)
- **Click**: Scale punch feedback
- **Fall**: Smooth downward motion (Ease.InQuad)
- **Destroy**: Instant (with delay)

## 🐛 Known Limitations

1. **Very Small Grids**: 2×2 with 6 colors may deadlock frequently
2. **Very Large Grids**: 10×10 may need larger object pool
3. **Color Swapping**: Not implemented (could be added)
4. **Combos**: Not tracked (could be added)

## 🔮 Future Enhancements

- [ ] Power-ups (bombs, rockets, etc.)
- [ ] Score and combo system
- [ ] Level progression
- [ ] Particle effects
- [ ] Sound effects
- [ ] Persistent saves
- [ ] Leaderboards
- [ ] Tutorial system

## 📖 Code Quality

- **Clean Code**: Well-documented with XML comments
- **SOLID Principles**: Single responsibility per class
- **Performance**: Optimized for mobile
- **Maintainable**: Easy to extend and modify
- **Testable**: Decoupled architecture

## 🏆 Technical Requirements Met

✅ Grid generation (M×N adjustable)  
✅ K colors (1-6 adjustable)  
✅ Match detection (2+ connected blocks)  
✅ Gravity system  
✅ Dynamic icons (A, B, C thresholds)  
✅ Deadlock detection  
✅ Smart shuffle (guaranteed moves)  
✅ Object pooling  
✅ BFS flood fill  
✅ DOTween animations  
✅ Performance optimized  

## 📄 License

This is a technical case study project. Use as needed.

## 🙏 Acknowledgments

- **DOTween**: Animation library by Demigiant
- **Unity**: Game engine
- **Reference**: Toon Blast / Toy Blast style gameplay

---

**Built with ❤️ for mobile game performance**
