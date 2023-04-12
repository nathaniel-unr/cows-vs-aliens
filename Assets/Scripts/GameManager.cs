using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

class GameGrid {
    public const int OBJECT_EMPTY = 0;
    
    private int[,] distances = null;
    private int[,] objects = null;
    
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
            if(distance >= distances[x, y] || objects[x, y] == 3) continue;
            
            distances[x, y] = distance;
            
            if(x + 1 < distances.GetLength(0) && distances[x + 1, y] > distance + 1) {
                queue.Enqueue((x + 1, y, distance + 1));
            } 
            if(y + 1 < distances.GetLength(1) && distances[x, y + 1] > distance + 1) {
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
            
            if(gameGrid.IsValidIndex(gridPositionX, gridPositionY) && gameGrid.GetObject(gridPositionX, gridPositionY) == GameGrid.OBJECT_EMPTY) {
                SpawnTower(gridPositionX, gridPositionY);
                   
                gameGrid.RegenerateDistances();
                
                (int gridPositionAlienSpawnX, int gridPositionAlienSpawnY) = GetGridPosition(alienSpawn.transform.position.x, alienSpawn.transform.position.y);
                if(gameGrid.GetDistance(gridPositionAlienSpawnX, gridPositionAlienSpawnY) > 1000) {
                    DestroyTower(gridPositionX, gridPositionY);
                    
                    gameGrid.RegenerateDistances();
                }
            }
        } else if(Input.GetMouseButtonDown(1)) {
            Vector3 mousePos3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            (int gridPositionX, int gridPositionY) = GetGridPosition(mousePos3.x, mousePos3.y);
            
            if(gameGrid.IsValidIndex(gridPositionX, gridPositionY) && gameGrid.GetObject(gridPositionX, gridPositionY) == 3) {
                DestroyTower(gridPositionX, gridPositionY);
                   
                gameGrid.RegenerateDistances();
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
        int currentPositionX = currentPosition.Item1;
        int currentPositionY = currentPosition.Item2;
        
        List<(int, int, int)> moves = new List<(int, int, int)>();
        
        if(currentPositionX + 1 < gameGrid.GetSizeX() && gameGrid.GetDistance(currentPositionX + 1, currentPositionY) < 1000) {            
            moves.Add((
                gameGrid.GetDistance(currentPositionX + 1, currentPositionY),
                currentPositionX + 1,
                currentPositionY
            ));
        }
        
        if(currentPositionX - 1 >= 0 && gameGrid.GetDistance(currentPositionX - 1, currentPositionY) < 1000) {            
            moves.Add((
                gameGrid.GetDistance(currentPositionX - 1, currentPositionY),
                currentPositionX - 1,
                currentPositionY
            ));
        }
        
        if(currentPositionY + 1 < gameGrid.GetSizeY() && gameGrid.GetDistance(currentPositionX, currentPositionY + 1) < 1000) {            
            moves.Add((
                gameGrid.GetDistance(currentPositionX, currentPositionY + 1),
                currentPositionX,
                currentPositionY + 1
            ));
        }
        
        if(currentPositionY - 1 >= 0 && gameGrid.GetDistance(currentPositionX, currentPositionY - 1) < 1000) {
            moves.Add((
                gameGrid.GetDistance(currentPositionX, currentPositionY - 1),
                currentPositionX,
                currentPositionY - 1
            ));
        }
        
        if(moves.Count == 0) {
            return (-1, -1);
        }
        
        moves.Sort(delegate((int, int, int) t1, (int, int, int) t2) {
            return t1.Item1.CompareTo(t2.Item1);
        });
        
        if(moves.Count == 1 || !minimax || gameGrid.GetDistance(currentPositionX, currentPositionY) < moves[1].Item1) {
            return (moves[0].Item2, moves[0].Item3);
        }
        
        return (moves[1].Item2, moves[1].Item3);
    }
    
    public Vector2 GridToWorldPosition((int, int) gridPosition) {
        return new Vector2(gridPosition.Item1 + minX, gridPosition.Item2 + minY);
    }
    
    public bool IsAtBarn((int, int) gridPosition) {
        // Debug.Log(gridPosition.Item1 + " " + gridPosition.Item2 + " = " + grid[gridPosition.Item1, gridPosition.Item2]);
        return gameGrid.GetObject(gridPosition.Item1, gridPosition.Item2) == 1;
    }
    
    public void DecrementHealth() {        
        SetHealth(health - 1);
    }
    
    void SetHealth(int newHealth) {
        health = newHealth;
        
        if(health <= 0) {
            // TODO: Game Over
        } 
        
        HealthText.GetComponent<TextMeshProUGUI>().SetText("Cows: " + health);
    }
    
    void SpawnTower(int gridPositionX, int gridPositionY) {
        Vector2 worldPosition = GridToWorldPosition((gridPositionX, gridPositionY));
        GameObject tower = Instantiate(TowerPrefab);
        towers[gridPositionX, gridPositionY] = tower;
        tower.transform.position = new Vector3(worldPosition.x, worldPosition.y, 0.0f);
        
        gameGrid.SetObject(gridPositionX, gridPositionY, 3);
    }
    
    void DestroyTower(int gridPositionX, int gridPositionY) {
        gameGrid.SetObject(gridPositionX, gridPositionY, GameGrid.OBJECT_EMPTY);
        
        Destroy(towers[gridPositionX, gridPositionY]);
        towers[gridPositionX, gridPositionY] = null;
    }
}
