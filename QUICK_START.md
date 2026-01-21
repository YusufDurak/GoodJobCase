# ⚡ Quick Start - One Page Guide

## 1️⃣ Install DOTween (2 minutes)
```
Window > Package Manager > Search "DOTween" > Import
Tools > Demigiant > DOTween Utility Panel > Setup DOTween
```

## 2️⃣ Copy Scripts (1 minute)
```
Assets/Scripts/
├── BlockData.cs      ✓
├── Block.cs          ✓
├── BlockPool.cs      ✓
└── BoardManager.cs   ✓
```

## 3️⃣ Create Block Prefab (3 minutes)
1. Hierarchy > Right-click > Create Empty > Name: "Block"
2. Add Component > **Sprite Renderer**
3. Add Component > **Box Collider 2D**
4. Add Component > **Block** (your script)
5. Drag to `Assets/Prefabs/` folder
6. Delete from Hierarchy

## 4️⃣ Create Test Sprites (5 minutes)
Create 5 colored squares (64×64 PNG):
- Red, Blue, Green, Yellow, Purple
- For quick test: Use same sprite for Default, Icon1, Icon2, Icon3

Import settings:
- Texture Type: **Sprite (2D and UI)**
- Pixels Per Unit: **100**

## 5️⃣ Setup Scene (5 minutes)

**Camera**:
- Projection: **Orthographic**
- Size: **5**
- Position: (0, 0, -10)
- Tag: **MainCamera**

**GameBoard**:
1. Create Empty GameObject > Name: "GameBoard"
2. Add Component > **Board Manager**
3. Add Component > **Block Pool**

## 6️⃣ Configure Inspector (5 minutes)

**Block Pool**:
- Block Prefab: [Drag Block prefab]
- Initial Pool Size: **100**

**Board Manager**:

*Grid Settings*:
- Rows: **8**
- Columns: **8**
- Block Size: **1**
- Block Spacing: **0.1**

*Block Colors*:
- Number Of Colors: **5**
- Block Colors (Size: 5)
  - [0] Color ID: 0, Default Sprite: Red
  - [1] Color ID: 1, Default Sprite: Blue
  - [2] Color ID: 2, Default Sprite: Green
  - [3] Color ID: 3, Default Sprite: Yellow
  - [4] Color ID: 4, Default Sprite: Purple

*Icon Thresholds*:
- Threshold A: **5**
- Threshold B: **10**
- Threshold C: **15**

*Animation*:
- Fall Duration: **0.3**
- Destroy Delay: **0.1**

*Components*:
- Block Pool: [Drag BlockPool component]
- Main Camera: [Drag Main Camera]

## 7️⃣ Press Play! (0 minutes)
✅ Board should generate  
✅ Click blocks to match groups of 2+  
✅ Watch gravity and animations  

---

## 🎯 Default Parameters (Copy-Paste Ready)

```csharp
// Grid
rows = 8
columns = 8
blockSize = 1.0f
blockSpacing = 0.1f

// Gameplay
numberOfColors = 5
thresholdA = 5
thresholdB = 10
thresholdC = 15

// Animation
fallDuration = 0.3f
destroyDelay = 0.1f
spawnPunchScale = 1.2f
clickPunchScale = 1.15f

// Pool
initialPoolSize = 100
```

---

## 🐛 Quick Fixes

| Problem | Solution |
|---------|----------|
| No blocks appear | Check Block Prefab assigned in BlockPool |
| Clicks don't work | Block prefab needs BoxCollider2D |
| DOTween errors | Install DOTween from Package Manager |
| Sprites missing | Assign sprites in Block Colors array |
| Low FPS | Reduce grid size to 6×6 |

---

## 📱 Recommended Settings by Platform

### Mobile (8×8 grid)
```
rows = 8
columns = 8
numberOfColors = 5
fallDuration = 0.25f
```

### Tablet (10×10 grid)
```
rows = 10
columns = 10
numberOfColors = 6
fallDuration = 0.3f
```

### PC/Web (10×10 grid)
```
rows = 10
columns = 10
numberOfColors = 6
fallDuration = 0.35f
```

---

## 🎨 Inspector At-A-Glance

```
GameBoard
├─ BoardManager
│  ├─ Grid Settings [8, 8, 1.0, 0.1]
│  ├─ Block Colors [Size: 5, All sprites assigned]
│  ├─ Thresholds [5, 10, 15]
│  ├─ Animation [0.3, 0.1]
│  └─ Components [✓ Pool, ✓ Camera]
└─ BlockPool
   ├─ Block Prefab [✓ Assigned]
   └─ Pool Size [100]
```

---

## ⚡ Speed Run (Experienced Unity Devs)

1. Install DOTween *(1 min)*
2. Copy 4 core scripts *(30 sec)*
3. Create Block prefab: SpriteRenderer + BoxCollider2D + Block.cs *(1 min)*
4. Create 5 test sprites *(2 min)*
5. GameBoard: BoardManager + BlockPool *(30 sec)*
6. Assign: Prefab, Sprites, References *(2 min)*
7. Press Play *(0 sec)*

**Total: ~7 minutes** ⏱️

---

## 🎓 Learning Path

| Time | Read This |
|------|-----------|
| Now | QUICK_START.md (you are here) |
| +5 min | README.md (overview) |
| +15 min | UNITY_SETUP_GUIDE.md (detailed setup) |
| +30 min | ALGORITHM_REFERENCE.md (deep dive) |
| As needed | TROUBLESHOOTING.md (when stuck) |

---

## 🔥 Hotkeys During Play

| Key | Action |
|-----|--------|
| F1 | Toggle performance stats (if PerformanceMonitor added) |
| Click Block | Match and destroy group |

---

## ✅ Working? Next Steps

Once you have the basic game running:

1. **Visual Polish**
   - Create proper sprites with icons
   - Add particle effects on match
   - Add background image

2. **Gameplay**
   - Add scoring (use GameUI.cs)
   - Add move limits
   - Add level goals

3. **Performance**
   - Profile on target device
   - Optimize sprite atlas
   - Test with larger grids

4. **Polish**
   - Add sound effects
   - Add screen shake on large matches
   - Add tutorial

---

## 🆘 Still Stuck?

1. **Check Console** - Any red errors?
2. **Verify Inspector** - All references assigned?
3. **Read TROUBLESHOOTING.md** - Common issues covered
4. **Check Validation** - BoardManager Inspector shows warnings

---

## 🚀 Production Checklist

Ready to ship? Verify:

- [ ] DOTween installed
- [ ] All sprites assigned (no missing references)
- [ ] Tested on target device (not just editor)
- [ ] FPS is 60 on target device
- [ ] No console errors/warnings
- [ ] Object pooling working (check Profiler: GC.Alloc = 0)
- [ ] Deadlock shuffle tested
- [ ] All thresholds achievable on your grid
- [ ] UI functional (if using GameUI.cs)
- [ ] Build size acceptable

---

**Congratulations! You now have a production-ready collapse/blast game!** 🎉

For more details, see the other documentation files.

**Good luck with your technical case study!** 🚀
