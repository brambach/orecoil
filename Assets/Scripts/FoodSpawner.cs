using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;

    readonly List<Vector2Int> foods = new List<Vector2Int>();
    readonly List<GameObject> foodGOs = new List<GameObject>();
    GameManager gm;

    public bool HasFood => foods.Count > 0;
    public Vector2Int FoodCell => foods.Count > 0 ? foods[0] : new Vector2Int(-1, -1);
    public IReadOnlyList<Vector2Int> AllFoodCells => foods;

    public void ClearFood()
    {
        foods.Clear();
        foreach (var go in foodGOs) if (go) Destroy(go);
        foodGOs.Clear();
    }

    public void SpawnSet(GameManager manager, HashSet<Vector2Int> occupied)
    {
        gm = manager;
        EnsureFoodCount(gm.config.foodSlots, occupied);
    }

    public void EnsureFoodCount(int target, HashSet<Vector2Int> occupied)
    {
        while (foods.Count < target)
        {
            if (TryRandomFreeCell(occupied, out var cell))
            {
                foods.Add(cell);
                var go = Instantiate(foodPrefab, gm.CellToWorld(cell), Quaternion.identity, gm.gridParent);
                go.transform.localScale = Vector3.one * gm.config.cellSize;
                go.AddComponent<FoodPulse>();
                foodGOs.Add(go);
                occupied.Add(cell);
            }
            else break;
        }
    }

    public bool FoodAt(Vector2Int cell) => foods.Contains(cell);

    public void ConsumeAt(Vector2Int cell, GameManager manager, HashSet<Vector2Int> occupied)
    {
        int idx = foods.IndexOf(cell);
        if (idx < 0) return;

        foods.RemoveAt(idx);
        var go = foodGOs[idx];
        foodGOs.RemoveAt(idx);
        if (go) Destroy(go);
        occupied.Remove(cell);

        // keep the board topped up
        EnsureFoodCount(manager.config.foodSlots, occupied);
    }

    bool TryRandomFreeCell(HashSet<Vector2Int> occupied, out Vector2Int cell)
    {
        int attempts = 0;
        do
        {
            int x = Random.Range(0, gm.config.cols);
            int y = Random.Range(0, gm.config.rows);
            cell = new Vector2Int(x, y);
            if (!occupied.Contains(cell)) return true;
            attempts++;
        } while (attempts < 2048);

        cell = default;
        return false;
    }
}