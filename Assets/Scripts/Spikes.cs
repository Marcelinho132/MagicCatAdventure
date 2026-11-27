using UnityEngine;

public class Spikes : MonoBehaviour
{
    [SerializeField] private float damageInterval = 1f;
    private float nextDamageTime;

    private void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("Spikes STAY: " + other.name);
        if (Time.time < nextDamageTime) return;

        PlayerHearts hearts = other.GetComponentInParent<PlayerHearts>();
        if (hearts == null) return;

        hearts.LoseHeart();
        nextDamageTime = Time.time + damageInterval;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Spikes ENTER: " + other.name);
    }
}