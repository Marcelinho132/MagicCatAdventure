using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class HazardEntry
{
    public string name;
    public GameObject prefab;
    public int minCount = 0;
    public int maxCount = 3;
    public float radius = 0.8f;
}

[System.Serializable]
public class EnemyEntry
{
    public string name;
    public GameObject prefab;
    public int unlockRoom = 1;
    public int startCount = 1;
    public float growthPerRoom = 0.5f; // extra enemies per room after unlockRoom
    public int maxCount = 15;
    public float radius = 0.8f;
}

[System.Serializable]
public class SpecialRoomEntry
{
    public string name;
    public GameObject prefab;
    [Range(1, 100)] public float chance = 10f; // percent
}

public class RoomManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private TMP_Text roomText;
    [SerializeField] private CanvasGroup fadeGroup; // full-screen black Image with a CanvasGroup
    [SerializeField] private Portal portalPrefab;

    [Header("Backgrounds")]
    [SerializeField] private List<Sprite> backgrounds = new List<Sprite>();
    [SerializeField] private Sprite easterEggBackground;
    [SerializeField, Range(0f, 100f)] private float easterEggChance = 1f;

    [Header("Spawning")]
    [SerializeField] private List<HazardEntry> hazards = new List<HazardEntry>();
    [SerializeField] private List<EnemyEntry> enemyTypes = new List<EnemyEntry>();
    [SerializeField] private float edgeMargin = 1f;
    [SerializeField] private int maxPlacementAttempts = 40;
    [SerializeField] private float playerSafeRadius = 3f;  // no spawns this close to the player's entry point
    [SerializeField] private float portalRadius = 1.5f;

    [Header("Special rooms")]
    [SerializeField] private List<SpecialRoomEntry> specialRooms = new List<SpecialRoomEntry>();
    [SerializeField] private int firstSpecialRoom = 2;

    [Header("Transition")]
    [SerializeField] private float fadeDuration = 0.6f;
    [SerializeField] private float blackHoldTime = 0.4f;
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float walkOutDistance = 2f; // keep below Player Safe Radius
    [SerializeField] private float minClearDelay = 1f;
    [SerializeField] private float maxClearDelay = 3f;

    private readonly List<EnemyHealth> enemies = new List<EnemyHealth>();
    private readonly List<GameObject> spawnedHazards = new List<GameObject>();
    private readonly List<(Vector2 pos, float radius)> occupied = new List<(Vector2, float)>();

    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");

    private Portal portal;
    private PlayerMovement playerMovement;
    private MageSpellAttack playerSpell;
    private Rigidbody2D playerRb;
    private Animator playerAnimator;
    private GameObject specialRoomInstance;
    private Sprite lastBackground;
    private Vector2 entryPoint, exitPoint, walkOutTarget;
    private int roomNumber;
    private bool isSpecialRoom;
    private bool portalEntered;

    private void Start()
    {
        playerMovement = player.GetComponent<PlayerMovement>();
        playerSpell = player.GetComponent<MageSpellAttack>();
        playerRb = player.GetComponent<Rigidbody2D>();
        playerAnimator = player.GetComponentInChildren<Animator>();

        portal = Instantiate(portalPrefab);
        portal.PlayerEntered += () => portalEntered = true;
        portal.Hide();

        StartCoroutine(RoomLoop());
    }

    private IEnumerator RoomLoop()
    {
        fadeGroup.alpha = 1f;

        while (true)
        {
            roomNumber++;
            SetPlayerControl(false);
            GenerateRoom();
            player.transform.position = entryPoint;

            yield return Fade(0f);

            // Player walks out of the portal while everything else is frozen
            yield return WalkTo(walkOutTarget, false);
            portal.Hide();
            SetEnemiesFrozen(false);
            SetPlayerControl(true);

            portalEntered = false;
            if (isSpecialRoom)
            {
                // Exit is open right away, the player leaves whenever they want
                portal.Show(exitPoint, true);
                while (!portalEntered) yield return null;
                SetPlayerControl(false);
            }
            else
            {
                while (AliveEnemies() > 0) yield return null;
                yield return new WaitForSeconds(Random.Range(minClearDelay, maxClearDelay));

                SetPlayerControl(false);
                portal.Show(exitPoint, true);
                yield return WalkTo(exitPoint, true);
            }

            yield return Fade(1f);
            // Later: play the transition animation here
            yield return new WaitForSeconds(blackHoldTime);
        }
    }

    // ---------- Generation ----------

    private void GenerateRoom()
    {
        ClearRoom();
        roomText.text = "RUME: " + roomNumber;

        lastBackground = PickBackground();
        background.sprite = lastBackground;

        occupied.Clear();
        Bounds b = background.bounds;

        if (!TryFindSpot(playerSafeRadius, out entryPoint))
            entryPoint = b.center;
        if (!TryFindSpot(portalRadius, out exitPoint))
            exitPoint = (Vector2)b.center * 2f - entryPoint;

        Vector2 dir = (Vector2)b.center - entryPoint;
        dir = dir.sqrMagnitude < 0.01f ? Vector2.down : dir.normalized;
        walkOutTarget = entryPoint + dir * walkOutDistance;

        SpecialRoomEntry special = roomNumber >= firstSpecialRoom ? RollSpecialRoom() : null;
        isSpecialRoom = special != null;

        if (isSpecialRoom)
        {
            specialRoomInstance = Instantiate(special.prefab, Vector3.zero, Quaternion.identity);
        }
        else
        {
            SpawnHazards();
            SpawnEnemies();
        }

        portal.Show(entryPoint, false);
        SetEnemiesFrozen(true);
    }

    private Sprite PickBackground()
    {
        bool canShowEgg = easterEggBackground != null && lastBackground != easterEggBackground;
        if (canShowEgg && Random.Range(0f, 100f) < easterEggChance)
            return easterEggBackground;

        if (backgrounds.Count == 0) return background.sprite;
        if (backgrounds.Count == 1) return backgrounds[0];

        Sprite pick;
        do { pick = backgrounds[Random.Range(0, backgrounds.Count)]; }
        while (pick == lastBackground);
        return pick;
    }

    private SpecialRoomEntry RollSpecialRoom()
    {
        int n = specialRooms.Count;
        if (n == 0) return null;

        int start = Random.Range(0, n);
        for (int i = 0; i < n; i++)
        {
            SpecialRoomEntry entry = specialRooms[(start + i) % n];
            if (entry.prefab != null && Random.Range(0f, 100f) < entry.chance)
                return entry;
        }
        return null;
    }

    private void SpawnHazards()
    {
        foreach (HazardEntry h in hazards)
        {
            if (h.prefab == null) continue;
            int count = Random.Range(h.minCount, h.maxCount + 1);
            for (int i = 0; i < count; i++)
            {
                GameObject obj = SpawnAt(h.prefab, h.radius);
                if (obj != null) spawnedHazards.Add(obj);
            }
        }
    }

    private void SpawnEnemies()
    {
        foreach (EnemyEntry e in enemyTypes)
        {
            if (e.prefab == null || roomNumber < e.unlockRoom) continue;

            int count = Mathf.FloorToInt(e.startCount + e.growthPerRoom * (roomNumber - e.unlockRoom));
            count = Mathf.Min(count, e.maxCount);
            for (int i = 0; i < count; i++)
                SpawnEnemy(e);
        }

        // Safety net: never create an empty combat room
        if (enemies.Count == 0)
        {
            foreach (EnemyEntry e in enemyTypes)
            {
                if (e.prefab != null && SpawnEnemy(e)) break;
            }
        }
    }

    private bool SpawnEnemy(EnemyEntry entry)
    {
        GameObject obj = SpawnAt(entry.prefab, entry.radius);
        if (obj == null) return false;

        EnemyHealth health = obj.GetComponent<EnemyHealth>();
        if (health == null)
        {
            Debug.LogError("Enemy prefab is missing the EnemyHealth script: " + entry.prefab.name);
            Destroy(obj);
            return false;
        }

        enemies.Add(health);
        return true;
    }

    private GameObject SpawnAt(GameObject prefab, float radius)
    {
        if (!TryFindSpot(radius, out Vector2 pos)) return null;
        return Instantiate(prefab, pos, Quaternion.identity);
    }

    // Rejection sampling: random point inside the background that doesn't overlap anything already placed
    private bool TryFindSpot(float radius, out Vector2 result)
    {
        Bounds b = background.bounds;
        float pad = radius + edgeMargin;

        for (int i = 0; i < maxPlacementAttempts; i++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(b.min.x + pad, b.max.x - pad),
                Random.Range(b.min.y + pad, b.max.y - pad));

            bool free = true;
            foreach (var o in occupied)
            {
                if (Vector2.Distance(candidate, o.pos) < radius + o.radius)
                {
                    free = false;
                    break;
                }
            }

            if (free)
            {
                occupied.Add((candidate, radius));
                result = candidate;
                return true;
            }
        }

        result = default;
        return false;
    }

    private void ClearRoom()
    {
        foreach (EnemyHealth e in enemies)
            if (e != null) Destroy(e.gameObject);
        foreach (GameObject h in spawnedHazards)
            if (h != null) Destroy(h);
        enemies.Clear();
        spawnedHazards.Clear();

        if (specialRoomInstance != null) Destroy(specialRoomInstance);

        // Leftover projectiles and effects from the previous room
        foreach (Bomb x in FindObjectsByType<Bomb>(FindObjectsSortMode.None)) Destroy(x.gameObject);
        foreach (MageShockwave x in FindObjectsByType<MageShockwave>(FindObjectsSortMode.None)) Destroy(x.gameObject);
        foreach (Spell x in FindObjectsByType<Spell>(FindObjectsSortMode.None)) Destroy(x.gameObject);
        foreach (ExplosionEffect x in FindObjectsByType<ExplosionEffect>(FindObjectsSortMode.None)) Destroy(x.gameObject);
    }

    // ---------- Helpers ----------

    private int AliveEnemies()
    {
        int alive = 0;
        foreach (EnemyHealth e in enemies)
            if (e != null && !e.IsDead) alive++;
        return alive;
    }

    private void SetEnemiesFrozen(bool frozen)
    {
        foreach (EnemyHealth e in enemies)
        {
            if (e == null) continue;

            foreach (MonoBehaviour mb in e.GetComponents<MonoBehaviour>())
                if (mb != e) mb.enabled = !frozen;

            Animator anim = e.GetComponentInChildren<Animator>();
            if (anim != null) anim.enabled = !frozen;
        }
    }

    private void SetPlayerControl(bool enabled)
    {
        playerMovement.enabled = enabled;
        playerSpell.enabled = enabled;

        if (!enabled)
        {
            playerRb.linearVelocity = Vector2.zero;
            if (playerAnimator != null) playerAnimator.SetBool(IsMovingHash, false);
        }
    }

    private IEnumerator WalkTo(Vector2 target, bool stopWhenPortalEntered)
    {
        if (playerAnimator != null) playerAnimator.SetBool(IsMovingHash, true);

        float timeout = 5f;
        while (timeout > 0f && Vector2.Distance(player.transform.position, target) > 0.1f)
        {
            if (stopWhenPortalEntered && portalEntered) break;

            Vector2 dir = (target - (Vector2)player.transform.position).normalized;
            playerRb.linearVelocity = dir * walkSpeed;
            timeout -= Time.deltaTime;
            yield return null;
        }

        playerRb.linearVelocity = Vector2.zero;
        if (playerAnimator != null) playerAnimator.SetBool(IsMovingHash, false);
    }

    private IEnumerator Fade(float target)
    {
        float start = fadeGroup.alpha;
        for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
        {
            fadeGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }
        fadeGroup.alpha = target;
    }
}