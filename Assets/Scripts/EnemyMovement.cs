using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopDistance = 0.8f;
    [SerializeField] private float retreatDistance = 0f;
    [SerializeField] private bool approachFromSide = false;
    [SerializeField] private float sideStandoffDistance = 1.1f;
    [SerializeField] private float enemySeparationDistance = 0.6f;
    [SerializeField] private float playerSeparationDistance = 0.45f;
    [SerializeField] private float separationDamping = 10f;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");

    private Transform player;
    private Transform[] enemies;
    private float nextScanTime;
    private float sideSign = -1f;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            player = playerObj.transform;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (player == null) return;

        if (Time.time >= nextScanTime)
        {
            ScanEnemies();
            nextScanTime = Time.time + 0.5f;
        }

        Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
        float distance = toPlayer.magnitude;

        Vector2 moveDirection = Vector2.zero;
        bool moving = false;

        if (approachFromSide)
        {
            if (Mathf.Abs(toPlayer.x) > 0.3f)
                sideSign = toPlayer.x > 0 ? -1f : 1f;

            Vector2 sideTarget = new Vector2(
                player.position.x + sideSign * sideStandoffDistance,
                player.position.y);

            Vector2 toSideTarget = sideTarget - (Vector2)transform.position;
            if (toSideTarget.magnitude > 0.1f)
            {
                moveDirection = toSideTarget.normalized;
                moving = true;
            }
        }
        else if (retreatDistance > 0f && distance < retreatDistance && distance > 0.01f)
        {
            moveDirection = -toPlayer.normalized;
            moving = true;
        }
        else if (distance > stopDistance)
        {
            moveDirection = toPlayer.normalized;
            moving = true;
        }

        if (moving)
            transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);

        ApplySeparation();

        if (spriteRenderer != null && Mathf.Abs(toPlayer.x) > 0.01f)
            spriteRenderer.flipX = toPlayer.x < 0;

        if (animator != null)
            animator.SetBool(IsMovingHash, moving);
    }

    private void ScanEnemies()
    {
        GameObject[] found = GameObject.FindGameObjectsWithTag(enemyTag);
        enemies = new Transform[found.Length];
        int count = 0;
        foreach (GameObject e in found)
        {
            if (e.transform != transform)
                enemies[count++] = e.transform;
        }
        if (count < enemies.Length)
            System.Array.Resize(ref enemies, count);
    }

    private void ApplySeparation()
    {
        Vector2 push = Vector2.zero;

        if (enemies != null)
        {
            foreach (Transform e in enemies)
            {
                if (e == null) continue;
                Vector2 d = (Vector2)transform.position - (Vector2)e.position;
                float dist = d.magnitude;
                if (dist > 0.001f && dist < enemySeparationDistance)
                    push += d / dist * (enemySeparationDistance - dist);
            }
        }

        Vector2 toPlayer = (Vector2)transform.position - (Vector2)player.position;
        float playerDist = toPlayer.magnitude;
        if (playerDist > 0.001f && playerDist < playerSeparationDistance)
            push += toPlayer / playerDist * (playerSeparationDistance - playerDist);

        if (push.sqrMagnitude > 0.0001f)
            transform.position += (Vector3)(push * separationDamping * Time.deltaTime);
    }
}
