using System;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public event Action PlayerEntered;

    // Later: trigger the open/close animation here
    public void Show(Vector2 position, bool enterable)
    {
        transform.position = position;
        gameObject.SetActive(true);
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = enterable;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerHearts>() != null)
            PlayerEntered?.Invoke();
    }
}