using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private static SpawnManager instance = null;
    public static SpawnManager GetInstance() {   
        return instance;
    }
    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
        } else {
            instance = this;
        }
    }
    
    public GameObject AlienPrefab = null;
    public GameObject alienSpawn = null;
    
    int nextWaveCount = 1;
    
    void Start() {
        
    }

    void Update() {
        if(AlienManager.GetInstance().GetAliens().Count == 0) {
            SpawnWave();
        }
    }
    
    void SpawnAlien(Vector3 position) {
        GameObject alien = Instantiate(AlienPrefab);
        alien.transform.position = new Vector3(position.x, position.y, -1.0f);
    }
    
    void SpawnWave() {
        for(int i = 0; i < nextWaveCount; i++) {
            SpawnAlien(alienSpawn.transform.position);
        }
        
        nextWaveCount += 1;
    }
}
