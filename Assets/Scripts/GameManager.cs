using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.SceneManagement;

class GameGrid : ICloneable {
    public const int OBJECT_EMPTY = 0;
    public const int OBJECT_BARN = 1;
    public const int OBJECT_TOWER = 3;
    
    private int[,] distances = null;
    private int[,] objects = null;
    
    GameGrid(){}
    
    public GameGrid(int sizeX, int sizeY) {
        distances = new int[sizeX, sizeY];
        objects = new int[sizeX, sizeY];
        
        for(int i = 0; i < objects.GetLength(0); i++) {
            for(int j = 0; j < objects.GetLength(1); j++) {
                objects[i, j] = GameGrid.OBJECT_EMPTY;
            }
        }
        
        ClearDistances();
    }
    
    public int GetSizeX() {
        return distances.GetLength(0);
    }
    
     public int GetSizeY() {
        return distances.GetLength(1);
    }
    
    public void ClearDistances() {
        for(int i = 0; i < distances.GetLength(0); i++) {
            for (int j = 0; j < distances.GetLength(1); j++) {
                distances[i, j] = Int32.MaxValue / 2;
            }
        }
    }
    
    public void GenerateDistances() {
        Queue<(int, int, int)> queue = new Queue<(int, int, int)>();
        
        // TODO: Avoid hardcoding
        int startX = 0;
        int startY = 4 - (-4); // maxY - minY;
        
        queue.Enqueue((startX, startY, 0));
        
        while(queue.Count > 0) {
            (int x, int y, int distance) = queue.Dequeue();
            if(distance >= distances[x, y] || objects[x, y] == GameGrid.OBJECT_TOWER) continue;
            
            distances[x, y] = distance;
            
            if(x + 1 < GetSizeX() && distances[x + 1, y] > distance + 1) {
                queue.Enqueue((x + 1, y, distance + 1));
            } 
            if(y + 1 < GetSizeY() && distances[x, y + 1] > distance + 1) {
                queue.Enqueue((x, y + 1, distance + 1));
            }
            if(x - 1 >= 0 && distances[x - 1, y] > distance + 1) {
                queue.Enqueue((x - 1, y, distance + 1));
            }
            if(y - 1 >= 0 && distances[x, y - 1] > distance + 1) {
                queue.Enqueue((x, y - 1, distance + 1));
            }
        }
    }
    
    public void RegenerateDistances() {
        ClearDistances();
        GenerateDistances();
    }
    
    public bool IsValidIndex(int x, int y) {
        return x >= 0 && x < GetSizeX() && y < GetSizeY() && y >= 0;
    }
    
    public int GetDistance(int x, int y) {
        return distances[x, y];
    }
    
    public int GetObject(int x, int y) {
        return objects[x, y];
    }
    
    public void SetObject(int x, int y, int obj) {
        objects[x, y] = obj;
    }
    
    public object Clone(){
        GameGrid ret = new GameGrid();
        ret.distances = (int[,])distances.Clone();
        ret.objects = (int[,])objects.Clone();
        
        return ret;
    }
    
    private List<(int, int, int)> GetNextPositionMoves(int currentPositionX, int currentPositionY) {
        List<(int, int, int)> moves = new List<(int, int, int)>();
        
        if(currentPositionX + 1 < GetSizeX() && GetDistance(currentPositionX + 1, currentPositionY) < 1000) {            
            moves.Add((
                GetDistance(currentPositionX + 1, currentPositionY),
                currentPositionX + 1,
                currentPositionY
            ));
        }
        
        if(currentPositionX - 1 >= 0 && GetDistance(currentPositionX - 1, currentPositionY) < 1000) {            
            moves.Add((
                GetDistance(currentPositionX - 1, currentPositionY),
                currentPositionX - 1,
                currentPositionY
            ));
        }
        
        if(currentPositionY + 1 < GetSizeY() && GetDistance(currentPositionX, currentPositionY + 1) < 1000) {            
            moves.Add((
                GetDistance(currentPositionX, currentPositionY + 1),
                currentPositionX,
                currentPositionY + 1
            ));
        }
        
        if(currentPositionY - 1 >= 0 && GetDistance(currentPositionX, currentPositionY - 1) < 1000) {
            moves.Add((
                GetDistance(currentPositionX, currentPositionY - 1),
                currentPositionX,
                currentPositionY - 1
            ));
        }
        
        moves.Sort(delegate((int, int, int) t1, (int, int, int) t2) {
            return t1.Item1.CompareTo(t2.Item1);
        });
        
        return moves;
    }
    
    public (int, int) GetNextPositionSimple((int currentPositionX, int currentPositionY) t) {
        return GetNextPositionSimple(t.currentPositionX, t.currentPositionY);
    }
    
