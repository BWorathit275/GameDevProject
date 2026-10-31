using UnityEngine;
using UnityEngine.UI;

public class MouseSettings : MonoBehaviour
{
    public Slider sensitivitySlider;

    public const string SENS_KEY = "MouseSensitivity";
    public const float DEFAULT_SENS = 2f;

    // ค่าปัจจุบัน ให้สคริปต์อื่นอ่านได้จากทุกซีน
    public static float Sensitivity { get; private set; } = DEFAULT_SENS;

    // โหลดค่าที่เซฟไว้ตั้งแต่เปิดเกม ก่อนซีนแรกจะโหลด
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadSaved()
    {
        Sensitivity = PlayerPrefs.GetFloat(SENS_KEY, DEFAULT_SENS);
    }

    private void Start()
    {
        if (sensitivitySlider == null) return;

        sensitivitySlider.SetValueWithoutNotify(Sensitivity);
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
    }

    public void OnSensitivityChanged(float value)
    {
        Sensitivity = value;                       // มีผลกับตัวควบคุมทันที
        PlayerPrefs.SetFloat(SENS_KEY, value);     // ยังไม่ลงดิสก์ รอ Save ตอนปิดแผงตั้งค่า
    }

    private void OnDisable()
    {
        PlayerPrefs.Save();                        // เซฟครั้งเดียวตอนปิดแผง ไม่เซฟทุกครั้งที่ลากสไลเดอร์
    }
}