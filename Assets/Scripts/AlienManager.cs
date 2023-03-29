using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienManager : MonoBehaviour
{
    private static AlienManager instance = null;
    public static AlienManager GetInstance() {   
        return instance;
    }
    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
        } else {
            instance = this;
        }
    }
    
    public List<Alien> aliens = new List<Alien>();
   
    void Start() {
        
    }
    void Update() {
        
    }
    
    public void RegisterAlien(Alien alien) {
        aliens.Add(alien);
    }
    
    public void RemoveAlien(Alien alien) {
        aliens.Remove(alien);
        Destroy(alien.gameObject);
    }
    
    public List<Alien> GetAliens() {
        return aliens;
    }
}
