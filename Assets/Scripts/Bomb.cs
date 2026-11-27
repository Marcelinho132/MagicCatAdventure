using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField] private float flightTime = 0.9f;
    [SerializeField] private float arcHeight = 2f;
    [SerializeField] private float damageRadius = 0.7f;
    [SerializeField] private float fuseTime = 0.8f;
    [SerializeField] private string playerTag = "Player";

    private const int TelegraphTextureSize = 128;
    private const float TelegraphColorAlpha = 0.35f;

    private Vector2 startPos;
    private Vector2 targetPos;
    private float elapsed;
    private float fuseElapsed;
    private bool flying;
    private bool fuseLit;
    private GameObject telegraphObj;
    private Vector3 telegraphBaseScale;
    private static Sprite telegraphSprite;

    public void Throw(Vector2 target)
    {
        transform.SetParent(null);
        startPos = transform.position;
        targetPos = target;
        elapsed = 0f;
        flying = true;
    }

    private void Update()
    {
        if (flying)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flightTime);

            Vector2 pos = Vector2.Lerp(startPos, targetPos, t);
            pos.y += arcHeight * 4f * t * (1f - t);
            transform.position = pos;

            if (t >= 1f)
            {
                flying = false;
                LightFuse();
            }
        }
        else if (fuseLit)
        {
            fuseElapsed += Time.deltaTime;

            if (fuseElapsed >= fuseTime)
                Explode();
        }
    }

    private void LightFuse()
    {
        fuseLit = true;
        fuseElapsed = 0f;

        SpriteRenderer bombRenderer = GetComponent<SpriteRenderer>();
        int sortingOrder = bombRenderer != null ? bombRenderer.sortingOrder : 0;
        int sortingLayer = bombRenderer != null ? bombRenderer.sortingLayerID : 0;

        telegraphObj = new GameObject("Telegraph");
        telegraphObj.transform.position = transform.position;

        SpriteRenderer telegraphRenderer = telegraphObj.AddComponent<SpriteRenderer>();
        telegraphRenderer.sprite = GetTelegraphSprite();
        telegraphRenderer.sortingLayerID = sortingLayer;
        telegraphRenderer.sortingOrder = sortingOrder + 1;
        telegraphRenderer.color = new Color(1f, 0.3f, 0.3f, TelegraphColorAlpha);

        float diameter = damageRadius * 2f;
        telegraphBaseScale = Vector3.one * diameter;
        telegraphObj.transform.localScale = telegraphBaseScale;
    }

    private void Explode()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null &&
            Vector2.Distance(transform.position, playerObj.transform.position) <= damageRadius)
        {
            PlayerHearts hearts = playerObj.GetComponent<PlayerHearts>();
            if (hearts != null)
                hearts.LoseHeart();
        }

        SpawnExplosion();
        if (telegraphObj != null)
            Destroy(telegraphObj);

        Destroy(gameObject);
    }

    private static Sprite GetTelegraphSprite()
    {
        if (telegraphSprite != null) return telegraphSprite;

        int size = TelegraphTextureSize;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = (size - 1) / 2f;
        float outerRadius = size / 2f;
        float innerRadius = outerRadius - 3f;

        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = distance <= innerRadius ? 0.6f : distance <= outerRadius ? 1f : 0f;
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;

        telegraphSprite = Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            TelegraphTextureSize);

        return telegraphSprite;
    }

    private void SpawnExplosion()
    {
        SpriteRenderer tr = telegraphObj != null ? telegraphObj.GetComponent<SpriteRenderer>() : null;
        GameObject fx = new GameObject("Explosion");
        fx.transform.position = transform.position;
        fx.AddComponent<ExplosionEffect>().Init(
            GetTelegraphSprite(),
            damageRadius,
            tr != null ? tr.sortingLayerID : 0,
            tr != null ? tr.sortingOrder + 1 : 1);
    }

    private void OnDestroy()
    {
        if (telegraphObj != null) Destroy(telegraphObj);
    }
}