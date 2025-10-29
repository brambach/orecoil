using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ShieldPulse : MonoBehaviour
{
    [Header("Pulse (idle breathing)")]
    [Range(0f, 0.5f)] public float pulseAmplitude = 0.10f;   // +/- scale around 1.0
    [Range(0.2f, 10f)] public float pulseSpeed = 2.5f;       // Hz-ish
    public bool randomStartPhase = true;                      // avoids sync with other shields

    [Header("Color Shimmer")]
    public bool enableColorFlicker = true;
    [Range(0f, 0.4f)] public float colorFlickerStrength = 0.10f;

    // If using URP + Additive, HDR color works nicely. Non-HDR is fine too.
    [ColorUsage(false, true)]
    public Color baseColor = new Color(0.0f, 1.0f, 1.0f, 0.85f);

    [Header("Slow Spin")]
    public bool enableSlowSpin = true;
    public float spinSpeed = 25f; // degrees/second

    [Header("Charge Flash (when shield activates)")]
    public bool enableChargeFlash = true;
    public float flashDuration = 0.40f;       // seconds total
    public float flashScaleBoost = 0.30f;     // extra scale at peak
    public float flashIntensityBoost = 1.6f;  // extra brightness at peak

    [Header("Safety Clamp")]
    [Tooltip("Absolute scale clamp applied after all effects")]
    public float minScale = 0.75f;
    public float maxScale = 1.60f;

    // --- runtime ---
    SpriteRenderer sr;
    Vector3 baseScale;
    float phaseOffset;
    float flashTimer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        phaseOffset = randomStartPhase ? Random.Range(0f, Mathf.PI * 2f) : 0f;

        if (sr != null) sr.color = baseColor;
    }

    void OnEnable()
    {
        // Randomize again on enable if desired
        if (randomStartPhase) phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        // Optional spin
        if (enableSlowSpin)
            transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime, Space.Self);

        // Flash takes priority while active
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            float t = 1f - Mathf.Clamp01(flashTimer / Mathf.Max(0.0001f, flashDuration));
            // Smooth up & down (0→1→0) with a sine arch
            float arch = Mathf.Sin(t * Mathf.PI);

            float flashScale = 1f + arch * flashScaleBoost;
            ApplyScale(flashScale);

            if (sr != null)
            {
                float flashIntensity = 1f + arch * flashIntensityBoost;
                sr.color = baseColor * flashIntensity;
            }
            return; // skip idle pulse during flash
        }

        // Idle breathing
        float s = Mathf.Sin((Time.time * pulseSpeed) + phaseOffset);
        float scale = 1f + s * pulseAmplitude;
        ApplyScale(scale);

        // Optional subtle color shimmer
        if (enableColorFlicker && sr != null)
        {
            float flicker = 1f + s * colorFlickerStrength;
            sr.color = baseColor * flicker;
        }
        else if (sr != null)
        {
            sr.color = baseColor;
        }
    }

    void ApplyScale(float factor)
    {
        // Clamp final factor, then apply relative to the prefab’s base scale
        factor = Mathf.Clamp(factor, minScale, maxScale);
        transform.localScale = baseScale * factor;
    }

    /// <summary>
    /// Call this when the shield is activated to play the burst.
    /// </summary>
    public void TriggerFlash()
    {
        if (!enableChargeFlash) return;
        flashTimer = flashDuration;
    }

    /// <summary>
    /// Optional helpers if you want to drive settings at runtime.
    /// </summary>
    public void SetBaseColor(Color c)
    {
        baseColor = c;
        if (sr != null) sr.color = c;
    }

    public void SetBaseScaleFromCell(float cellSize, float relative = 1.0f)
    {
        baseScale = Vector3.one * (cellSize * relative);
        transform.localScale = baseScale;
    }
}