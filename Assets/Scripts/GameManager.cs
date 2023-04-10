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
    public GameObject brightGrassTilePrefab = null;
    public GameObject TowerPrefab = null;
    
    // These probably shouldn't be tunable from the editor yet
    int minX = -8;
    int maxX = 8;
    int minY = -4;
    int maxY = 4;
    
    // Member variables
    int health = 5;
    
    // TODO: Merge maps with classes
    public int[,] grid = null;
    public int[,] distanceMap = null;
    
    public GameObject HealthText = null;
    
    void Start() {
        int mapXSize = maxX - minX + 1;
        int mapYSize = maxY - minY + 1;
        grid = new int[mapXSize, mapYSize];
        
        distanceMap = new int[mapXSize, mapYSize];
        
        for(int posY = 0; minY + posY <= maxY; posY++) {
            for (int posX = 0; minX + posX <= maxX; posX++) {
                if(posX % 2 ==  posY % 2) {
                    GameObject tile = Instantiate(brightGrassTilePrefab);
                    tile.transform.position = new Vector3(minX + posX, minY + posY, 1.0f);
                    
                    grid[posX, posY] = 0;
                }
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
    
    public (int, int) GetNextGridPosition((int, int) currentPosition) {
        int currentPositionX = currentPosition.Item1;
        int currentPositionY = currentPosition.Item2;
        int bestMoveX = -1;
        int bestMoveY = -1;
        int bestDistance = Int32.MaxValue / 2;
        
        if(currentPositionX + 1 < distanceMap.GetLength(0)) {
            bestMoveX = currentPositionX + 1;
            bestMoveY = currentPositionY;
            bestDistance = distanceMap[currentPositionX + 1, currentPositionY];
        }
        
        if(currentPositionX - 1 >= 0 && distanceMap[currentPositionX - 1, currentPositionY] < bestDistance) {
            bestMoveX = currentPositionX - 1;
            bestMoveY = currentPositionY;
            bestDistance = distanceMap[currentPositionX - 1, currentPositionY];
        }
        
        if(currentPositionY + 1 < distanceMap.GetLength(1) && distanceMap[currentPositionX, currentPositionY + 1] < bestDistance) {
            bestMoveX = currentPositionX;
            bestMoveY = currentPositionY + 1;
            bestDistance = distanceMap[currentPositionX, currentPositionY + 1];
        }
        
        if(currentPositionY - 1 >= 0 && distanceMap[currentPositionX, currentPositionY - 1] < bestDistance) {
            bestMoveX = currentPositionX;
            bestMoveY = currentPositionY - 1;
            bestDistance = distanceMap[currentPositionX, currentPositionY - 1];
        }
        
        return (bestMoveX, bestMoveY);
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
        
        HealthText.GetComponent<TextMeshProUGUI>().SetText("Health: " + health);
    }
    
    void SpawnTower(int gridPositionX, int gridPositionY) {
        Vector2 worldPosition = GridToWorldPosition((gridPositionX, gridPositionY));
        GameObject tower = Instantiate(TowerPrefab);
        tower.transform.position = new Vector3(worldPosition.x, worldPosition.y, 0.0f);
        
        grid[gridPositionX, gridPositionY] = 3;
    }
}