# AudioManager Setup Guide

## ✅ **AudioManager.cs Created**

A professional, production-ready audio system with:
- ✅ Singleton pattern
- ✅ AudioMixer integration
- ✅ Pitch randomization for PlayPop()
- ✅ PlayOneShot for overlapping sounds
- ✅ Clean, well-documented code

---

## 🎵 **Setup Instructions**

### **Step 1: Create AudioMixer**

1. In Project window: **Right-click > Create > Audio Mixer**
2. Name it **"MainAudioMixer"**
3. Open the AudioMixer window: **Window > Audio > Audio Mixer**

### **Step 2: Configure Mixer Groups**

In the AudioMixer window:

1. Click the **"+"** next to "Groups"
2. Create two groups:
   - **SFX** (for sound effects)
   - **Music** (for background music)

Your hierarchy should look like:
```
Master
├─ SFX
└─ Music
```

### **Step 3: Add Exposed Parameters (Optional)**

For volume control via sliders:

1. Click on **SFX** group
2. Right-click on **Volume** in Inspector
3. Select **"Expose 'Volume' to script"**
4. Rename to **"SFXVolume"** in the exposed parameters list
5. Repeat for **Music** group, name it **"MusicVolume"**

---

## 🎮 **Scene Setup**

### **Step 1: Create AudioManager GameObject**

1. In Hierarchy: **Right-click > Create Empty**
2. Name it **"AudioManager"**
3. Add **AudioManager.cs** component

### **Step 2: Assign References in Inspector**

Select AudioManager GameObject and assign:

```
AudioManager (Script)
├─ Audio Mixer
│  └─ Assign: MainAudioMixer
├─ Sfx Mixer Group
│  └─ Assign: SFX (from MainAudioMixer)
├─ Music Mixer Group
│  └─ Assign: Music (from MainAudioMixer)
├─ Sfx Source
│  └─ Leave empty (auto-created) or assign AudioSource
├─ Music Source
│  └─ Leave empty (auto-created) or assign AudioSource
├─ Sound Effects
│  ├─ Pop Sound: [Your pop audio clip]
│  ├─ Shuffle Sound: [Your shuffle audio clip]
│  ├─ Match Sound: [Your match audio clip]
│  └─ Fall Sound: [Your fall audio clip]
├─ Music
│  └─ Background Music: [Your music clip]
├─ Pitch Randomization
│  ├─ Min Pitch: 0.9
│  └─ Max Pitch: 1.1
└─ Volume Settings
   ├─ Sfx Volume: 1.0
   └─ Music Volume: 0.7
```

**Note**: If you leave Sfx Source and Music Source empty, they'll be created automatically and routed to the correct mixer groups!

---

## 🎯 **Usage in Your Game**

### **In BoardManager.cs (or wherever you need audio):**

```csharp
// Play pop sound when clicking blocks
AudioManager.Instance.PlayPop();  // ← Randomized pitch!

// Play shuffle sound
AudioManager.Instance.PlayShuffle();

// Play match sound when destroying blocks
AudioManager.Instance.PlayMatch();

// Play fall sound when blocks fall
AudioManager.Instance.PlayFall();

// Play custom sound with pitch randomization
AudioManager.Instance.PlaySFX(myClip, randomizePitch: true);
```

### **Integration Example (BoardManager):**

Add these calls to BoardManager.cs:

```csharp
// In OnBlockClicked() - when clicking a block
private void OnBlockClicked(Block block)
{
    matchedBlocks.Clear();
    FloodFill(block.GridPos, block.ColorID, matchedBlocks);

    if (matchedBlocks.Count >= 2)
    {
        block.PlayClickFeedback();
        AudioManager.Instance.PlayPop();  // ← Add this!
        StartCoroutine(DestroyMatchedBlocksSequence(matchedBlocks));
    }
}

// In DestroyMatchedBlocksSequence() - when destroying blocks
if (matchedBlocks.Count >= 2)
{
    AudioManager.Instance.PlayMatch();  // ← Add this!
}

// In SmartShuffle() - when shuffling
AudioManager.Instance.PlayShuffle();  // ← Add this!
```

---

## 🎨 **Features Included**

### **Core Methods:**
- `PlayPop()` - Pop sound with **randomized pitch (0.9-1.1)**
- `PlayShuffle()` - Shuffle sound
- `PlayMatch()` - Match/destroy sound
- `PlayFall()` - Fall sound (with slight pitch variation)
- `PlaySFX(clip, randomizePitch, volume)` - Generic SFX player

### **Music Control:**
- `PlayMusic(clip)` - Play background music
- `StopMusic()` - Stop music
- `PauseMusic()` - Pause music
- `ResumeMusic()` - Resume music

### **Volume Control:**
- `SetSFXVolume(0-1)` - Control SFX volume
- `SetMusicVolume(0-1)` - Control music volume
- `ToggleMute()` - Mute/unmute all audio
- `IsMuted()` - Check mute state

### **Special Features:**
- ✅ **Pitch Randomization**: PlayPop() varies pitch 0.9-1.1
- ✅ **PlayOneShot**: Allows overlapping sounds
- ✅ **Auto-routing**: Audio sources automatically connected to mixer groups
- ✅ **Singleton Pattern**: Access from anywhere via `AudioManager.Instance`
- ✅ **Null Safety**: All methods check for null clips

---

## 🔧 **Optimization Notes**

### **Performance:**
- Uses `PlayOneShot()` for SFX (allows overlapping without creating new AudioSources)
- Minimal memory allocation
- Pitch reset after randomization

### **Best Practices:**
- Singleton persists across scenes (optional, can be disabled)
- Audio sources created automatically if not assigned
- Mixer groups properly routed
- Volume converted to decibels for mixer compatibility

---

## 📝 **Quick Test**

To test if it's working:

1. Press Play
2. Open Console
3. In another script, call:
   ```csharp
   AudioManager.Instance.PlayPop();
   ```
4. You should hear the sound with varying pitch each time!

---

## 🎵 **Finding/Creating Audio**

### **Free Sound Resources:**
- **Freesound.org** - Free sound effects
- **OpenGameArt.org** - Game audio
- **Incompetech.com** - Royalty-free music
- **Unity Asset Store** - Free audio packs

### **Quick Test Sounds:**
For testing, you can:
1. Record simple sounds (clap, snap, voice)
2. Use Unity's built-in audio clips
3. Generate sounds with tools like BFXR or ChipTone

---

## 🚀 **Advanced: Volume Sliders**

If you want UI sliders for volume control:

```csharp
using UnityEngine.UI;

public class AudioUI : MonoBehaviour
{
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    
    private void Start()
    {
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
    }
}
```

---

## ✅ **Checklist**

Setup complete when:
- [ ] MainAudioMixer created with SFX and Music groups
- [ ] AudioManager GameObject in scene
- [ ] AudioManager script has mixer and groups assigned
- [ ] At least one sound clip assigned (for testing)
- [ ] Can call `AudioManager.Instance.PlayPop()` without errors
- [ ] Pitch varies each time you call PlayPop()
- [ ] Multiple sounds can overlap (PlayOneShot working)

---

**Your audio system is now production-ready!** 🎵✨
