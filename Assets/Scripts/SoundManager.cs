using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Clips")]
    public AudioClip pickupSound;
    public AudioClip dropSound;
    public AudioClip buttonClickSound;
    public AudioClip gameCompleteSound;
    public AudioClip wrongPlacementSound;
    public AudioClip backgroundMusic;

    [Header("Audio Sources")]
    public AudioSource audioSource; // For sound effects
    public AudioSource musicSource; // For background music

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayBackgroundMusic();
    }

    /// <summary>
    /// Plays the pickup sound.
    /// </summary>
    public void PlayPickupSound()
    {
        PlaySound(pickupSound);
    }

    /// <summary>
    /// Plays the drop sound.
    /// </summary>
    public void PlayDropSound()
    {
        PlaySound(dropSound);
    }

    /// <summary>
    /// Plays the button click sound.
    /// </summary>
    public void PlayButtonClickSound()
    {
        PlaySound(buttonClickSound);
    }

    /// <summary>
    /// Plays the game complete sound.
    /// </summary>
    public void PlayGameCompleteSound()
    {
        PlaySound(gameCompleteSound);
    }

    /// <summary>
    /// Plays the wrong placement sound.
    /// </summary>
    public void PlayWrongPlacementSound()
    {
        PlaySound(wrongPlacementSound);
    }

    /// <summary>
    /// Generic method to play a sound effect.
    /// </summary>
    /// <param name="clip">Audio clip to play.</param>
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Plays the background music in a loop at 30% volume.
    /// </summary>
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.volume = 0.3f; // 30% volume
            musicSource.Play();
        }
    }

    /// <summary>
    /// Stops the background music.
    /// </summary>
    public void StopBackgroundMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }
}
