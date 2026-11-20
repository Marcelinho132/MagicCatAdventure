using UnityEngine;

public class Spell : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private int damage = 1;
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private bool destroyOnHit = true;
    [SerializeField] private bool canHitPlayer = false;
    [SerializeField] private float playerHitRadius = 0.35f;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool canHitEnemy = true;
    [SerializeField] private float enemyHitRadius = 0.4f;

    private Vector2 moveDirection;
    private bool launched;
    private Transform player;

    public void Launch(Vector2 direction)
    {
        transform.SetParent(null);

        moveDirection = direction.normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        launched = true;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!launched) return;
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);

        if (canHitPlayer && CheckPlayerHit())
        {
            if (destroyOnHit) Destroy(gameObject);
        }

        if (canHitEnemy && CheckEnemyHit())
        {
            if (destroyOnHit) Destroy(gameObject);
        }
    }

    private bool CheckEnemyHit()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        foreach (GameObject enemy in enemies)
        {
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health == null) continue;

            if (Vector2.Distance(transform.position, health.HitCenter) > enemyHitRadius)
                continue;

            health.TakeDamage(damage);
            return true;
        }

        return false;
    }

    private bool CheckPlayerHit()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj == null) return false;
            player = playerObj.transform;
        }

        if (Vector2.Distance(transform.position, player.position) > playerHitRadius)
            return false;

        PlayerHearts hearts = player.GetComponent<PlayerHearts>();
        if (hearts != null)
            hearts.LoseHeart();

        return true;
    }
}