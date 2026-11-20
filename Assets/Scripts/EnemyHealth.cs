using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHp = 3;
    [SerializeField] private float hitFlashTime = 0.1f;
    [SerializeField] private float fallTime = 0.4f;
    [SerializeField] private float lieTime = 0.5f;
    [SerializeField] private float fadeTime = 0.6f;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Vector2 hitOffset = new Vector2(0f, 0.5f);
    public Vector2 HitCenter => (Vector2)transform.position + hitOffset;

    private bool isDead;

    private int currentHp;
    private SpriteRenderer spriteRenderer;
    private Color baseColor;

    private void Start()
    {
        currentHp = maxHp;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            baseColor = spriteRenderer.color;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die();
            return;
        }

        if (spriteRenderer != null)
            StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(hitFlashTime);
        if (spriteRenderer != null)
            spriteRenderer.color = baseColor;
    }

    private void Die()
    {
        isDead = true;
        StopAllCoroutines();
        gameObject.tag = "Untagged"; // spells and separation checks ignore it from now on

        foreach (MonoBehaviour mb in GetComponents<MonoBehaviour>())
            if (mb != this) mb.enabled = false;

        foreach (Collider2D c in GetComponentsInChildren<Collider2D>())
            c.enabled = false;

        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.enabled = false;

        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        // Fall away from the player (+90 = falls left, -90 = falls right)
        float angle = 90f;
        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null)
            angle = p.transform.position.x > transform.position.x ? 90f : -90f;

        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(0f, 0f, angle);

        for (float t = 0f; t < fallTime; t += Time.deltaTime)
        {
            transform.rotation = Quaternion.Lerp(startRot, endRot, t / fallTime);
            yield return null;
        }
        transform.rotation = endRot;

        yield return new WaitForSeconds(lieTime);

        for (float t = 0f; t < fadeTime; t += Time.deltaTime)
        {
            if (spriteRenderer != null)
            {
                Color c = baseColor;
                c.a = Mathf.Lerp(1f, 0f, t / fadeTime);
                spriteRenderer.color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}