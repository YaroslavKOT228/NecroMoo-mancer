using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class Clicker : MonoBehaviour
{
    [Header("Stats Reference")]
    [SerializeField] private Stats playerStats; 

    [Header("UI Objects")]
    public Text milkText; 

    [Header("Objects")]
    public GameObject objectDead;
    public Transform deadTransform;
    public GameObject objectCow;
    public Transform cowTransform;

    [Header("Components for FX")]
    public SpriteRenderer cowSpriteRenderer;
    public SpriteRenderer deadSpriteRenderer;
    public AudioSource audioSource;

    [Header("Switch Animation Settings")]
    public float moveDuration = 1f;
    public Ease ease = Ease.OutCubic;
    
    [Header("Y Coordinates")]
    public float cowStartY = -5f;
    public float cowTargetY = -1.7f;
    public float deadStartY = 10f;
    public float deadTargetY = -1.5f;

    [Header("Value")]
    public int clickCount = 0;
    public bool isCowDead;

    [Header("1. Click Scale Animation (Punch)")]
    public float clickScaleDuration = 0.15f;
    public Vector3 clickScaleMultiplier = new Vector3(0.8f, 1.4f, 1f);

    [Header("2. Click Rotation Animation (Shake)")]
    public float clickRotationDuration = 0.15f;
    public Vector3 rotationStrength = new Vector3(0, 0, 15f);
    public int rotationFrequency = 10;

    [Header("3. Click Flash Color Animation")]
    public float flashDuration = 0.1f;
    public Color flashColor = new Color(1.6f, 1.6f, 1.6f, 1f);

    [Header("4. Camera Shake on Switch")]
    public float cameraShakeDuration = 0.2f;
    public Vector3 cameraShakeStrength = new Vector3(0.2f, 0.2f, 0f);
    public int cameraShakeFrequency = 10;

    private Tween deadTween;
    private Tween cowTween;
    private Tween clickScaleTween;
    private Tween clickRotationTween;
    private Tween clickColorTween;
    private Tween cameraShakeTween;
    
    private Vector3 cowOriginalScale;
    private Vector3 deadOriginalScale;
    private ManaFill manaFillUi;

    void Start()
    {
        manaFillUi = FindObjectOfType<ManaFill>();

        if (playerStats == null)
        {
            playerStats = FindObjectOfType<Stats>();
        }

        if (objectCow != null && objectDead != null)
        {
            objectCow.SetActive(!isCowDead);
            objectDead.SetActive(isCowDead);
            
            cowOriginalScale = cowTransform.localScale;
            deadOriginalScale = deadTransform.localScale;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (playerStats != null && manaFillUi != null)
        {
            manaFillUi.UpdateMana(playerStats.mana);
        }

        UpdateMilkUI();
    }

    void Update()
    {
        if (playerStats == null) return;

        RegenerateMana();

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                if (hit.collider.gameObject == objectCow && !isCowDead)
                {
                    clickCount++;
                    
                    
                    playerStats.milk += playerStats.milkPerClick;
                    
                    
                    UpdateMilkUI();
                    
                    PlayClickSound();
                    AnimateJuicyClick(cowTransform, cowOriginalScale, cowSpriteRenderer);

                    if (clickCount >= 10)
                    {
                        SwitchObjects();
                    }
                }
                else if (hit.collider.gameObject == objectDead && isCowDead)
                {
                    if (playerStats.mana >= 50f)
                    {
                        PlayClickSound();
                        AnimateJuicyClick(deadTransform, deadOriginalScale, deadSpriteRenderer);
                        SwitchObjects();
                    }
                }
            }
        }
    }

    void RegenerateMana()
    {
        if (playerStats.mana < playerStats.maxMana)
        {
            playerStats.mana += playerStats.manaRSpeed * Time.deltaTime;
            
            if (playerStats.mana > playerStats.maxMana) 
            {
                playerStats.mana = playerStats.maxMana;
            }

            if (manaFillUi != null)
            {
                manaFillUi.UpdateMana(playerStats.mana);
            }
        }
    }

    
    void UpdateMilkUI()
    {
        if (milkText != null && playerStats != null)
        {
            milkText.text = "Milk: " + playerStats.milk;
        }
    }

    void PlayClickSound()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    void AnimateJuicyClick(Transform target, Vector3 originalScale, SpriteRenderer spriteRenderer)
    {
        clickScaleTween.Stop();
        Vector3 punchAmount = new Vector3(
            originalScale.x * (clickScaleMultiplier.x - 1f),
            originalScale.y * (clickScaleMultiplier.y - 1f),
            originalScale.z * (clickScaleMultiplier.z - 1f)
        );
        clickScaleTween = Tween.PunchLocalPosition(target, punchAmount, clickScaleDuration);

        clickRotationTween.Stop();
        clickRotationTween = Tween.ShakeLocalRotation(target, rotationStrength, clickRotationDuration, rotationFrequency);

        if (spriteRenderer != null)
        {
            clickColorTween.Stop();
            clickColorTween = Tween.Color(spriteRenderer, startValue: flashColor, endValue: Color.white, duration: flashDuration);
        }
    }

    void SwitchObjects()
    {
        if (objectDead == null || objectCow == null || playerStats == null) return;

        deadTween.Stop();
        cowTween.Stop();
        cameraShakeTween.Stop();

        cameraShakeTween = Tween.ShakeLocalPosition(Camera.main.transform, cameraShakeStrength, cameraShakeDuration, cameraShakeFrequency);

        if (!isCowDead)
        {
            isCowDead = true;
            objectDead.SetActive(true);
            
            Vector3 deadPos = deadTransform.position;
            deadTransform.position = new Vector3(deadPos.x, deadStartY, deadPos.z);

            deadTween = Tween.PositionY(deadTransform, deadTargetY, moveDuration, ease)
                .OnComplete(() => {
                    objectCow.SetActive(false);
                });
        }
        else
        {
            isCowDead = false;
            
            playerStats.mana -= 50f;
            if (playerStats.mana < 0f) playerStats.mana = 0f;

            if (manaFillUi != null)
            {
                manaFillUi.UpdateMana(playerStats.mana);
            }

            clickCount = 0;
            
            objectCow.SetActive(true);
            
            Vector3 cowPos = cowTransform.position;
            cowTransform.position = new Vector3(cowPos.x, deadStartY, cowPos.z);
            
            cowTween = Tween.PositionY(cowTransform, cowTargetY, moveDuration, ease)
                .OnComplete(() => {
                    objectDead.SetActive(false);
                });
        }
    }

    private void OnDestroy()
    {
        deadTween.Stop();
        cowTween.Stop();
        clickScaleTween.Stop();
        clickRotationTween.Stop();
        clickColorTween.Stop();
        cameraShakeTween.Stop();
    }
}