    public (int, int) GetNextPositionSimple(int currentPositionX, int currentPositionY) {
        List<(int, int, int)> moves = GetNextPositionMoves(currentPositionX, currentPositionY);
        
        if(moves.Count == 0) {
            return (-1, -1);
        }
        
        while(moves[moves.Count - 1].Item1 > moves[0].Item1) {
            moves.RemoveAt(moves.Count - 1);
        }
        
        int index = UnityEngine.Random.Range(0, moves.Count);
        return (moves[index].Item2, moves[index].Item3);
    }
    
    public (int, int) GetNextPositionMinimax((int currentPositionX, int currentPositionY) t) {
        return GetNextPositionMinimax(t.currentPositionX, t.currentPositionY);
    }
    
    public (int, int) GetNextPositionMinimax(int currentPositionX, int currentPositionY) {
        // Limited to depth = 2 to avoid blow-up, manually unrolled.
        
        // Depth 1.
        List<(int, int, int)> moves = GetNextPositionMoves(currentPositionX, currentPositionY);
        
        if(moves.Count == 0) {
            return (-1, -1);
        }
        
        // Depth 2, enemy turn. Generate out of loop for performance.
        // We assume that the AI's move has no effect on how the player will move.
        // We also assume that the user will not perform multiple actions quickly, like placing multiple towers.
        List<GameGrid> grids = new List<GameGrid>();
        grids.Add((GameGrid)Clone());
        
        for(int i = 0; i < GetSizeX(); i++) {
            for(int j = 0; j < GetSizeY(); j++) {
                if(GetObject(i, j) == GameGrid.OBJECT_EMPTY) {
                    GameGrid newGrid = (GameGrid)Clone();
                    newGrid.SetObject(i, j, GameGrid.OBJECT_TOWER);
                    grids.Add(newGrid);
                } else if (GetObject(i, j) == GameGrid.OBJECT_TOWER) {
                    GameGrid newGrid = (GameGrid)Clone();
                    newGrid.SetObject(i, j, GameGrid.OBJECT_EMPTY);
                    grids.Add(newGrid);
                }
            }
        }
        
        // If enemy cannot move, use static evaluator.
        if(grids.Count == 0) {
            while(moves[moves.Count - 1].Item1 > moves[0].Item1) {
                moves.RemoveAt(moves.Count - 1);
            }
            int index = UnityEngine.Random.Range(0, moves.Count);
            return (moves[index].Item2, moves[index].Item3);
        }
        
        // Recalculate move goodness based on all possible player moves.
        // Max, since we want to minimize distance but the enemy wants to max.
        for(int i = 0; i < moves.Count; i++) {
            (int oldDistance, int x, int y) = moves[i];
            
            int maxDistance = Int32.MinValue;
            for(int j = 0; j < grids.Count; j++) {
                int distance = grids[i].GetDistance(x, y);
                if(distance < 1000) {
                    maxDistance = Math.Max(maxDistance, distance);
                }
            }
            
            moves[i] = (maxDistance, x, y);
        }
        
        // Unity will hoist `index` so that its name clashes
        {
            moves.Sort(delegate((int, int, int) t1, (int, int, int) t2) {
                return t1.Item1.CompareTo(t2.Item1);
            });
            while(moves[moves.Count - 1].Item1 > moves[0].Item1) {
                moves.RemoveAt(moves.Count - 1);
            }
            int index = UnityEngine.Random.Range(0, moves.Count);
            return (moves[index].Item2, moves[index].Item3);
        }
    }
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;
    public static GameManager GetInstance() {   
        return instance;
    }
    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
        } else {
            instance = this;
        }
    }
    
    // Prefabs
    public GameObject GrassTilePrefab = null;
    public GameObject Grass2TilePrefab = null;
    public GameObject TowerPrefab = null;
    public GameObject alienSpawn = null;
    
    // These probably shouldn't be tunable from the editor yet
    int minX = -8;
    int maxX = 8;
    int minY = -4;
    int maxY = 4;
    
    // Member variables
    int health = 20;
    
    GameGrid gameGrid = null;
    
    private GameObject[,] towers = null;
    
    public GameObject HealthText = null;
    public GameObject GameOverScreen = null;
    public GameObject GameOverWaveText = null;
    
    int towerCost = 4;
    
    void Start() {
        int mapXSize = maxX - minX + 1;
        int mapYSize = maxY - minY + 1;
        gameGrid = new GameGrid(mapXSize, mapYSize);
        
        towers = new GameObject[mapXSize, mapYSize];
        
        for(int posY = 0; minY + posY <= maxY; posY++) {
            for (int posX = 0; minX + posX <= maxX; posX++) {
                GameObject tile = Instantiate(UnityEngine.Random.Range(0, 10) == 0 ? Grass2TilePrefab : GrassTilePrefab);
                tile.transform.position = new Vector3(minX + posX, minY + posY, 1.0f);
            }
        }
        
        // TODO: Make barn and alien spawn declare themselves to avoid hardcoding.
        // NOTE: The GameGrid class itself makes some assumptions based on these values.
        gameGrid.SetObject(0, maxY - minY, 1);
        gameGrid.SetObject(maxX - minX, 0, 2);
        
        gameGrid.GenerateDistances();
        
        SetHealth(20);
    }
    
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Vector3 mousePos3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            (int gridPositionX, int gridPositionY) = GetGridPosition(mousePos3.x, mousePos3.y);
            
            MoneyManager moneyManager = MoneyManager.GetInstance();
            int money = moneyManager.GetMoney();
            
            if(gameGrid.IsValidIndex(gridPositionX, gridPositionY) && gameGrid.GetObject(gridPositionX, gridPositionY) == GameGrid.OBJECT_EMPTY && money >= towerCost) {
                SpawnTower(gridPositionX, gridPositionY);
                   
                gameGrid.RegenerateDistances();
                
                (int gridPositionAlienSpawnX, int gridPositionAlienSpawnY) = GetGridPosition(alienSpawn.transform.position.x, alienSpawn.transform.position.y);
                if(gameGrid.GetDistance(gridPositionAlienSpawnX, gridPositionAlienSpawnY) > 1000) {
                    DestroyTower(gridPositionX, gridPositionY);
                    
                    gameGrid.RegenerateDistances();
                } else {
                    moneyManager.SetMoney(money - towerCost);
                }
            }
        } else if(Input.GetMouseButtonDown(1)) {
            Vector3 mousePos3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            (int gridPositionX, int gridPositionY) = GetGridPosition(mousePos3.x, mousePos3.y);
            
            MoneyManager moneyManager = MoneyManager.GetInstance();
            int money = moneyManager.GetMoney();
            
            if(gameGrid.IsValidIndex(gridPositionX, gridPositionY) && gameGrid.GetObject(gridPositionX, gridPositionY) == GameGrid.OBJECT_TOWER) {
                DestroyTower(gridPositionX, gridPositionY);
                   
                gameGrid.RegenerateDistances();
                
                moneyManager.SetMoney(money + (int)(towerCost * 0.75));
            }
        }
    }
    
    public (int, int) GetGridPosition(float xFloat, float yFloat) {
        int x = (int)Math.Round(xFloat);
        int y = (int)Math.Round(yFloat);
        x -= minX;
        y -= minY;
        
        return (x, y);
    }
    
    public (int, int) GetNextGridPosition((int, int) currentPosition, bool minimax) {
        if(!minimax) return gameGrid.GetNextPositionSimple(currentPosition);
        return gameGrid.GetNextPositionMinimax(currentPosition);
    }
    
    public Vector2 GridToWorldPosition((int, int) gridPosition) {
        return new Vector2(gridPosition.Item1 + minX, gridPosition.Item2 + minY);
    }
    
    public bool IsAtBarn((int, int) gridPosition) {
        // Debug.Log(gridPosition.Item1 + " " + gridPosition.Item2 + " = " + grid[gridPosition.Item1, gridPosition.Item2]);
        return gameGrid.GetObject(gridPosition.Item1, gridPosition.Item2) == GameGrid.OBJECT_BARN;
    }
    
    public void DecrementHealth() {        
        SetHealth(health - 1);
    }
    
    void SetHealth(int newHealth) {
        health = newHealth;
        
        if(health <= 0) {
            Time.timeScale = 0;
            
            GameOverScreen.SetActive(true);
            GameOverWaveText.GetComponent<TextMeshProUGUI>().SetText("Wave: " + SpawnManager.GetInstance().GetCurrentWave());
        } 
        
        HealthText.GetComponent<TextMeshProUGUI>().SetText("Cows: " + health);
    }
    
    void SpawnTower(int gridPositionX, int gridPositionY) {
        Vector2 worldPosition = GridToWorldPosition((gridPositionX, gridPositionY));
        GameObject tower = Instantiate(TowerPrefab);
        towers[gridPositionX, gridPositionY] = tower;
        tower.transform.position = new Vector3(worldPosition.x, worldPosition.y, 0.0f);
        
        gameGrid.SetObject(gridPositionX, gridPositionY, GameGrid.OBJECT_TOWER);
    }
    
    void DestroyTower(int gridPositionX, int gridPositionY) {
        gameGrid.SetObject(gridPositionX, gridPositionY, GameGrid.OBJECT_EMPTY);
        
        Destroy(towers[gridPositionX, gridPositionY]);
        towers[gridPositionX, gridPositionY] = null;
    }
    
    public void LoadMainMenu() {
        Time.timeScale = 1;
        SceneManager.LoadScene("main menu");
    }
    
    public void ReloadScene() {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
