using UnityEngine;
using System.Collections.Generic;

public class Snake : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject headPrefab;
    public GameObject bodyPrefab;

    [Header("Render Parent")]
    public Transform renderParent;

    [Header("Shield VFX")]
    public bool useShieldPrefab = true;                 // true = use prefab below, false = generate a circle
    public GameObject shieldVisualPrefab;               // assign your ShieldVisual prefab
    public Sprite shieldSprite;                         // used if not using a prefab
    public Material shieldMaterial;                     // optional
    public float shieldScale = 1.45f;                   // relative to head size
    public Color shieldColor = new Color(0f, 1f, 0.95f, 0.85f);
    public int shieldOrderOffset = 10;                  // draw above head

    readonly List<Vector2Int> body = new List<Vector2Int>();
    public IReadOnlyList<Vector2Int> Body => body;

    Vector2Int dir = Vector2Int.right;
    Vector2Int lastDir = Vector2Int.right;
    Vector2Int pendingDir = Vector2Int.right;

    GameManager gm;
    GameObject headGO;
    readonly List<GameObject> bodyGOs = new List<GameObject>();

    GameObject shieldVfxGO;
    SpriteRenderer shieldSR; // only used for generated shield

    public Vector2Int HeadCell => body.Count > 0 ? body[0] : Vector2Int.zero;
    public Vector2Int CurrentDir => dir;

    public void Init(GameManager manager)
    {
        gm = manager;
        body.Clear();
        ClearVisuals();

        int sx = Mathf.Clamp(gm.config.cols / 2, 3, gm.config.cols - 3);
        int sy = Mathf.Clamp(gm.config.rows / 2, 3, gm.config.rows - 3);
        Vector2Int start = new Vector2Int(sx, sy);

        body.Add(start);
        body.Add(start + Vector2Int.left);
        body.Add(start + 2 * Vector2Int.left);

        dir = Vector2Int.right;
        lastDir = dir;
        pendingDir = dir;

        headGO = Instantiate(headPrefab, gm.CellToWorld(body[0]), Quaternion.identity, renderParent);
        headGO.transform.localScale = Vector3.one * gm.config.cellSize;

        CreateShieldVfx();

        for (int i = 1; i < body.Count; i++)
        {
            var seg = Instantiate(bodyPrefab, gm.CellToWorld(body[i]), Quaternion.identity, renderParent);
            seg.transform.localScale = Vector3.one * gm.config.cellSize;
            bodyGOs.Add(seg);
        }
    }

    void CreateShieldVfx()
    {
        if (shieldVfxGO != null) return;

        if (useShieldPrefab && shieldVisualPrefab != null)
        {
            // Instantiate prefab as a child of the head
            shieldVfxGO = Instantiate(shieldVisualPrefab, headGO.transform);
            shieldVfxGO.transform.localPosition = Vector3.zero;
            shieldVfxGO.transform.localRotation = Quaternion.identity;
            shieldVfxGO.transform.localScale = Vector3.one * shieldScale;

            // Make sure it renders above the head
            var headSR = headGO.GetComponent<SpriteRenderer>();
            if (headSR)
            {
                foreach (var sr in shieldVfxGO.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    sr.sortingLayerID = headSR.sortingLayerID;
                    sr.sortingOrder = headSR.sortingOrder + shieldOrderOffset;
                }
            }

            shieldVfxGO.SetActive(false);
            return;
        }

        // Generated simple circle if no prefab is provided
        shieldVfxGO = new GameObject("ShieldVFX");
        shieldVfxGO.transform.SetParent(headGO.transform, false);
        shieldVfxGO.transform.localPosition = Vector3.zero;

        shieldSR = shieldVfxGO.AddComponent<SpriteRenderer>();
        var headSprite = headGO.GetComponent<SpriteRenderer>();

        shieldSR.sprite = shieldSprite ? shieldSprite : (headSprite ? headSprite.sprite : null);
        if (shieldMaterial) shieldSR.material = shieldMaterial;

        if (headSprite)
        {
            shieldSR.sortingLayerID = headSprite.sortingLayerID;
            shieldSR.sortingOrder = headSprite.sortingOrder + shieldOrderOffset;
        }

        shieldVfxGO.transform.localScale = Vector3.one * shieldScale;
        shieldSR.color = shieldColor;

        // optional soft glow behind the main ring
        var glowGO = new GameObject("ShieldGlow");
        glowGO.transform.SetParent(shieldVfxGO.transform, false);
        var glowSR = glowGO.AddComponent<SpriteRenderer>();
        glowSR.sprite = shieldSR.sprite;
        if (headSprite)
        {
            glowSR.sortingLayerID = headSprite.sortingLayerID;
            glowSR.sortingOrder = shieldSR.sortingOrder - 1;
        }
        glowGO.transform.localScale = Vector3.one * 1.12f;
        var c = shieldColor; c.a *= 0.35f;
        glowSR.color = c;

        shieldVfxGO.SetActive(false);
    }

    public void SetShieldVisual(bool on)
    {
        if (shieldVfxGO) shieldVfxGO.SetActive(on);
    }

    void ClearVisuals()
    {
        if (headGO) Destroy(headGO);
        if (shieldVfxGO) Destroy(shieldVfxGO);
        shieldVfxGO = null; shieldSR = null;

        for (int i = 0; i < bodyGOs.Count; i++)
            if (bodyGOs[i]) Destroy(bodyGOs[i]);
        bodyGOs.Clear();
    }

    void Update() => ReadInputFrame();

    void ReadInputFrame()
    {
        bool up = false, down = false, left = false, right = false;

#if ENABLE_INPUT_SYSTEM
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null)
        {
            up = kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame;
            down = kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame;
            left = kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame;
            right = kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        up = up || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);
        down = down || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);
        left = left || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
        right = right || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
