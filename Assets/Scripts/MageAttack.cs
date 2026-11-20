using UnityEngine;

public class MageAttack : MonoBehaviour
{
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private Sprite[] boltSprites;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float attackRange = 7f;
    [SerializeField] private float cooldown = 1.2f;
    [SerializeField] private float spreadDistance = 0.45f;
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
        if (player == null || spellPrefab == null) return;
        if (Time.time < nextAttackTime) return;

        Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
        if (toPlayer.magnitude > attackRange) return;

        Attack(player.position);
    }

    private void Attack(Vector2 targetPosition)
    {
        nextAttackTime = Time.time + cooldown;

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
        Vector2 baseDirection = (targetPosition - (Vector2)spawnPos).normalized;
        Vector2 perpendicular = new Vector2(-baseDirection.y, baseDirection.x);

        int count = boltSprites != null && boltSprites.Length > 0 ? boltSprites.Length : 1;
        for (int i = 0; i < count; i++)
        {
            float offset = count > 1 ? (i - (count - 1) / 2f) * spreadDistance : 0f;
            Vector2 boltTarget = targetPosition + perpendicular * offset;

            GameObject spellObj = Instantiate(spellPrefab, spawnPos, Quaternion.identity);

            MageShockwave shockwave = spellObj.GetComponent<MageShockwave>();
            if (shockwave == null)
            {
                Debug.LogError("No MageShockwave prefab!");
                Destroy(spellObj);
                continue;
            }

            if (boltSprites != null && i < boltSprites.Length)
                shockwave.SetBolt(boltSprites[i]);

            shockwave.Cast(boltTarget);
        }
    }
}
