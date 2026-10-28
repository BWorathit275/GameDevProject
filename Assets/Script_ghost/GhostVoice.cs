using UnityEngine;

// เสียงร้องของผีแบบเว้นช่วง: เล่นทีละเสียง สุ่มคลิป/ความสูงเสียง/ความดัง แล้วเว้นระยะสุ่มก่อนเสียงถัดไป
// ใช้แทนการเล่นวนลูปตลอดเวลา จะได้ไม่น่ารำคาญ
[System.Serializable]
public class GhostVoice
{
    [Tooltip("คลิปที่สุ่มเล่น (ไม่ซ้ำคลิปเดิมติดกัน)")]
    public AudioClip[] clips;
    [Tooltip("ช่วงเงียบระหว่างเสียง (วินาที) ต่ำสุด-สูงสุด หลังเสียงก่อนหน้าจบ")]
    public Vector2 gap = new Vector2(3f, 7f);
    public Vector2 pitchRange = new Vector2(0.9f, 1.1f);
    public Vector2 volumeRange = new Vector2(0.8f, 1f);

    private bool active;
    private float nextTime;
    private int lastClip = -1;
    private float lastEnd = -999f;   // เวลาที่เสียงล่าสุดเล่นจบ

    public bool HasClips => clips != null && clips.Length > 0;

    // playNow = เล่นทันที (เช่นตอนเห็นผู้เล่น) ไม่งั้นรอช่วงสุ่มก่อน
    public void Begin(AudioSource source, bool playNow)
    {
        if (active)
            return;

        active = true;

        // เพิ่งร้องไปไม่นาน (เช่นผู้เล่นโผล่ๆ หายๆ) ไม่ต้องร้องซ้ำทันที
        if (playNow && Time.time >= lastEnd + gap.x * 0.5f)
            Play(source);
        else if (playNow)
            nextTime = Mathf.Max(nextTime, lastEnd + gap.x);
        else
            // ไม่เลื่อนให้เร็วกว่ากำหนดเดิม (กันเสียงซ้อน/ถี่ตอนหยุด-เดินสลับกัน)
            nextTime = Mathf.Max(nextTime, Time.time + Random.Range(gap.x, gap.y) * 0.5f);
    }

    // cut = ตัดเสียงที่กำลังเล่นทันที, ไม่งั้นปล่อยให้เล่นจนจบ
    public void End(AudioSource source, bool cut = true)
    {
        if (!active)
            return;

        active = false;

        if (cut && source != null)
        {
            source.Stop();
            lastEnd = Mathf.Min(lastEnd, Time.time);
        }
    }

    public void Tick(AudioSource source)
    {
        if (active && Time.time >= nextTime)
            Play(source);
    }

    void Play(AudioSource source)
    {
        if (source == null || !HasClips)
        {
            nextTime = Time.time + gap.x;
            return;
        }

        int index = Random.Range(0, clips.Length);
        if (clips.Length > 1 && index == lastClip)
            index = (index + 1) % clips.Length;
        lastClip = index;

        AudioClip clip = clips[index];
        if (clip == null)
        {
            nextTime = Time.time + gap.x;
            return;
        }

        float pitch = Random.Range(pitchRange.x, pitchRange.y);
        source.pitch = pitch;
        source.clip = clip;
        source.volume = Random.Range(volumeRange.x, volumeRange.y);
        source.Play();

        lastEnd = Time.time + clip.length / pitch;
        nextTime = lastEnd + Random.Range(gap.x, gap.y);
    }
}
