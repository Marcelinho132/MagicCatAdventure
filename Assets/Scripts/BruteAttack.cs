using System.Collections;
using UnityEngine;

public class BruteAttack : MonoBehaviour
{
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float verticalTolerance = 0.8f;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private float attackAnimDuration = 0.6f;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private string attackLeftTrigger = "AttackLeft";
    [SerializeField] private string attackRightTrigger = "AttackRight";
    [SerializeField] private GameObject leftHitbox;
    [SerializeField] private GameObject rightHitbox;

    private static readonly int AttackLeftHash = Animator.StringToHash("AttackLeft");
    private static readonly int AttackRightHash = Animator.StringToHash("AttackRight");

    private Transform player;
    private EnemyMovement enemyMovement;
    private float nextAttackTime;
    private bool hasAttackLeft;
    private bool hasAttackRight;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        enemyMovement = GetComponent<EnemyMovement>();

        if (animator != null)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == attackLeftTrigger) hasAttackLeft = true;
                if (param.name == attackRightTrigger) hasAttackRight = true;
            }
        }

        DisableHitbox();
    }

    private void Update()
    {
        if (player == null) return;
        if (Time.time < nextAttackTime) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > attackRange) return;

        Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
        float dx = Mathf.Abs(toPlayer.x);
        float dy = Mathf.Abs(toPlayer.y);

        if (dx < dy || dy > verticalTolerance) return;

        Attack();
    }

    private void Attack()
    {
        nextAttackTime = Time.time + cooldown;

        bool playerOnLeft = player.position.x < transform.position.x;
        if (playerOnLeft && hasAttackLeft)
            animator.SetTrigger(AttackLeftHash);
        else if (!playerOnLeft && hasAttackRight)
            animator.SetTrigger(AttackRightHash);

        if (enemyMovement != null)
            StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        DisableHitbox();
        enemyMovement.enabled = false;

        if (spriteRenderer != null)
            spriteRenderer.flipX = false;

        yield return new WaitForSeconds(attackAnimDuration);

        enemyMovement.enabled = true;
    }

    public void EnableHitbox(string side)
    {
        if (side == "Left" && leftHitbox != null) leftHitbox.SetActive(true);
        if (side == "Right" && rightHitbox != null) rightHitbox.SetActive(true);
    }

    public void DisableHitbox()
    {
        if (leftHitbox != null) leftHitbox.SetActive(false);
        if (rightHitbox != null) rightHitbox.SetActive(false);
    }
}