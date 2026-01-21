# Troubleshooting Guide

## 🐛 Common Issues and Solutions

### Installation Issues

#### ❌ "The type or namespace name 'DG' could not be found"
**Cause**: DOTween is not installed or not imported correctly.

**Solution**:
1. Open **Window > Package Manager**
2. Search for **DOTween (HOTween v2)**
3. Import the package
4. Open **Tools > Demigiant > DOTween Utility Panel**
5. Click **Setup DOTween**
6. Restart Unity

**Alternative**: Download from [Asset Store](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676)

---

#### ❌ "The type or namespace name 'TMPro' could not be found"
**Cause**: TextMeshPro is not installed (only affects optional UI scripts).

**Solution**:
1. Open **Window > Package Manager**
2. Search for **TextMesh Pro**
3. Click **Install**
4. Or simply remove `GameUI.cs` and `PerformanceMonitor.cs` if not needed

---

### Runtime Issues

#### ❌ Board doesn't generate / No blocks appear
**Possible Causes**:
1. Block Prefab not assigned in BlockPool
2. Sprites not assigned in BlockColors array
3. Camera not positioned correctly

**Solutions**:

**Check Block Pool**:
- Select GameBoard GameObject
- Verify **Block Pool > Block Prefab** is assigned
- Verify **Initial Pool Size** > 0

**Check Block Colors**:
- Expand **Block Colors** array
- Verify **Size** matches **Number Of Colors**
- Each entry must have **Default Sprite** assigned

**Check Camera**:
- Camera should be at (0, 0, -10)
- Projection: **Orthographic**
- Size: **5-8** depending on grid size

---

#### ❌ Blocks appear but clicks don't work
**Possible Causes**:
1. Missing BoxCollider2D on Block prefab
2. Camera not assigned in BoardManager
3. Block prefab on wrong layer

**Solutions**:

**Check Collider**:
1. Open Block Prefab
2. Verify **BoxCollider2D** component exists
3. Collider should NOT be trigger
4. Size should be (1, 1)

**Check Camera**:
- BoardManager > Components > **Main Camera** assigned
- Or leave empty to auto-detect (camera must be tagged "MainCamera")

**Check Raycast**:
- Block layer should be in camera's culling mask
- No UI elements blocking clicks (check Canvas sort order)

---

#### ❌ Blocks fall through the board
**Cause**: Gravity calculation error or positioning issue.

**Solution**:
- Check **Block Size** and **Block Spacing** in BoardManager
- Ensure values are positive
- Typical: Block Size = 1.0, Block Spacing = 0.1

---

#### ❌ Icons don't change
**Possible Causes**:
1. Icon sprites not assigned
2. Thresholds set too high
3. Groups never exceed threshold

**Solutions**:

**Check Sprites**:
- Each color needs Icon1, Icon2, Icon3 sprites
- Can leave blank if not using all tiers

**Check Thresholds**:
- Default: A=5, B=10, C=15
- If grid is 6×6 (36 cells), max group = 36
- Ensure C < 36 or reduce threshold

**Test**:
1. Set A=2, B=4, C=6 for testing
2. Click large groups (5+ blocks)
3. Verify icon changes

---

#### ❌ Game freezes on shuffle
**Possible Causes**:
1. Infinite shuffle loop
2. Grid too small for number of colors
3. MaxAttempts too high

**Solutions**:

**Check Grid/Color Ratio**:
- Minimum: 2 cells per color
- Recommended: 5+ cells per color
- Example: 6×6 grid = max 7 colors safe

**Reduce Colors or Increase Grid**:
- 4×4 grid: Use 3-4 colors max
- 6×6 grid: Use 4-5 colors
- 8×8 grid: Use 5-6 colors

**Check Console**:
- Look for "Deadlock detected!" message
- If repeating, grid is too constrained

---

### Performance Issues

#### ❌ Low FPS (< 30)
**Possible Causes**:
1. Grid too large
2. Too many active tweens
3. No sprite atlas
4. Quality settings too high

**Solutions**:

**Reduce Grid Size**:
- Target for mobile: 8×8 or smaller
- 10×10 may be too large for low-end devices

**Check Profiler**:
1. Open **Window > Analysis > Profiler**
2. Click Play
3. Check CPU usage
4. Look for spikes in Scripts or Rendering

**Optimize Rendering**:
1. Create Sprite Atlas: **Assets > Create > 2D > Sprite Atlas**
2. Add all block sprites to atlas
3. Check batching in **Stats** window

**Lower Quality**:
- **Edit > Project Settings > Quality**
- Disable shadows
- Reduce anti-aliasing

---

#### ❌ Memory warnings / crashes
**Cause**: Object pool too small or memory leak.

**Solutions**:

**Increase Pool Size**:
- Select GameBoard
- BlockPool > **Initial Pool Size**: Increase to 150-200

**Check for Leaks**:
1. Open **Window > Analysis > Profiler**
2. Select **Memory** tab
3. Look for increasing **GC.Alloc**
4. Should be ~0 during gameplay after first match

---

### Visual Issues

