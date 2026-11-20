using UnityEngine;

public class MageShockwave : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float hitRadius = 0.5f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private string playerTag = "Player";

    private Vector2 moveDirection;
    private SpriteRenderer spriteRenderer;
    private Transform player;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetBolt(Sprite boltSprite)
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = boltSprite;
    }

    public void Cast(Vector2 targetPosition)
    {
        transform.SetParent(null);
        moveDirection = (targetPosition - (Vector2)transform.position).normalized;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);

        Transform p = GetPlayer();
        if (p != null && Vector2.Distance(transform.position, p.position) <= hitRadius)
        {
            PlayerHearts hearts = p.GetComponent<PlayerHearts>();
            if (hearts != null)
                hearts.LoseHeart();
            Destroy(gameObject);
        }
    }

    private Transform GetPlayer()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
                player = playerObj.transform;
        }
        return player;
    }
}