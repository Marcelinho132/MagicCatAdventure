using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerHearts : MonoBehaviour
{
    public int maxHearts = 3;
    public int currentHearts;
    public float invulnerableTime = 0.5f;

    public UnityEvent<int, int> OnHealthChanged;

    private float invulnerableUntil;

    void Start()
    {
        currentHearts = maxHearts;
        OnHealthChanged.Invoke(currentHearts, maxHearts);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            LoseHeart();
        }
    }

    public void LoseHeart()
    {
        if (currentHearts <= 0) return;
        if (Time.time < invulnerableUntil) return;

        invulnerableUntil = Time.time + invulnerableTime;

        currentHearts--;
        StartCoroutine(HitFlash());
        OnHealthChanged.Invoke(currentHearts, maxHearts);
        Debug.Log("Hearts: " + currentHearts);

        if (currentHearts == 0)
        {
            Debug.Log("Player died!");
        }
    }

    private IEnumerator HitFlash()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null) yield break;
        Color original = sr.color;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        sr.color = original;
    }
}