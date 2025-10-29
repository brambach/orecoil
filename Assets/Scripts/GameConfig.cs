using UnityEngine;

[CreateAssetMenu(menuName = "Orecoil/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Board")]
    public int cols = 20;
    public int rows = 14;
    public float cellSize = 32f;
    public Vector2 worldOrigin = Vector2.zero;

    [Header("Speed")]
    public float startTickSeconds = 0.12f;
    public float speedupEverySeconds = 60f;
    public float speedupFactor = 0.92f;

    [Header("Shield")]
    public int shieldMaxPips = 3;
    public float shieldActiveSeconds = 2.6f;

    [Header("Scoring")]
    public int pointsPerFood = 1;

    [Header("Food & Hazards")]
    public int foodSlots = 3;              // how many food pieces exist at once
    public int hazardSlots = 1;            // how many hazards to spawn
    public int hazardStepTicks = 2;        // move hazards every N snake ticks

    [Header("Shield Effects")]
    public bool trimTailOnShieldPop = true;
    public int trimTailAmount = 1;
    public bool slowTimeOnShieldPop = true;
    public float slowTimeSeconds = 0.40f;  // short time window where tick is slower
    public float slowTimeFactor = 0.6f;    // 0.6 ⇒ 40% slower

    [Header("UI")]
    public bool showShieldToast = true;
}