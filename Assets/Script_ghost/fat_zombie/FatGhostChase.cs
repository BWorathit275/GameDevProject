using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FatGhostChase : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Chase Settings")]
    public float chaseRange = 8f;
    public float chaseSpeed = 3.5f;

    [Header("Attack Settings")]
    public float attackRange = 2.5f;
    public float attackCooldown = 2f;
    public float pauseMovementAfterAttack = 1.5f;
    [SerializeField] GhostAttack attack;

    [Header("Attack Sound")]
    public AudioSource attackAudioSource;
    public AudioClip attackSound;
    public float attackSoundDelay = 0.38f;

    [Header("References")]
    public FatGhostAnimation fatGhostAnimation;
    public FatGhostPatrol fatGhostPatrol;
    public FatGhostSound fatGhostSound;
    public Animator animator;

    private NavMeshAgent agent;
    private float patrolSpeed; // ความเร็วเดิมตอนลาดตระเวน ไว้คืนค่าหลังเลิกไล่
    private bool isChasing;

    // ให้สคริปต์อื่น (เช่นเสียงบรรยากาศ) รู้ว่าผีกำลังไล่อยู่
    public bool IsChasing => isChasing;
    private bool isAttacking;
    private bool isPausingAfterAttack;
    private float nextAttackTime;
    private Coroutine attackSoundCoroutine;
    private Coroutine pauseCoroutine;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
            patrolSpeed = agent.speed;

        if (fatGhostAnimation == null)
            fatGhostAnimation = GetComponent<FatGhostAnimation>();

        if (fatGhostPatrol == null)
            fatGhostPatrol = GetComponent<FatGhostPatrol>();

        if (fatGhostSound == null)
            fatGhostSound = GetComponent<FatGhostSound>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (agent == null || player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        // ซ่อนในตู้อยู่ ผีเลิกไล่
        if (distance <= chaseRange && !PlayerHide.IsPlayerHidden(player))
        {
            if (!isChasing)
            {
                isChasing = true;

                if (fatGhostPatrol != null)
                    fatGhostPatrol.enabled = false;

                agent.isStopped = false;

                if (fatGhostSound != null)
                    fatGhostSound.PlayScream();
            }

            agent.speed = chaseSpeed;

            if (distance <= attackRange)
            {
                agent.isStopped = true;

                if (!isAttacking &&
                    !isPausingAfterAttack &&
                    Time.time >= nextAttackTime)
                {
                    attack.TryAttack(player);
                    isAttacking = true;
                    nextAttackTime = Time.time + attackCooldown;

                    if (animator != null)
                        animator.SetTrigger("Attack_fat");

                    if (attack != null)
                        attack.TryAttack(player);

                    if (attackSoundCoroutine != null)
                        StopCoroutine(attackSoundCoroutine);

                    attackSoundCoroutine = StartCoroutine(
                        PlayAttackSoundDelayed()
                    );

                    if (pauseCoroutine != null)
                        StopCoroutine(pauseCoroutine);

                    pauseCoroutine = StartCoroutine(
                        PauseMovementAfterAttack()
                    );
                }
            }
            else
            {
                isAttacking = false;

                if (!isPausingAfterAttack)
                {
                    agent.isStopped = false;
                    agent.SetDestination(player.position);
                }

                if (attackSoundCoroutine != null)
                {
                    StopCoroutine(attackSoundCoroutine);
                    attackSoundCoroutine = null;
                }
            }
        }
        else if (isChasing)
        {
            isChasing = false;
            isAttacking = false;
            isPausingAfterAttack = false;

            if (pauseCoroutine != null)
            {
                StopCoroutine(pauseCoroutine);
                pauseCoroutine = null;
            }

            if (fatGhostSound != null)
                fatGhostSound.StopScream();

            if (attackSoundCoroutine != null)
            {
                StopCoroutine(attackSoundCoroutine);
                attackSoundCoroutine = null;
            }

            if (fatGhostAnimation != null)
                fatGhostAnimation.SetChasing(false);

            if (fatGhostPatrol != null)
                fatGhostPatrol.enabled = true;

            agent.speed = patrolSpeed;
            agent.isStopped = false;
        }

        if (fatGhostAnimation != null)
            fatGhostAnimation.SetChasing(isChasing);
    }

    private IEnumerator PauseMovementAfterAttack()
    {
        isPausingAfterAttack = true;

        // รอให้แอนิเมชันข่วนเล่นไปก่อน
        yield return new WaitForSeconds(0.8f);

        // หยุดการเคลื่อนที่ของผี
        if (agent != null)
            agent.isStopped = true;

        // หยุดนิ่งตามเวลาที่กำหนด
        yield return new WaitForSeconds(pauseMovementAfterAttack);

        isPausingAfterAttack = false;
        isAttacking = false;

        // ถ้ายังไล่ผู้เล่นอยู่ ให้กลับมาเคลื่อนที่
        if (agent != null && isChasing)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }

        pauseCoroutine = null;
    }

    private IEnumerator PlayAttackSoundDelayed()
    {
        yield return new WaitForSeconds(attackSoundDelay);

        if (attackAudioSource != null && attackSound != null)
            attackAudioSource.PlayOneShot(attackSound, 0.8f);

        attackSoundCoroutine = null;
    }
}