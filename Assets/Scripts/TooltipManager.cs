using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    public GameObject tooltipWindow; 
    public Text tooltipText;         
    public Text costText;
    public Vector3 offset = new Vector3(20, 20, 0); 

    private Canvas canvas;
    private RectTransform canvasRect;
    private RectTransform tooltipRect;
    private int currentCost = 0;
    private UpgradeItem currentUpgrade;
    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        if (tooltipWindow != null)
            tooltipWindow.SetActive(false); 

        canvas = GetComponent<Canvas>();
        canvasRect = GetComponent<RectTransform>();
        
        if (tooltipWindow != null)
        {
            tooltipRect = tooltipWindow.GetComponent<RectTransform>();
        }
    }

    private void Update()
    {
        if (tooltipWindow != null && tooltipWindow.activeSelf && canvas != null)
        {
            Vector2 localPoint;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, 
                Input.mousePosition, 
                canvas.worldCamera,
                out localPoint
            );

            tooltipRect.anchoredPosition = (Vector3)localPoint + offset;
        }
    }

    public void Show(string text, UpgradeItem upgrade = null)
    {
        if (upgrade != null && currentUpgrade != upgrade)
        {
            currentUpgrade = upgrade;
        }
        
        if (tooltipText != null)
            tooltipText.text = text;
        
        if (costText != null)
            costText.text = currentCost.ToString();
        
        if (tooltipWindow != null && !tooltipWindow.activeSelf)
        {
            if (hideCoroutine != null) StopCoroutine(hideCoroutine);
            tooltipWindow.SetActive(true);
        }
    }

    public void UpdateCost(int newCost, UpgradeItem upgrade = null)
    {
        if (upgrade != null && currentUpgrade != upgrade) 
            return;
            
        currentCost = newCost;
        
        if (costText != null && tooltipWindow != null && tooltipWindow.activeSelf)
        {
            costText.text = currentCost.ToString();
        }
    }

    public void Hide()
    {
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideDelay());
    }
    
    private IEnumerator HideDelay()
    {
        yield return new WaitForSeconds(0.1f);
        if (tooltipWindow != null)
            tooltipWindow.SetActive(false);
        
        currentUpgrade = null;
        currentCost = 0;
    }
}