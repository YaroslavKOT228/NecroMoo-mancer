using UnityEngine;

public class ManaFill : MonoBehaviour
{
    [Header("Настройки диапазона")]
    [SerializeField] private float maxVariableValue = 100f;

    private RectTransform rectTransform;
    private float maxUiWidth;
    private float currentManaValue = 100f; 

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        maxUiWidth = rectTransform.sizeDelta.x;
    }

    public void UpdateMana(float currentMana)
    {
        Stats stats = FindObjectOfType<Stats>();
        float maxMana = (stats != null) ? stats.maxMana : 100f;

        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);
    
        float percentage = currentMana / maxMana;
        float newWidth = maxUiWidth * percentage;

        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(newWidth, rectTransform.sizeDelta.y);
        }
    }


    public void SetMana(float value)
    {
        UpdateMana(value);
    }
}
