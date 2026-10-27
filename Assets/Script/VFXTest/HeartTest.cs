using System.Reflection;
using UnityEngine;

// Sandbox-only stand-in for the real heart system, so the hit VFX can be play-tested in VFX_Test.
// Each ghost attack (or the H key) costs a heart, plays the blood effects and steps up the injury look.
// Doesn't change the ghost scripts: it only reads whether a ghost has just started an attack.
public class HeartTest : MonoBehaviour
{
    [Header("Hearts")]
    public int maxHearts = 5;
    [Tooltip("After a hit, further hits are ignored for this long.")]
    public float invulnerableTime = 1f;

    [Header("Effects")]
    public ParticleSystem bloodScreen;
    public ParticleSystem bloodBody;
    public Animator injuryAnimator;
    [Tooltip("How long the injury look takes to blend to the new heart level.")]
    public float injuryBlendTime = 0.5f;

    [Header("Test Keys")]
    public KeyCode hitKey = KeyCode.H;
    public KeyCode resetKey = KeyCode.R;

    private int hearts;
    private float injury;
    private float lastHitTime = -999f;

    private GhostAI[] ghosts;
    private bool[] ghostWasAttacking;
    private FieldInfo attackCooldownField;

    void Start()
    {
        hearts = maxHearts;
        ghosts = FindObjectsByType<GhostAI>(FindObjectsSortMode.None);
        ghostWasAttacking = new bool[ghosts.Length];
        // GhostAI sets attackCooldown the moment it attacks; read it to know when a hit lands.
        attackCooldownField = typeof(GhostAI).GetField("attackCooldown", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    void Update()
    {
        if (Input.GetKeyDown(hitKey))
            TakeHit();
        if (Input.GetKeyDown(resetKey))
            ResetHearts();

        for (int i = 0; i < ghosts.Length; i++)
        {
            if (ghosts[i] == null || attackCooldownField == null)
                continue;

            bool attacking = (bool)attackCooldownField.GetValue(ghosts[i]);
            if (attacking && !ghostWasAttacking[i])
                TakeHit();
            ghostWasAttacking[i] = attacking;
        }

        // Ease the injury look towards the current heart level.
        float target = maxHearts > 1 ? (maxHearts - Mathf.Max(hearts, 1)) / (float)(maxHearts - 1) : 0f;
        injury = Mathf.MoveTowards(injury, target, Time.deltaTime / Mathf.Max(0.01f, injuryBlendTime));
        if (injuryAnimator != null)
            injuryAnimator.SetFloat("Injury", injury);
    }

    public void TakeHit()
    {
        if (hearts <= 0 || Time.time - lastHitTime < invulnerableTime)
            return;

        lastHitTime = Time.time;
        hearts--;

        if (bloodScreen != null)
            bloodScreen.Play();
        if (bloodBody != null)
            bloodBody.Play();
    }

    public void ResetHearts()
    {
        hearts = maxHearts;
    }

    void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold };
        style.normal.textColor = new Color(0.85f, 0.1f, 0.1f);
        string row = new string('♥', Mathf.Max(hearts, 0)) + new string('♡', maxHearts - Mathf.Max(hearts, 0));
        GUI.Label(new Rect(20, 16, 400, 40), row, style);

        var hint = new GUIStyle(GUI.skin.label) { fontSize = 14 };
        hint.normal.textColor = new Color(1f, 1f, 1f, 0.7f);
        GUI.Label(new Rect(22, 52, 500, 24), hearts > 0 ? "VFX test: " + hitKey + " = take a hit, " + resetKey + " = reset hearts" : "Dead - press " + resetKey + " to reset", hint);
    }
}