#### ❌ Blocks are too small/large
**Solution**:
- Adjust **Block Size** in BoardManager (default: 1.0)
- Adjust Camera **Size** (default: 5)
- Formula: Camera Size ≈ (Rows × BlockSize) / 2

---

#### ❌ Blocks overlap or have gaps
**Solution**:
- Adjust **Block Spacing** (default: 0.1)
- Increase for gaps, decrease for tighter layout
- Can be 0 for no spacing

---

#### ❌ Sprites are blurry
**Cause**: Import settings incorrect.

**Solution**:
1. Select all sprites
2. **Texture Type**: Sprite (2D and UI)
3. **Filter Mode**: Point (for pixel art) or Bilinear
4. **Compression**: None (best quality) or High Quality
5. Click **Apply**

---

#### ❌ Animations are choppy
**Possible Causes**:
1. VSync disabled
2. FPS drop
3. Tween duration too short

**Solutions**:

**Enable VSync**:
- **Edit > Project Settings > Quality**
- **V Sync Count**: Every V Blank

**Adjust Timings**:
- BoardManager > Animation Settings
- Increase **Fall Duration** (0.3 → 0.5)
- Increase **Spawn Punch Duration**

---

### Editor Issues

#### ❌ Custom Editor not showing
**Cause**: Editor script not in Editor folder.

**Solution**:
1. Create folder: `Assets/Scripts/Editor/`
2. Move `BoardManagerEditor.cs` to this folder
3. Move `GameConfigValidator.cs` to this folder
4. Restart Unity

---

#### ❌ "Regenerate Board" button doesn't work
**Cause**: Not in Play mode.

**Solution**:
- Button only works in Play mode
- Press Play first, then click button

---

## 🔍 Debugging Tips

### Enable Debug Logs
Add to BoardManager.cs:

```csharp
// In OnBlockClicked():
Debug.Log($"Clicked block at ({block.GridPos.Row}, {block.GridPos.Column})");
Debug.Log($"Group size: {matchedBlocks.Count}");

// In ApplyGravity():
Debug.Log($"Gravity applied to column {col}");

// In SmartShuffle():
Debug.Log($"Shuffle attempt {attempts}");
```

### Visualize Grid
Add to BoardManager.cs:

```csharp
private void OnDrawGizmos()
{
    if (grid == null) return;

    for (int row = 0; row < rows; row++)
    {
        for (int col = 0; col < columns; col++)
        {
            Vector3 pos = GetWorldPosition(row, col);
            Gizmos.color = grid[row, col] != null ? Color.green : Color.red;
            Gizmos.DrawWireCube(pos, Vector3.one * blockSize);
        }
    }
}
```

### Performance Monitoring
Press **F1** during gameplay (if PerformanceMonitor.cs is active) to toggle performance stats.

---

## 📞 Still Having Issues?

### Checklist Before Reporting:
- [ ] DOTween installed and setup
- [ ] Block Prefab has SpriteRenderer + BoxCollider2D
- [ ] All sprites assigned in Inspector
- [ ] BlockPool reference assigned
- [ ] Camera is Orthographic
- [ ] Unity version 2020.3 or later
- [ ] Tested in Play mode (not Edit mode)

### Gather Information:
1. Unity version
2. Platform (Windows/Mac/iOS/Android)
3. Console errors (copy full error)
4. Inspector screenshot
5. Profiler screenshot (if performance issue)

### Common Error Messages:

**"NullReferenceException: Object reference not set to an instance of an object"**
- Something is not assigned in Inspector
- Check all references in BoardManager and BlockPool

**"IndexOutOfRangeException"**
- Array access out of bounds
- Usually: BlockColors array too small for numberOfColors

**"DOTween is not initialized"**
- DOTween not setup correctly
- Run DOTween Setup Utility Panel

---

## 🎯 Best Practices for Stability

1. **Always test with default settings first**
   - 8×8 grid
   - 5 colors
   - Thresholds: 5, 10, 15

2. **Increase complexity gradually**
   - Add colors one at a time
   - Test after each change
   - Monitor performance

3. **Use validation tools**
   - BoardManager Inspector shows warnings
   - GameConfig Inspector validates settings
   - Check green/yellow/red indicators

4. **Profile regularly**
   - Use Unity Profiler
   - Check memory usage
   - Monitor FPS

5. **Test on target device**
   - Editor performance != device performance
   - Always test on actual mobile device
   - Use lowest-spec device in target range

---

## 🆘 Emergency Fixes

### Nuclear Option: Reset Everything
If nothing works:

1. Close Unity
2. Delete `Library` folder
3. Delete `Temp` folder
4. Reopen Unity (will reimport everything)
5. Reassign all references in Inspector
6. Test again

### Minimal Test Setup
Create simplest possible configuration:
- 4×4 grid
- 3 colors
- Same sprite for all icons (just change color)
- No optional scripts (remove UI, Performance Monitor)

If this works, add complexity incrementally.

---

**Remember**: Most issues are configuration problems, not code bugs. Double-check all Inspector assignments!
