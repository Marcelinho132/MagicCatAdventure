using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    private const float Duration = 0.35f;
    private float maxScale;
    private float elapsed;
    private SpriteRenderer sr;
    private readonly Color startColor = new Color(1f, 0.6f, 0.1f, 0.9f);
    private readonly Color endColor = new Color(0.3f, 0.3f, 0.3f, 0f);

    public void Init(Sprite sprite, float radius, int sortingLayerId, int sortingOrder)
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerID = sortingLayerId;
        sr.sortingOrder = sortingOrder;
        sr.color = startColor;
        maxScale = radius * 2f * 1.3f;
        transform.localScale = Vector3.one * 0.2f;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / Duration);
        float eased = 1f - (1f - t) * (1f - t);

        transform.localScale = Vector3.one * Mathf.Lerp(0.2f, maxScale, eased);
        sr.color = Color.Lerp(startColor, endColor, t);

        if (t >= 1f)
            Destroy(gameObject);
    }
}