#endif

        Vector2Int wanted = pendingDir;
        if (up) wanted = Vector2Int.up;
        if (down) wanted = Vector2Int.down;
        if (left) wanted = Vector2Int.left;
        if (right) wanted = Vector2Int.right;
        if (wanted + dir != Vector2Int.zero) pendingDir = wanted;
    }

    public Vector2Int NextHead()
    {
        if (pendingDir + lastDir != Vector2Int.zero)
            dir = pendingDir;
        return body[0] + dir;
    }

    public void Advance(Vector2Int nextHead, bool grow)
    {
        lastDir = dir;
        body.Insert(0, nextHead);
        if (!grow && body.Count > 1) body.RemoveAt(body.Count - 1);
        Render();
    }

    public void ForceAdvance(Vector2Int newHead, Vector2Int newDir)
    {
        dir = newDir; lastDir = newDir;
        body.Insert(0, newHead);
        if (body.Count > 1) body.RemoveAt(body.Count - 1);
        Render();
    }

    public void TrimTail(int n)
    {
        for (int i = 0; i < n; i++)
        {
            if (body.Count <= 1) break;
            body.RemoveAt(body.Count - 1);
            if (bodyGOs.Count > 0)
            {
                var go = bodyGOs[bodyGOs.Count - 1];
                bodyGOs.RemoveAt(bodyGOs.Count - 1);
                if (go) Destroy(go);
            }
        }
        Render();
    }

    public bool BodyContains(Vector2Int cell, bool ignoreTail)
    {
        int limit = body.Count - (ignoreTail ? 1 : 0);
        for (int i = 0; i < limit; i++)
            if (body[i] == cell) return true;
        return false;
    }

    void Render()
    {
        if (headGO) headGO.transform.position = gm.CellToWorld(body[0]);

        while (bodyGOs.Count < body.Count - 1)
        {
            var seg = Instantiate(bodyPrefab, Vector3.zero, Quaternion.identity, renderParent);
            seg.transform.localScale = Vector3.one * gm.config.cellSize;
            bodyGOs.Add(seg);
        }
        while (bodyGOs.Count > body.Count - 1)
        {
            var last = bodyGOs[bodyGOs.Count - 1];
            bodyGOs.RemoveAt(bodyGOs.Count - 1);
            if (last) Destroy(last);
        }
        for (int i = 1; i < body.Count; i++)
            bodyGOs[i - 1].transform.position = gm.CellToWorld(body[i]);
    }
}