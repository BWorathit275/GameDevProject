using UnityEngine;
using UnityEngine.UI;

public class MouseSettings : MonoBehaviour
{
    public Slider sensitivitySlider;

    private const string SENS_KEY = "MouseSensitivity";

    private void Start()
    {
        if (sensitivitySlider == null) return;

        sensitivitySlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(SENS_KEY, 2f));
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
    }

    public void OnSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat(SENS_KEY, value);
        PlayerPrefs.Save();
    }
}