using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider volumeSlider;

    private const string VolumeKey = "MasterVolume";

    void Start()
    {
        float volume = PlayerPrefs.GetFloat(VolumeKey, 0.8f);

        if (volumeSlider != null)
            volumeSlider.SetValueWithoutNotify(volume);

        SetVolume(volume);
    }

    public void SetVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        float dB = volume <= 0.0001f
            ? -80f
            : Mathf.Log10(volume) * 20f;

        Debug.Log($"volume={volume}, dB={dB}");

        if (!audioMixer.SetFloat("MasterVolume", dB))
            Debug.LogError("หา exposed parameter ชื่อ MasterVolume ไม่เจอ!");

        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();
    }
}