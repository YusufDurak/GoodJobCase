using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Audio Manager with Singleton pattern
/// Handles all game audio including SFX and Music through AudioMixer
/// Optimized for performance with pitch randomization support
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup musicMixerGroup;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip popSound;
    [SerializeField] private AudioClip shuffleSound;
    [SerializeField] private AudioClip matchSound;
    [SerializeField] private AudioClip fallSound;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Pitch Randomization")]
    [SerializeField] private float minPitch = 0.9f;
    [SerializeField] private float maxPitch = 1.1f;

    [Header("Volume Settings")]
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.7f;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: persist across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeAudioSources();
    }

    /// <summary>
    /// Initialize audio sources and route them to mixer groups
    /// </summary>
    private void InitializeAudioSources()
    {
        // Create audio sources if not assigned
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        // Route to mixer groups
        if (sfxMixerGroup != null)
        {
            sfxSource.outputAudioMixerGroup = sfxMixerGroup;
        }
        
        if (musicMixerGroup != null)
        {
            musicSource.outputAudioMixerGroup = musicMixerGroup;
        }

        // Configure SFX source
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;

        // Configure Music source
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = musicVolume;

        // Auto-play background music if assigned
        if (backgroundMusic != null)
        {
            PlayMusic(backgroundMusic);
        }
    }

    /// <summary>
    /// Play pop sound with randomized pitch (0.9 - 1.1)
    /// Used for block interactions
    /// </summary>
    public void PlayPop()
    {
        if (popSound == null)
        {
            Debug.LogWarning("Pop sound not assigned to AudioManager!");
            return;
        }

        // Randomize pitch for variation
        sfxSource.pitch = Random.Range(minPitch, maxPitch);
        
        // Play one-shot to allow overlapping
        sfxSource.PlayOneShot(popSound, sfxVolume);
        
        // Reset pitch for other sounds
        sfxSource.pitch = 1f;
    }

    /// <summary>
    /// Play shuffle sound when board is shuffled
    /// </summary>
    public void PlayShuffle()
    {
        if (shuffleSound == null)
        {
            Debug.LogWarning("Shuffle sound not assigned to AudioManager!");
            return;
        }

        sfxSource.pitch = 1f;
        sfxSource.PlayOneShot(shuffleSound, sfxVolume);
    }

    /// <summary>
    /// Play match sound when blocks are destroyed
    /// </summary>
    public void PlayMatch()
    {
        if (matchSound == null) return;

        sfxSource.pitch = 1f;
        sfxSource.PlayOneShot(matchSound, sfxVolume);
    }

    /// <summary>
    /// Play fall sound when blocks fall
    /// </summary>
    public void PlayFall()
    {
        if (fallSound == null) return;

        sfxSource.pitch = Random.Range(0.95f, 1.05f); // Slight variation
        sfxSource.PlayOneShot(fallSound, sfxVolume * 0.5f); // Quieter
        sfxSource.pitch = 1f;
    }

    /// <summary>
    /// Play custom sound effect with optional pitch randomization
    /// </summary>
    public void PlaySFX(AudioClip clip, bool randomizePitch = false, float volumeMultiplier = 1f)
    {
        if (clip == null) return;

        if (randomizePitch)
        {
            sfxSource.pitch = Random.Range(minPitch, maxPitch);
        }

        sfxSource.PlayOneShot(clip, sfxVolume * volumeMultiplier);
        
        if (randomizePitch)
        {
            sfxSource.pitch = 1f;
        }
    }

    /// <summary>
    /// Play background music
    /// </summary>
    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip == null || musicSource == null) return;

        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }

        musicSource.clip = musicClip;
        musicSource.Play();
    }

    /// <summary>
    /// Stop background music
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    /// <summary>
    /// Pause background music
    /// </summary>
    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }

    /// <summary>
    /// Resume background music
    /// </summary>
    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.UnPause();
        }
    }

    /// <summary>
    /// Set SFX volume (0-1)
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }

        // Also set mixer volume if using mixer
        if (audioMixer != null)
        {
            audioMixer.SetFloat("SFXVolume", VolumeToDecibels(volume));
        }
    }

    /// <summary>
    /// Set music volume (0-1)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }

        // Also set mixer volume if using mixer
        if (audioMixer != null)
        {
            audioMixer.SetFloat("MusicVolume", VolumeToDecibels(volume));
        }
    }

    /// <summary>
    /// Convert linear volume (0-1) to decibels for mixer
    /// </summary>
    private float VolumeToDecibels(float volume)
    {
        return volume > 0 ? Mathf.Log10(volume) * 20f : -80f;
    }

    /// <summary>
    /// Mute/unmute all audio
    /// </summary>
    public void ToggleMute()
    {
        AudioListener.pause = !AudioListener.pause;
    }

    /// <summary>
    /// Check if audio is muted
    /// </summary>
    public bool IsMuted()
    {
        return AudioListener.pause;
    }
}
