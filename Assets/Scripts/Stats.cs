using UnityEngine;

public class Stats : MonoBehaviour
{
    [Header("Mana Settings")]
    public float mana = 100f;
    public float maxMana = 100f;
    public float manaRSpeed = 2f;

    [Header("Milk Economy")]
    public int milk = 0;          
    public int milkPerClick = 1;  

    [Header("Time State")]
    public string dayNightCycle = "Day";
    private void Start() {
        print("сигма");  
    } 
}
