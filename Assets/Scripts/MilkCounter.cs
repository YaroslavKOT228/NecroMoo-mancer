using UnityEngine;
using UnityEngine.UI;

public class MilkCounter : MonoBehaviour
{
    private Text textComponent;
    private Stats playerStats;

    void Start()
    {
        textComponent = GetComponent<Text>();
        playerStats = FindObjectOfType<Stats>();
    }

    void Update()
    {
        if (textComponent != null && playerStats != null)
        {
            textComponent.text = playerStats.milk.ToString();
        }
    }
}
