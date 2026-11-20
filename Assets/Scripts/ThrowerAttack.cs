using UnityEngine;

public class ThrowerAttack : MonoBehaviour
{
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float handReach = 0.5f;
    [SerializeField] private float handHeight = 0.3f;
    [SerializeField] private float attackRange = 6f;
    [SerializeField] private float cooldown = 1.5f;
    [SerializeField] private string playerTag = "Player";

    private Transform player;
    private float nextAttackTime;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Update()
    {
        if (player == null || bombPrefab == null) return;
        if (Time.time < nextAttackTime) return;

        Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
        if (toPlayer.magnitude > attackRange) return;

        Attack();
    }

    private void Attack()
    {
        nextAttackTime = Time.time + cooldown;

        Vector3 spawnPos;
        if (spawnPoint != null)
        {
            float side = player.position.x >= transform.position.x ? 1f : -1f;
            Vector3 lp = spawnPoint.localPosition;
            lp.x = Mathf.Abs(lp.x) * side;
            spawnPoint.localPosition = lp;
            spawnPos = spawnPoint.position;
        }
        else
        {
            Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
            float side = toPlayer.x >= 0 ? 1f : -1f;
            spawnPos = transform.position + new Vector3(side * handReach, handHeight, 0f);
        }
        GameObject bombObj = Instantiate(bombPrefab, spawnPos, Quaternion.identity);

        Bomb bomb = bombObj.GetComponent<Bomb>();
        if (bomb == null)
        {
            Debug.LogError("No bomb script on the bomb prefab!");
            Destroy(bombObj);
            return;
        }

        bomb.Throw(player.position);
    }
}
