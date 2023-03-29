using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    int cooldownCounter = 0;
    
    void Start() {
        
    }
    
    void Update() {
        
    }
    
    void FixedUpdate() {
        if(cooldownCounter <= 0) {
            AlienManager alienManager = AlienManager.GetInstance();
            List<Alien> aliens = alienManager.GetAliens();
            for(int i = 0; i < aliens.Count; i++) {
                if((aliens[i].transform.position - this.transform.position).magnitude < 2.0f) {
                    aliens[i].ProcessHit();
                    cooldownCounter = 30;
                    break;
                }
            }
        }
        
        cooldownCounter -= 1;
    }
}
