using UnityEngine;

public class BruteHitbox : MonoBehaviour
{
    private bool hasHit;

    private void OnEnable()
    {
        hasHit = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (hasHit) return;

        PlayerHearts hearts = other.GetComponentInParent<PlayerHearts>();
        if (hearts == null) return;

        hearts.LoseHeart();
        hasHit = true;
    }
}