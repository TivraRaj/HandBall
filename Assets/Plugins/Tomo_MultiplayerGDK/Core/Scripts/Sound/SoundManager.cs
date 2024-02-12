using UnityEngine.UI;
using UnityEngine;
using System;

public static class SoundMessages
{
    public static Action<AudioClip> PlayMusic;
    public static Action<AudioClip> PlaySFX;
    public static Action PlayClickSFX;
    public static Action StopMusic;
    public static Action<AudioClip> PlayTTS;
    public static Action StopTTS;
}

public class SoundManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioSource ttsSource; //tts -> text to speech

    [Header("Audio UI")]
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider sfxVolumeSlider;
    [SerializeField] Slider ttsVolumeSlider;

    [Header("Sound Base Settings")]
    [SerializeField] private float baseMusicSound;
    [SerializeField] private float baseSFXSound;
    [SerializeField] private float baseTTSSound;

    [Header("Common Sounds")]
    [SerializeField] private AudioClip clickSound; 


    private void Awake()
    {
        VolumeInit();

        SoundMessages.PlayMusic += PlayMusic;
        SoundMessages.PlaySFX += PlaySFX;
        SoundMessages.PlayClickSFX += PlayClickSound;
        SoundMessages.StopMusic += StopMusic;
        SoundMessages.PlayTTS += PlayTTS;
        SoundMessages.StopTTS += StopTTS;

        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
		sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        ttsVolumeSlider.onValueChanged.AddListener(SetTTSVolume);
	}

	private void OnDestroy()
    {
        VolumeDeInit();

        SoundMessages.PlayMusic -= PlayMusic;
        SoundMessages.PlaySFX -= PlaySFX;
        SoundMessages.PlayClickSFX -= PlayClickSound;
        SoundMessages.StopMusic -= StopMusic;
        SoundMessages.PlayTTS -= PlayTTS;
        SoundMessages.StopTTS -= StopTTS;

        musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.RemoveListener(SetSFXVolume);
        ttsVolumeSlider.onValueChanged.RemoveListener(SetTTSVolume);
    }

    private void VolumeInit()
    {
        //Music Volume Init
        musicSource.volume = PlayerPrefs.GetFloat("Music Volume", baseMusicSound);
        musicVolumeSlider.value = musicSource.volume;

        //SFX Volume Init
        sfxSource.volume = PlayerPrefs.GetFloat("SFX Volume", baseSFXSound);
        sfxVolumeSlider.value = sfxSource.volume;

        //TTS Volume Init
        ttsSource.volume = PlayerPrefs.GetFloat("TTS Volume", baseTTSSound);
        ttsVolumeSlider.value = ttsSource.volume;

    }

    private void VolumeDeInit()
    {
        PlayerPrefs.SetFloat("Music Volume", musicSource.volume);
        PlayerPrefs.SetFloat("SFX Volume", sfxSource.volume);
        PlayerPrefs.SetFloat("TTS Volume", ttsSource.volume);
    }    

    private void SetMusicVolume(float changedVolume)
    {
        musicSource.volume = changedVolume;

    }

    private void SetSFXVolume(float changedVolume)
    {
        sfxSource.volume = changedVolume;
    }

    private void SetTTSVolume(float changedVolume)
	{
        ttsSource.volume = changedVolume;
	}

    private void PlayMusic(AudioClip musicClip)
    {
        if(musicSource.isPlaying) musicSource.Stop();
        musicSource.clip = musicClip;
        musicSource.Play();

    }

    private void PlaySFX(AudioClip audioClip)
    {
        sfxSource.PlayOneShot(audioClip);
    }

    private void PlayClickSound()
	{
        PlaySFX(clickSound);
	}

    private void StopMusic() => musicSource.Stop();

    private void PlayTTS(AudioClip ttsClip)
	{
        if (ttsSource.isPlaying) ttsSource.Stop();
        ttsSource.clip = ttsClip;
        ttsSource.Play();
    }

    private void StopTTS() => ttsSource.Stop();
}
