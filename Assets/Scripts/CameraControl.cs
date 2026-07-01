using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class CameraControl : MonoBehaviour
{
    [Header("Objects")]
    public Transform cameraTransform; 
    public Text buttonText;
    public SpriteRenderer[] sprites = new SpriteRenderer[3];

    [Header("Position")]
    public float moveDuration = 1f;
    public Ease ease = Ease.OutCubic;
    public float targetPositionY;

    [Header("Text")]
    public string textInitial = "END DAY";
    public string textTarget = "END NIGHT";

    private float startPositionY;
    private bool isAtTarget;
    
    private readonly Color targetColor = new Color(0.302f, 0.302f, 0.302f, 1f);
    private readonly Color startColor = Color.white;
    private Tween cameraTween;

    private Stats playerStats;
    private ManaFill manaFillUi;
    private Clicker clickerScript;

    void Start()
    {
        playerStats = FindObjectOfType<Stats>();
        manaFillUi = FindObjectOfType<ManaFill>();
        clickerScript = FindObjectOfType<Clicker>();

        if (cameraTransform != null)
        {
            startPositionY = cameraTransform.position.y;
        }
        
        if (buttonText != null)
        {
            buttonText.text = textInitial;
        }

        if (playerStats != null)
        {
            playerStats.dayNightCycle = "Day";
        }
    }

    public void OnButtonClick()
    {
        if (cameraTransform == null || buttonText == null || cameraTween.isAlive) return;

        if (!isAtTarget)
        {
            
            cameraTween = Tween.PositionY(cameraTransform, startPositionY, targetPositionY, moveDuration, ease);
            AnimateSpritesColor(startColor, targetColor);
            buttonText.text = textTarget;

            if (playerStats != null)
            {
                playerStats.dayNightCycle = "Night";
                playerStats.mana = 100f; 

                if (manaFillUi != null)
                {
                    manaFillUi.UpdateMana(playerStats.mana);
                }
            }

           
            if (clickerScript != null)
            {
                clickerScript.isCowDead = false; 
                clickerScript.clickCount = 0;    

                
                if (clickerScript.objectCow != null) clickerScript.objectCow.SetActive(true);
                if (clickerScript.objectDead != null) clickerScript.objectDead.SetActive(false);
                
               
                if (clickerScript.cowTransform != null)
                {
                    Vector3 cowPos = clickerScript.cowTransform.position;
                    clickerScript.cowTransform.position = new Vector3(cowPos.x, clickerScript.cowTargetY, cowPos.z);
                }
            }
        }
        else
        {
            
            cameraTween = Tween.PositionY(cameraTransform, targetPositionY, startPositionY, moveDuration, ease);
            AnimateSpritesColor(targetColor, startColor);
            buttonText.text = textInitial;

            if (playerStats != null)
            {
                playerStats.dayNightCycle = "Day";
            }
        }
        
        isAtTarget = !isAtTarget;
    }

    private void AnimateSpritesColor(Color fromColor, Color toColor)
    {
        foreach (var sprite in sprites)
        {
            if (sprite != null)
            {
                Tween.Color(sprite, fromColor, toColor, moveDuration, ease);
            }
        }
    }
}
