using UnityEngine;
using UnityEngine.Audio;

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
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeAudioSources();
    }

    private void InitializeAudioSources()
    {
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        if (sfxMixerGroup != null)
        {
            sfxSource.outputAudioMixerGroup = sfxMixerGroup;
        }
        
        if (musicMixerGroup != null)
        {
            musicSource.outputAudioMixerGroup = musicMixerGroup;
        }

        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = musicVolume;

        if (backgroundMusic != null)
        {
            PlayMusic(backgroundMusic);
        }
    }

    public void PlayPop()
    {
        if (popSound == null) return;

        sfxSource.pitch = Random.Range(minPitch, maxPitch);
        sfxSource.PlayOneShot(popSound, sfxVolume);
        sfxSource.pitch = 1f;
    }

    public void PlayShuffle()
    {
        if (shuffleSound == null) return;

        sfxSource.pitch = 1f;
        sfxSource.PlayOneShot(shuffleSound, sfxVolume);
    }

    public void PlayMatch()
    {
        if (matchSound == null) return;

        sfxSource.pitch = 1f;
        sfxSource.PlayOneShot(matchSound, sfxVolume);
    }

    public void PlayFall()
    {
        if (fallSound == null) return;

        sfxSource.pitch = Random.Range(0.95f, 1.05f);
        sfxSource.PlayOneShot(fallSound, sfxVolume * 0.5f);
        sfxSource.pitch = 1f;
    }

    public void PlaySfx(AudioClip clip, bool randomizePitch = false, float volumeMultiplier = 1f)
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

    private void PlayMusic(AudioClip musicClip)
    {
        if (musicClip == null || musicSource == null) return;

        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }

        musicSource.clip = musicClip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.UnPause();
        }
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }

        if (audioMixer != null)
        {
            audioMixer.SetFloat("SFXVolume", VolumeToDecibels(volume));
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }

        if (audioMixer != null)
        {
            audioMixer.SetFloat("MusicVolume", VolumeToDecibels(volume));
        }
    }

    private float VolumeToDecibels(float volume)
    {
        return volume > 0 ? Mathf.Log10(volume) * 20f : -80f;
    }

    public void ToggleMute()
    {
        AudioListener.pause = !AudioListener.pause;
    }

    public bool IsMuted()
    {
        return AudioListener.pause;
    }
}
