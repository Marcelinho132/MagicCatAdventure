using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHearts playerHealth;
    [SerializeField] private Image[] hearts;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    void OnEnable()
    {
        playerHealth.OnHealthChanged.AddListener(UpdateHearts);
        UpdateHearts(playerHealth.currentHearts, playerHealth.maxHearts);
    }

    void OnDisable()
    {
        playerHealth.OnHealthChanged.RemoveListener(UpdateHearts);
    }

    void UpdateHearts(int current, int max)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].enabled = i < max;
            hearts[i].sprite = i < current ? fullHeart : emptyHeart;
        }
    }
}