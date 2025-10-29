using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FoodPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [Tooltip("Seconds for one full pulse cycle")]
    public float period = 1.2f;               // slower, calmer rhythm
    [Tooltip("Min/Max scale relative to the base size")]
    public float minScale = 0.985f;
    public float maxScale = 1.025f;           // only 2.5% pulse difference

    [Header("Color Settings")]
    public Color highlightColor = new Color(1f, 0.95f, 0.6f); // softer, warm yellow
    [Range(0f, 1f)] public float colorStrength = 0.35f;        // smaller tint range

    Vector3 baseScale;
    SpriteRenderer sr;
    Color baseColor;
    float phase;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        baseColor = sr ? sr.color : Color.white;
        phase = Random.value * Mathf.PI * 2f;
    }

    void Update()
    {
        if (!sr) return;

        // smooth cosine loop
        float t = 0.5f - 0.5f * Mathf.Cos((Time.time + phase) * (2f * Mathf.PI / Mathf.Max(0.01f, period)));

        // very slight scaling
        float s = Mathf.Lerp(minScale, maxScale, t);
        transform.localScale = baseScale * s;

        // gentle tint blend
        Color target = Color.Lerp(baseColor, highlightColor, colorStrength);
        sr.color = Color.Lerp(baseColor, target, t);
    }
}
