
using UnityEngine;
using System.Collections;

public class ThinGhostSound : MonoBehaviour
{
    [Header("Walk Sound (ครางเป็นช่วงๆ ตอนเดิน)")]
    public AudioSource walkSource;
    public AudioClip walkSound;
    [Tooltip("Walk Sound จะถูกใส่รวมใน Clips ให้อัตโนมัติ ใส่คลิปเพิ่มได้")]
    public GhostVoice walkVoice = new GhostVoice { gap = new Vector2(4f, 9f) };

    [Header("Scream Sound (กรีดร้องเป็นช่วงๆ ตอนไล่)")]
    public AudioSource screamSource;
    public AudioClip screamSound;
    [Tooltip("Scream Sound จะถูกใส่รวมใน Clips ให้อัตโนมัติ ใส่คลิปเพิ่มได้")]
    public GhostVoice screamVoice = new GhostVoice { gap = new Vector2(1.5f, 4f) };

    [Header("Search Sound (คำรามตอนมองหาหลังคลาดสายตา)")]
    public GhostVoice searchVoice = new GhostVoice { gap = new Vector2(2f, 5f) };

    [Header("Attack Sound")]
    public AudioSource attackSource;
    public AudioClip attackSound;
    public float attackSoundDelay = 0.38f;

    private Coroutine attackCoroutine;
    private bool searching;

    void Awake()
    {
        SetupSource(ref walkSource, false);
        SetupSource(ref screamSource, false);
        SetupSource(ref attackSource, false);

        AddClip(walkVoice, walkSound);
        AddClip(screamVoice, screamSound);
    }

    // ใส่คลิปเดี่ยวเดิมรวมเข้าไปในรายการสุ่ม (ถ้ายังไม่มี)
    static void AddClip(GhostVoice voice, AudioClip clip)
    {
        if (clip == null)
            return;

        if (voice.clips == null)
            voice.clips = new AudioClip[0];

        if (System.Array.IndexOf(voice.clips, clip) >= 0)
            return;

        AudioClip[] list = new AudioClip[voice.clips.Length + 1];
        list[0] = clip;
        voice.clips.CopyTo(list, 1);
        voice.clips = list;
    }

    void Update()
    {
        screamVoice.Tick(screamSource);
        walkVoice.Tick(walkSource);
        searchVoice.Tick(walkSource);
    }

    // ช่วงค้นหาผู้เล่น: ใช้เสียงคำรามแทนเสียงครางตอนเดิน
    public void SetSearching(bool value)
    {
        if (searching == value)
            return;

        searching = value;

        if (searching)
        {
            walkVoice.End(walkSource);
            searchVoice.Begin(walkSource, true);
        }
        else
        {
            searchVoice.End(walkSource, false);
        }
    }

    void SetupSource(ref AudioSource source, bool shouldLoop)
    {
        if (source == null)
            source = gameObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.loop = shouldLoop;
        source.spatialBlend = 1f; // เสียง 3D ได้ยินตามระยะและทิศทางของผี
        source.volume = 1f;
        source.mute = false;
        source.enabled = true;
    }

    // เรียกได้ทุกเฟรม เสียงจะเล่นตามช่วงเวลาของ walkVoice
    public void StartWalk()
    {
        if (searching)
            return;

        walkVoice.Begin(walkSource, false);
    }

    // หยุดเดินชั่วคราว (เช่นรอที่จุดลาดตระเวน) ปล่อยเสียงครางที่ค้างอยู่ให้จบเอง
    public void StopWalk()
    {
        walkVoice.End(walkSource, false);
    }

    public void PlayScream()
    {
        if (!screamVoice.HasClips || screamSource == null)
        {
            Debug.LogWarning("ThinGhostSound: ยังไม่ได้กำหนด Scream Sound หรือ Source!", this);
            return;
        }

        // เริ่มไล่: ตัดเสียงครางทิ้ง ให้เสียงกรีดร้องเด่น
        if (walkSource != null && walkSource != screamSource)
            walkSource.Stop();

        // เรียกได้ทุกเฟรม: ร้องทันทีครั้งแรก แล้วร้องซ้ำเป็นช่วงๆ จนกว่าจะ StopScream
        screamVoice.Begin(screamSource, true);
    }

    public void StopScream()
    {
        screamVoice.End(screamSource);
    }

    public void PlayAttack()
    {
        if (attackSound == null || attackSource == null)
        {
            Debug.LogWarning("ThinGhostSound: ยังไม่ได้กำหนด Attack Sound หรือ Source!", this);
            return;
        }

        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);

        attackCoroutine = StartCoroutine(PlayAttackDelayed());
    }

    private IEnumerator PlayAttackDelayed()
    {
        yield return new WaitForSeconds(attackSoundDelay);

        if (attackSource != null && attackSound != null)
        {
            attackSource.PlayOneShot(attackSound, 1f);
            Debug.Log("ThinGhostSound: เล่นเสียง Attack", this);
        }

        attackCoroutine = null;
    }

    void OnDisable()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        SetSearching(false);
        StopWalk();
        StopScream();
    }
}
