using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider musicSlider;

    private const string MusicKey = "MusicVolume";

    void Start()
    {
        float volume = PlayerPrefs.GetFloat(MusicKey, 0.8f);

        musicSlider.SetValueWithoutNotify(volume);
        SetMusicVolume(volume);
    }

    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        float dB = volume <= 0.0001f
            ? -80f
            : Mathf.Log10(volume) * 20f;

        audioMixer.SetFloat("MusicVolume", dB);
        PlayerPrefs.SetFloat(MusicKey, volume);
        PlayerPrefs.Save();
    }
}