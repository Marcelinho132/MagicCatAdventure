using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerHearts : MonoBehaviour
{
    public int maxHearts = 3;
    public int currentHearts;

    public UnityEvent<int, int> OnHealthChanged;

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

        currentHearts--;
        OnHealthChanged.Invoke(currentHearts, maxHearts);
        Debug.Log("Serduszka: " + currentHearts);

        if (currentHearts == 0)
        {
            Debug.Log("Gracz zgin¹³!");
        }
    }
}