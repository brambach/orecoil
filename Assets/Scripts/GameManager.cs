using UnityEngine;
using System.Collections.Generic;

public enum GameState { Title, Playing, GameOver }

public class GameManager : MonoBehaviour
{
    [Header("Scene References")]
    public GameConfig config;
    public Snake snake;
    public FoodSpawner spawner;
    public UIController ui;
    public Sfx sfx;
    public Transform gridParent;

    [Header("Tuning")]
    public float wallGraceSeconds = 0.35f;
    float wallGraceTimer = 0f;
    public float bounceSlowFactor = 0.6f;

    [Header("Prefabs")]
    public GameObject hazardPrefab;

    [Header("UI Helpers")]
    public ToastTMP toast;

    [Header("State")]
    public GameState state = GameState.Title;

    float tickTimer;
    float tickInterval;
    float runTimer;
    int score;
    int shieldPips;
    bool shieldActive;
    float shieldTimer;
    float slowTimeTimer = 0f;
    int tickCount = 0;

    readonly List<Vector2Int> hazardCells = new List<Vector2Int>();
    readonly List<GameObject> hazardGOs = new List<GameObject>();
    readonly HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

    void Start()
    {
        Application.targetFrameRate = 120;
        tickInterval = config.startTickSeconds;
        ui.ShowTitle(true);
        ui.UpdateScore(0);
        ui.UpdateShield(0, config.shieldMaxPips, false);
    }

    void Update()
    {
        if (state == GameState.Title || state == GameState.GameOver)
        {
            bool pressedEnter = false;
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null) pressedEnter = kb.enterKey.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            pressedEnter = pressedEnter || Input.GetKeyDown(KeyCode.Return);
#endif
            if (pressedEnter) StartRun();
            return;
        }

        runTimer += Time.deltaTime;

        if (wallGraceTimer <= 0f && runTimer >= config.speedupEverySeconds)
        {
            runTimer = 0f;
            tickInterval *= config.speedupFactor;
        }

