using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class UpgradeItem : MonoBehaviour
{
    [Header("UI Элементы кнопки")]
    public Text itemCostText; 

    [Header("Ручная настройка экономики")]
    public int[] pricesPerLevel = new int[] { 2, 5, 10 }; 

    [Header("Лимит и визуализация")]
    public Sprite maxUpgradeSprite; 

    [Header("Аудио (Звуки)")]
    public AudioSource audioSource;       
    public AudioClip upgradeSound;        
    public AudioClip maxUpgradeSound;     

    [Header("Что бафаем? (Поставьте галочки)")]
    public bool upgradeMilkPerClick;
    public bool upgradeMaxMana;
    public bool upgradeManaRSpeed;
    public bool upgradeCurrentMana; 

    [Header("На сколько изменяем статы? (+/-)")]
    public int milkPerClickBonus = 1;
    public float maxManaBonus = 25f;
    public float manaRSpeedBonus = 0.5f;
    public float currentManaBonus = 50f;

    [Header("Настройки Мастер-Апгрейда")]
    public bool isMasterUpgrade = false;
    [TextArea(2, 4)]
    public string customMasterDescription = "Финальный контракт: Корова улетает на луну.";

    [Header("Цепочка открытий (Новое!)")]
    [Tooltip("Включите, чтобы этот апгрейд был заблокирован, пока не вкачаны другие")]
    public bool lockUntilOthersMaxed = false;
    [Tooltip("Перетащите сюда апгрейды, которые нужно МАКСНУТЬ, чтобы открыть этот")]
    public UpgradeItem[] requiredUpgradesToUnlock;

    private int currentUpgradeCount = 0; 
    private SpriteRenderer spriteRenderer;
    protected Stats playerStats;
    private Collider2D itemCollider;

    private Tween punchScaleTween;
    private Tween rotateTween;
    private Tween hoverScaleTween;
    private Vector3 originalScale;
    private bool isCurrentlyLocked = false;

    private int CurrentCost
    {
        get
        {
            if (pricesPerLevel == null || pricesPerLevel.Length == 0) return 0;
            if (currentUpgradeCount >= pricesPerLevel.Length) return pricesPerLevel[pricesPerLevel.Length - 1];
            return pricesPerLevel[currentUpgradeCount];
        }
    }

    private int MaxUpgrades => pricesPerLevel != null ? pricesPerLevel.Length : 0;

    void Start()
    {
        playerStats = FindObjectOfType<Stats>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        itemCollider = GetComponent<Collider2D>();
        originalScale = transform.localScale;

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        CheckChainLock();
        UpdateItemButtonText();
    }

    void Update()
    {
        if (lockUntilOthersMaxed && isCurrentlyLocked)
        {
            CheckChainLock();
        }
    }

    private void CheckChainLock()
    {
        if (!lockUntilOthersMaxed || requiredUpgradesToUnlock == null || requiredUpgradesToUnlock.Length == 0)
        {
            isCurrentlyLocked = false;
            return;
        }

        foreach (var req in requiredUpgradesToUnlock)
        {
            if (req == null) continue;
            if (!req.IsMaxed())
            {
                isCurrentlyLocked = true;
                if (itemCollider != null) itemCollider.enabled = false;
                

                if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f);
                return;
            }
        }

        if (isCurrentlyLocked)
        {
            isCurrentlyLocked = false;
            if (itemCollider != null) itemCollider.enabled = true;
            if (spriteRenderer != null) spriteRenderer.color = Color.white; 
            UpdateItemButtonText();
        }
    }

    private void OnMouseEnter()
    {
        if (isCurrentlyLocked || currentUpgradeCount >= MaxUpgrades) return;
        
        TooltipManager.Instance.UpdateCost(CurrentCost, this);
        ShowTooltipData();
        
        hoverScaleTween.Stop();
        hoverScaleTween = Tween.Scale(transform, originalScale * 1.05f, 0.15f, Ease.OutQuad);
    }

    private void OnMouseExit()
    {
        if (currentUpgradeCount < MaxUpgrades) TooltipManager.Instance.Hide();
        
        hoverScaleTween.Stop();
        hoverScaleTween = Tween.Scale(transform, originalScale, 0.15f, Ease.OutQuad);
    }

    private void OnMouseDown()
    {
        if (playerStats == null || isCurrentlyLocked) return;
        if (currentUpgradeCount >= MaxUpgrades) return;

        int activeCost = CurrentCost;

        if (playerStats.milk >= activeCost)
        {
            playerStats.milk -= activeCost; 
            
            ApplyUpgrade(); 
            currentUpgradeCount++; 

            if (isMasterUpgrade)
            {
                TooltipManager.Instance.Hide();
                PlaySound(maxUpgradeSound);
                
                UpgradeMasterController master = FindObjectOfType<UpgradeMasterController>();
                if (master != null) 
                {
                    master.StartEpicEnding(transform, this);
                }
                return; 
            }

            if (currentUpgradeCount >= MaxUpgrades)
            {
                TooltipManager.Instance.Hide();
                PlaySound(maxUpgradeSound);
                AnimateMaxUpgrade(); 
            }
            else
            {
                PlaySound(upgradeSound);
                AnimateNormalUpgrade(); 
                
                if (itemCollider != null && itemCollider.bounds.Contains(GetMouseWorldPosition()))
                {
                    TooltipManager.Instance.UpdateCost(CurrentCost, this);
                    ShowTooltipData();
                }
            }

            UpdateItemButtonText();
        }
        else
        {
            rotateTween.Stop();
            transform.localRotation = Quaternion.identity; 
            rotateTween = Tween.ShakeLocalRotation(transform, new Vector3(0, 0, 10f), 0.2f, 15);
            Debug.Log("Недостаточно молока для апгрейда!");
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    private void AnimateNormalUpgrade()
    {
        punchScaleTween.Stop();
        rotateTween.Stop();
        transform.localScale = originalScale;
        transform.localRotation = Quaternion.identity;

        Vector3 punchStrength = new Vector3(-0.2f * originalScale.x, 0.3f * originalScale.y, 0f);
        punchScaleTween = Tween.PunchLocalScale(transform, punchStrength, 0.25f);
    }

    private void AnimateMaxUpgrade()
    {
        ForceMaxVisuals();
    }

    public void ForceMaxVisuals()
    {
        punchScaleTween.Stop();
        rotateTween.Stop();
        transform.localScale = originalScale;
        transform.localRotation = Quaternion.identity;

        if (spriteRenderer != null && maxUpgradeSprite != null)
        {
            spriteRenderer.sprite = maxUpgradeSprite;
        }
        UpdateItemButtonText();

        rotateTween = Tween.ShakeLocalRotation(transform, new Vector3(0, 0, 15f), 0.3f, 20)
            .OnComplete(() =>
            {
                punchScaleTween = Tween.Scale(transform, originalScale * 1.3f, 0.15f, Ease.OutBack)
                    .OnComplete(() =>
                    {
                        punchScaleTween = Tween.Scale(transform, originalScale, 0.3f, Ease.InOutQuad);
                    });
            });
    }

    public void SetToAbsoluteMax()
    {
        currentUpgradeCount = MaxUpgrades;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }

    public void ShowTooltipData()
    {
        if (isCurrentlyLocked) return;
        TooltipManager.Instance.Show(GenerateAutoDescription(), this);
        TooltipManager.Instance.UpdateCost(CurrentCost, this);
    }

    public void UpdateItemButtonText()
    {
        if (itemCostText != null)
        {
            if (isCurrentlyLocked) itemCostText.text = "LOCKED";
            else if (currentUpgradeCount >= MaxUpgrades) itemCostText.text = "MAX";
            else itemCostText.text = CurrentCost.ToString();
        }
    }

    private string GenerateAutoDescription()
    {
        if (isMasterUpgrade) return customMasterDescription; 

        string description = "";
        if (upgradeMilkPerClick) description += (milkPerClickBonus >= 0 ? "+" : "") + milkPerClickBonus + " Milk Per Click\n";
        if (upgradeMaxMana) description += (maxManaBonus >= 0 ? "+" : "") + maxManaBonus + " Max Mana\n";
        if (upgradeManaRSpeed) description += (manaRSpeedBonus >= 0 ? "+" : "") + manaRSpeedBonus + " Mana R Speed\n";
        if (upgradeCurrentMana) description += (currentManaBonus >= 0 ? "+" : "") + currentManaBonus + " Mana\n";
        if (string.IsNullOrEmpty(description)) description = "Empty Upgrade";
        return description.TrimEnd('\n');
    }

    protected virtual void ApplyUpgrade()
    {
        if (isMasterUpgrade) return; 

        if (upgradeMilkPerClick) playerStats.milkPerClick += milkPerClickBonus;
        if (upgradeMaxMana) playerStats.maxMana += maxManaBonus;
        if (upgradeManaRSpeed) playerStats.manaRSpeed += manaRSpeedBonus;
        
        if (upgradeCurrentMana)
        {
            playerStats.mana += currentManaBonus;
            if (playerStats.mana > playerStats.maxMana) playerStats.mana = playerStats.maxMana;
        }
    }

    public bool IsMaxed()
    {
        return currentUpgradeCount >= MaxUpgrades;
    }

    private void OnDestroy()
    {
        punchScaleTween.Stop();
        rotateTween.Stop();
        hoverScaleTween.Stop();
    }
}