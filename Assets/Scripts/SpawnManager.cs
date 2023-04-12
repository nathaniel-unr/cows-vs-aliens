using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    
    public GameObject WaveText = null;
    
    // Prefabs
    public GameObject AlienPrefab = null;
    public GameObject alienSpawn = null;
    
    
    public int CurrentWave = 1;
    
    int spawnRemaining = 0;
    int nextAlienSpawnSleep = 0;
    
    void Start() {
        SetCurrentWave(CurrentWave);
    }

    void Update() {
        if(AlienManager.GetInstance().GetAliens().Count == 0 && spawnRemaining == 0) {
            SpawnWave();
        }
    }
    
    void FixedUpdate() {
        if(spawnRemaining > 0 && nextAlienSpawnSleep <= 0) {
            SpawnAlien(alienSpawn.transform.position);
            spawnRemaining -= 1;
            nextAlienSpawnSleep = 25;
        }
        nextAlienSpawnSleep -= 1;
    }
    
    void SpawnAlien(Vector3 position) {
        GameObject alien = Instantiate(AlienPrefab);
        alien.transform.position = new Vector3(position.x, position.y, -1.0f);
    }
    
    void SpawnWave() {
        spawnRemaining = CurrentWave;
        SetCurrentWave(CurrentWave + 1);
    }
    
    void SetCurrentWave(int newCurrentWave) {
        CurrentWave = newCurrentWave;
        WaveText.GetComponent<TextMeshProUGUI>().SetText("Wave: " + CurrentWave);
    }
    
    public int GetCurrentWave() {
        return CurrentWave;
    } 
}
