using UnityEngine;
using UnityEngine.InputSystem;

public class MageSpellAttack : MonoBehaviour
{
    [Header("Spell")]
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private Transform spellSpawnPoint;

    [Header("Strzelanie")]
    [SerializeField] private float cooldown = 0.6f;
    [SerializeField] private bool allowHoldToFire = true; // przytrzymanie = ciągły ogień

    [Header("Opcjonalnie")]
    [SerializeField] private Animator animator;
    [SerializeField] private string attackTrigger = "Attack";

    private float nextAttackTime;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        bool wantsToFire = allowHoldToFire
            ? Mouse.current.leftButton.isPressed
            : Mouse.current.leftButton.wasPressedThisFrame;

        if (wantsToFire && Time.time >= nextAttackTime)
        {
            Attack();
        }
    }

    private void Attack()
    {
        if (cam == null) cam = Camera.main;

        if (cam == null || spellSpawnPoint == null || spellPrefab == null)
        {
            Debug.LogError("Missing MainCamera, Spell Spawn Point, or Spell Prefab!");
            return;
        }

        nextAttackTime = Time.time + cooldown;

        if (animator != null)
            animator.SetTrigger(attackTrigger);

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 mouseWorld = cam.ScreenToWorldPoint(mouseScreen);
        Vector2 direction = (mouseWorld - (Vector2)spellSpawnPoint.position).normalized;

        GameObject spellObj = Instantiate(spellPrefab, spellSpawnPoint.position, Quaternion.identity, spellSpawnPoint);
        spellObj.transform.localPosition = Vector3.zero;

        Spell spell = spellObj.GetComponent<Spell>();
        if (spell == null)
        {
            Debug.LogError("No Spell script on the prefab!");
            Destroy(spellObj);
            return;
        }

        spell.Launch(direction);
    }
}