        if (shieldActive)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                shieldActive = false;
                ui.UpdateShield(shieldPips, config.shieldMaxPips, shieldActive);
                snake.SetShieldVisual(false);
                sfx.PlayShieldExpire();
            }
        }

        if (slowTimeTimer > 0f) slowTimeTimer -= Time.deltaTime;
        if (wallGraceTimer > 0f) wallGraceTimer -= Time.deltaTime;

        HandleShieldInput();

        float activeTick = (slowTimeTimer > 0f && config.slowTimeOnShieldPop)
            ? Mathf.Max(0.02f, config.startTickSeconds * config.slowTimeFactor)
            : tickInterval;

        tickTimer += Time.deltaTime;
        while (tickTimer >= activeTick)
        {
            tickTimer -= activeTick;
            StepGame();
        }
    }

    void StartRun()
    {
        state = GameState.Playing;

        ClearBoard();

        score = 0;
        shieldPips = 0;
        shieldActive = false;
        shieldTimer = 0f;
        wallGraceTimer = 0f;
        slowTimeTimer = 0f;
        runTimer = 0f;
        tickInterval = config.startTickSeconds;
        tickTimer = 0f;
        tickCount = 0;

        ui.ShowTitle(false);
        ui.ShowGameOver(false);
        ui.UpdateScore(score);
        ui.UpdateShield(shieldPips, config.shieldMaxPips, shieldActive);

        AdjustBoardToCameraAspect(changeCols: true);
        snake.Init(this);
        RebuildOccupied();
        spawner.SpawnSet(this, occupied);
        SpawnHazards();
        FrameBoard(FrameMode.FitHeight);
    }

    void StepGame()
    {
        tickCount++;

        if (config.hazardSlots > 0 && (tickCount % Mathf.Max(1, config.hazardStepTicks) == 0))
            MoveHazards();

        var next = snake.NextHead();
        bool willGrow = spawner.FoodAt(next);
        bool hitHazard = HazardAt(next);
        bool hitWall = next.x < 0 || next.x >= config.cols || next.y < 0 || next.y >= config.rows;
        bool hitSelf = snake.BodyContains(next, ignoreTail: !willGrow);

        if (wallGraceTimer > 0f)
        {
            if (hitWall)
            {
                SmartWallBounce();
                wallGraceTimer = wallGraceSeconds;
                RebuildOccupied();
                return;
            }
            hitSelf = false;
        }

        if (hitHazard || hitWall || hitSelf)
        {
            if (shieldActive)
            {
                shieldActive = false;
                shieldTimer = 0f;
                ui.UpdateShield(shieldPips, config.shieldMaxPips, shieldActive);
                snake.SetShieldVisual(false);
                sfx.PlayShieldPop();

                if (config.trimTailOnShieldPop)
                    snake.TrimTail(Mathf.Max(1, config.trimTailAmount));

                if (config.slowTimeOnShieldPop)
                    slowTimeTimer = config.slowTimeSeconds;

                if (hitWall)
                {
                    SmartWallBounce();
                    wallGraceTimer = wallGraceSeconds;
                    tickTimer = 0f;
                    RebuildOccupied();
                    return;
                }
                else
                {
                    snake.Advance(next, grow: false);

                    if (hitHazard)
                    {
                        DespawnHazardAt(next);
                        SpawnOneHazardSafe();
                    }

                    RebuildOccupied();
                    return;
                }
            }
            else
            {
                EndRun();
                return;
            }
        }

        snake.Advance(next, grow: willGrow);

        if (willGrow)
        {
            sfx.PlayEat();
            score += config.pointsPerFood;
            ui.UpdateScore(score);
            GainPip();
            spawner.ConsumeAt(next, this, occupied);
        }

        RebuildOccupied();
    }

    void GainPip()
    {
        if (shieldPips < config.shieldMaxPips)
        {
            shieldPips++;
            ui.UpdateShield(shieldPips, config.shieldMaxPips, shieldActive);
        }
    }

    void HandleShieldInput()
    {
        if (state != GameState.Playing) return;

        bool pressedSpace = false;
#if ENABLE_INPUT_SYSTEM
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null) pressedSpace = kb.spaceKey.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        pressedSpace = pressedSpace || Input.GetKeyDown(KeyCode.Space);
#endif

        if (pressedSpace && shieldPips >= config.shieldMaxPips && !shieldActive)
        {
            shieldActive = true;
            shieldTimer = config.shieldActiveSeconds;
            shieldPips = 0;
            ui.UpdateShield(shieldPips, config.shieldMaxPips, shieldActive);
            snake.SetShieldVisual(true);
            sfx.PlayShieldOn();
            snake.TrimTail(1);

            if (config.showShieldToast && toast != null)
                toast.Show("Shield!", 0.9f);
        }
    }

    void EndRun()
    {
        state = GameState.GameOver;
        sfx.PlayDeath();
        ui.ShowGameOver(true, score);
        snake.SetShieldVisual(false);
    }

    // ---------- Utilities + helpers ----------

    void ClearBoard()
    {
        occupied.Clear();
        spawner.ClearFood();
        foreach (Transform c in gridParent) Destroy(c.gameObject);
        foreach (var go in hazardGOs) if (go) Destroy(go);
        hazardGOs.Clear();
        hazardCells.Clear();
    }

    void RebuildOccupied()
    {
        occupied.Clear();
        foreach (var seg in snake.Body) occupied.Add(seg);
        foreach (var f in spawner.AllFoodCells) occupied.Add(f);
        foreach (var h in hazardCells) occupied.Add(h);
    }

    void SpawnHazards()
    {
        for (int i = 0; i < config.hazardSlots; i++)
            SpawnOneHazardSafe();
    }

    void SpawnOneHazardSafe()
    {
        for (int tries = 0; tries < 2048; tries++)
        {
            int x = Random.Range(0, config.cols);
            int y = Random.Range(0, config.rows);
            var c = new Vector2Int(x, y);
            if (occupied.Contains(c)) continue;

            hazardCells.Add(c);
            var go = Instantiate(hazardPrefab, CellToWorld(c), Quaternion.identity, gridParent);
            go.transform.localScale = Vector3.one * config.cellSize;
            hazardGOs.Add(go);
            occupied.Add(c);
            return;
        }
    }

    void MoveHazards()
    {
        for (int i = 0; i < hazardCells.Count; i++)
        {
            var cur = hazardCells[i];

            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            Shuffle(dirs);

            Vector2Int target = cur;
            foreach (var d in dirs)
            {
                var c = cur + d;
                if (!InBounds(c)) continue;
                if (IsHazardCell(c)) continue;
                if (IsSnakeCell(c)) continue;
                target = c;
                break;
            }

            occupied.Remove(cur);
            hazardCells[i] = target;
            occupied.Add(target);

            if (i < hazardGOs.Count && hazardGOs[i])
                hazardGOs[i].transform.position = CellToWorld(target);

            if (spawner.FoodAt(target))
                spawner.ConsumeAt(target, this, occupied);
        }
    }

    bool HazardAt(Vector2Int cell) => hazardCells.Contains(cell);
    bool IsHazardCell(Vector2Int c) => hazardCells.Contains(c);

    bool IsSnakeCell(Vector2Int c)
    {
        foreach (var seg in snake.Body) if (seg == c) return true;
        return false;
    }

    void DespawnHazardAt(Vector2Int cell)
    {
        int idx = hazardCells.IndexOf(cell);
        if (idx < 0) return;
        hazardCells.RemoveAt(idx);
        occupied.Remove(cell);
        if (idx < hazardGOs.Count)
        {
            var go = hazardGOs[idx];
            hazardGOs.RemoveAt(idx);
            if (go) Destroy(go);
        }
    }

    public Vector3 CellToWorld(Vector2Int c)
    {
        return (Vector2)config.worldOrigin
             + new Vector2((c.x + 0.5f) * config.cellSize, (c.y + 0.5f) * config.cellSize);
    }

    bool InBounds(Vector2Int c) =>
        c.x >= 0 && c.x < config.cols && c.y >= 0 && c.y < config.rows;

    enum FrameMode { Contain, Cover, FitWidth, FitHeight }

    void FrameBoard(FrameMode mode = FrameMode.Contain)
    {
        var cam = Camera.main;
        if (!cam) return;

        cam.orthographic = true;

        float worldW = config.cols * config.cellSize;
        float worldH = config.rows * config.cellSize;

        Vector2 center = config.worldOrigin + new Vector2(worldW * 0.5f, worldH * 0.5f);
        cam.transform.position = new Vector3(Mathf.Round(center.x), Mathf.Round(center.y), -10f);

        float sizeForHeight = worldH * 0.5f;
        float sizeForWidth = (worldW * 0.5f) / Mathf.Max(cam.aspect, 0.0001f);

        float size = mode switch
        {
            FrameMode.Cover => Mathf.Min(sizeForHeight, sizeForWidth),
            FrameMode.FitWidth => sizeForWidth,
            FrameMode.FitHeight => sizeForHeight,
            _ => Mathf.Max(sizeForHeight, sizeForWidth),
        };

        cam.orthographicSize = size;
    }

    void AdjustBoardToCameraAspect(bool changeCols = true)
    {
        var cam = Camera.main;
        if (!cam) return;

        float cameraAspect = cam.aspect;
        float boardAspect = (float)config.cols / Mathf.Max(1, config.rows);
        if (Mathf.Abs(boardAspect - cameraAspect) < 0.001f) return;

        if (changeCols)
            config.cols = Mathf.Max(4, Mathf.RoundToInt(config.rows * cameraAspect));
        else
            config.rows = Mathf.Max(4, Mathf.RoundToInt(config.cols / cameraAspect));
    }

    void SmartWallBounce()
    {
        Vector2Int cur = snake.HeadCell;
        Vector2Int d = snake.CurrentDir;

        Vector2Int left = new Vector2Int(-d.y, d.x);
        Vector2Int right = new Vector2Int(d.y, -d.x);
        Vector2Int inward = -d;

        if (TryAdvanceIfFree(cur, left)) return;
        if (TryAdvanceIfFree(cur, right)) return;
        if (TryAdvanceIfFree(cur, inward)) return;

        Vector2Int forced = ClampToBoard(cur + inward);
        snake.ForceAdvance(forced, inward);
    }

    bool TryAdvanceIfFree(Vector2Int from, Vector2Int dir)
    {
        Vector2Int c = from + dir;
        if (!InBounds(c)) return false;
        if (snake.BodyContains(c, ignoreTail: true)) return false;
        snake.ForceAdvance(c, dir);
        return true;
    }

    Vector2Int ClampToBoard(Vector2Int c)
    {
        c.x = Mathf.Clamp(c.x, 0, config.cols - 1);
        c.y = Mathf.Clamp(c.y, 0, config.rows - 1);
        return c;
    }

    static void Shuffle(Vector2Int[] a)
    {
        for (int i = a.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (a[i], a[j]) = (a[j], a[i]);
        }
    }
}