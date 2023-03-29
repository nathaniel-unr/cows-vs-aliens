using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alien : MonoBehaviour
{
    public float StepSize = 0.05f;
    
    private Vector3 target;
    bool hasTarget = false;
    int health = 3;
    
    void Start() {
        AlienManager.GetInstance().RegisterAlien(this);
    }
    
    void FixedUpdate() {
        if(!hasTarget) {
            GameManager gameManager = GameManager.GetInstance();
            (int, int) gridPosition = gameManager.GetGridPosition(transform.position.x, transform.position.y);
            if(gameManager.IsAtBarn(gridPosition)) {
                gameManager.DecrementHealth();
                AlienManager.GetInstance().RemoveAlien(this);
                return;
            }
                
            (int, int) nextGridPosition = gameManager.GetNextGridPosition(gridPosition);
            target = gameManager.GridToWorldPosition(nextGridPosition);
            target.z = -1.0f;
            hasTarget = true;
        }
        
        Vector3 diff = target - transform.position;
        if(diff.magnitude < StepSize) {
            transform.position = target;
            hasTarget = false;
        } else {
            transform.position += Vector3.Normalize(diff) * StepSize;
        }
    }
    
    public void ProcessHit() {
        health -= 1;
        if(health <=0) AlienManager.GetInstance().RemoveAlien(this);
    }
}
