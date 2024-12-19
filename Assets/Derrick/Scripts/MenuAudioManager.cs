using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuAudioManager : MonoBehaviour
{
    private static MenuAudioManager instance;
    private AudioSource audioSource;
    private AudioSource musicSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
            audioSource.Play();
            SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to the sceneLoaded event
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if the loaded scene is the GameScene
        if (scene.name == "GameScene")
        {
            audioSource.Stop(); // Stop the music in the GameScene
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe to avoid memory leaks
    }

    public void MuteAudio(bool mute)
    {
        audioSource.mute = mute;
    }

        public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume; // Set the volume of the music
    }

    public void SetSFXVolume(float volume)
    {
    sfxSource.volume = volume; // Set the volume for SFX
    }
}
