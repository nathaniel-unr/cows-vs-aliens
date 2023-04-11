using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

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
    
    // TODO: Merge maps with classes
    public int[,] grid = null;
    public int[,] distanceMap = null;
    
    private GameObject[,] towers = null;
    
    public GameObject HealthText = null;
    
    void Start() {
        int mapXSize = maxX - minX + 1;
        int mapYSize = maxY - minY + 1;
        grid = new int[mapXSize, mapYSize];
        
        distanceMap = new int[mapXSize, mapYSize];
        
        towers = new GameObject[mapXSize, mapYSize];
        
        for(int posY = 0; minY + posY <= maxY; posY++) {
            for (int posX = 0; minX + posX <= maxX; posX++) {
                // posX % 2 == posY % 2
                GameObject tile = Instantiate(UnityEngine.Random.Range(0, 10) == 0 ? Grass2TilePrefab : GrassTilePrefab);
                tile.transform.position = new Vector3(minX + posX, minY + posY, 1.0f);
                grid[posX, posY] = 0;
            }
        }
        
        // TODO: Make barn and alien spawn declare themselves to avoid hardcoding
        grid[0, maxY - minY] = 1;
        grid[maxX - minX, 0] = 2;

        ClearDistanceMap();
        GenerateDistanceMap();
        
        SetHealth(20);
    }
    
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Vector3 mousePos3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            (int gridPositionX, int gridPositionY) = GetGridPosition(mousePos3.x, mousePos3.y);
            
            if(gridPositionX >= 0 && 
               gridPositionX < grid.GetLength(0) && 
               gridPositionY >= 0 && 
               gridPositionY < grid.GetLength(1) &&
               grid[gridPositionX, gridPositionY] == 0) {
                SpawnTower(gridPositionX, gridPositionY);
                   
                ClearDistanceMap();
                GenerateDistanceMap();
                
                (int gridPositionAlienSpawnX, int gridPositionAlienSpawnY) = GetGridPosition(alienSpawn.transform.position.x, alienSpawn.transform.position.y);
                if(distanceMap[gridPositionAlienSpawnX, gridPositionAlienSpawnY] > 1000) {
                    DestroyTower(gridPositionX, gridPositionY);
                    
                    ClearDistanceMap();
                    GenerateDistanceMap();
                }
            }
        } else if(Input.GetMouseButtonDown(1)) {
            Vector3 mousePos3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            (int gridPositionX, int gridPositionY) = GetGridPosition(mousePos3.x, mousePos3.y);
            
            if(gridPositionX >= 0 && 
               gridPositionX < grid.GetLength(0) && 
               gridPositionY >= 0 && 
               gridPositionY < grid.GetLength(1) &&
               grid[gridPositionX, gridPositionY] == 3) {
                DestroyTower(gridPositionX, gridPositionY);
                   
                ClearDistanceMap();
                GenerateDistanceMap();
            }
        }
    }
    
    void ClearDistanceMap() {
        for(int i = 0; i < distanceMap.GetLength(0); i++) {
            for (int j = 0; j < distanceMap.GetLength(1); j++) {
                distanceMap[i, j] = Int32.MaxValue / 2;
            }
        }
    }
    
    void GenerateDistanceMap() {
        Queue<(int, int, int)> queue = new Queue<(int, int, int)>();
        
        queue.Enqueue((0, maxY - minY, 0));
        
        while(queue.Count > 0) {
            (int x, int y, int distance) = queue.Dequeue();
            if(distance >= distanceMap[x, y] || grid[x,y] == 3) continue;
            
            distanceMap[x, y] = distance;
            
            if(x + 1 < distanceMap.GetLength(0) && distanceMap[x + 1, y] > distance + 1) {
                queue.Enqueue((x + 1, y, distance + 1));
            } 
            if(y + 1 < distanceMap.GetLength(1) && distanceMap[x, y + 1] > distance + 1) {
                queue.Enqueue((x, y + 1, distance + 1));
            }
            if(x - 1 >= 0 && distanceMap[x - 1, y] > distance + 1) {
                queue.Enqueue((x - 1, y, distance + 1));
            }
            if(y - 1 >= 0 && distanceMap[x, y - 1] > distance + 1) {
                queue.Enqueue((x, y - 1, distance + 1));
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
        
        if(currentPositionX + 1 < distanceMap.GetLength(0) && distanceMap[currentPositionX + 1, currentPositionY] < 1000) {            
            moves.Add((
                distanceMap[currentPositionX + 1, currentPositionY],
                currentPositionX + 1,
                currentPositionY
            ));
        }
        
        if(currentPositionX - 1 >= 0 && distanceMap[currentPositionX - 1, currentPositionY] < 1000) {            
            moves.Add((
                distanceMap[currentPositionX - 1, currentPositionY],
                currentPositionX - 1,
                currentPositionY
            ));
        }
        
        if(currentPositionY + 1 < distanceMap.GetLength(1) && distanceMap[currentPositionX, currentPositionY + 1] < 1000) {            
            moves.Add((
                distanceMap[currentPositionX, currentPositionY + 1],
                currentPositionX,
                currentPositionY + 1
            ));
        }
        
        if(currentPositionY - 1 >= 0 && distanceMap[currentPositionX, currentPositionY - 1] < 1000) {
            moves.Add((
                distanceMap[currentPositionX, currentPositionY - 1],
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
        
        if(moves.Count == 1 || !minimax || distanceMap[currentPositionX, currentPositionY] < moves[1].Item1) {
            return (moves[0].Item2, moves[0].Item3);
        }
        
        return (moves[1].Item2, moves[1].Item3);
    }
    
    public Vector2 GridToWorldPosition((int, int) gridPosition) {
        return new Vector2(gridPosition.Item1 + minX, gridPosition.Item2 + minY);
    }
    
    public bool IsAtBarn((int, int) gridPosition) {
        // Debug.Log(gridPosition.Item1 + " " + gridPosition.Item2 + " = " + grid[gridPosition.Item1, gridPosition.Item2]);
        return grid[gridPosition.Item1, gridPosition.Item2] == 1;
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
        
        grid[gridPositionX, gridPositionY] = 3;
    }
    
    void DestroyTower(int gridPositionX, int gridPositionY) {
        grid[gridPositionX, gridPositionY] = 0;
        Destroy(towers[gridPositionX, gridPositionY]);
        towers[gridPositionX, gridPositionY] = null;
    }
}
