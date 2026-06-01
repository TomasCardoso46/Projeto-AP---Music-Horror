using UnityEngine;

public class MenuAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioClip menuOpenSound;
    [SerializeField] private AudioClip menuMusic;

    public void OpenMenuAudio()
    {
        if (menuOpenSound != null)
            sfxSource.PlayOneShot(menuOpenSound);

        if (menuMusic != null)
        {
            musicSource.clip = menuMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void CloseMenuAudio()
    {
        musicSource.Stop();
    }
}