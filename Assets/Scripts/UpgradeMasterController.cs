using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class UpgradeMasterController : MonoBehaviour
{
    [Header("Список обычных апгрейдов для Мастер-Кнопки")]
    [Tooltip("Сюда перетаскиваем только те кнопки, которые открывают финальный Мастер-Апгрейд на 5к")]
    public UpgradeItem[] requiredUpgrades;

    [Header("Секретный финальный апгрейд")]
    public UpgradeItem finalUpgrade;

    [Header("Настройки активации")]
    public bool activateWholeGameObject = true;

    private bool isFinalActivated = false;

    void Start()
    {
        if (finalUpgrade != null)
        {
            if (activateWholeGameObject)
                finalUpgrade.gameObject.SetActive(false);
            else
                finalUpgrade.enabled = false;
        }
    }

    void Update()
    {
        if (isFinalActivated || finalUpgrade == null || requiredUpgrades == null || requiredUpgrades.Length == 0) 
            return;

        if (AreAllUpgradesMaxed())
        {
            ActivateFinalUpgrade();
        }
    }

    private bool AreAllUpgradesMaxed()
    {
        foreach (var upgrade in requiredUpgrades)
        {
            if (upgrade == null) continue;
            if (!upgrade.IsMaxed()) return false; 
        }
        return true; 
    }

    private void ActivateFinalUpgrade()
    {
        isFinalActivated = true;

        if (finalUpgrade != null)
        {
            finalUpgrade.isMasterUpgrade = true;
            finalUpgrade.pricesPerLevel = new int[] { 45000 }; 
            finalUpgrade.UpdateItemButtonText(); 

            if (activateWholeGameObject)
                finalUpgrade.gameObject.SetActive(true);
            else
                finalUpgrade.enabled = true;

            finalUpgrade.ShowTooltipData();
        }
    }

    
    public void StartEpicEnding(Transform targetButton, UpgradeItem masterItem)
    {
        float totalDuration = 12f; 
        float fadeStartDelay = 4f; 
        float fadeDuration = totalDuration - fadeStartDelay; 

        
        Vector3 savedButtonPosition = targetButton.position;

        
        Tween.PositionY(targetButton, targetButton.position.y + 12f, totalDuration, Ease.InOutQuad);
        Tween.Scale(targetButton, targetButton.localScale * 2f, totalDuration, Ease.InOutQuad);

       
        GameObject whiteScreenObj = new GameObject("FadeOverlay");
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null) whiteScreenObj.transform.SetParent(canvas.transform, false);
        
        Image whiteImage = whiteScreenObj.AddComponent<Image>();
        whiteImage.color = new Color(1f, 1f, 1f, 0f); 
        
        RectTransform rect = whiteScreenObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        
        Tween.Color(whiteImage, new Color(1f, 1f, 1f, 1f), fadeDuration, Ease.InCubic, startDelay: fadeStartDelay)
            .OnComplete(() =>
            {
                
                
                
                
                targetButton.position = savedButtonPosition;
                
                
                if (masterItem != null)
                {
                    masterItem.SetToAbsoluteMax();
                    masterItem.ForceMaxVisuals(); 
                }

                
                Tween.Color(whiteImage, new Color(1f, 1f, 1f, 0f), 2f, Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        
                        Destroy(whiteScreenObj);
                        Debug.Log("Кат-сцена завершена, все объекты возвращены на свои места!");
                    });
            });
    }
}
