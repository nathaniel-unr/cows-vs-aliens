using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    int cooldownCounter = 0;
    
    Alien target = null;
    public GameObject gun = null;
    public GameObject barrel = null;
    
    AudioSource audioData;
    
    void Start() {
        audioData = GetComponent<AudioSource>();
    }
    
    void Update() {
        if(target && target.gameObject != null) {
            Vector3 lookPos = target.gameObject.transform.position - gun.transform.position;
            Quaternion rotation = Quaternion.LookRotation(lookPos, Vector3.forward);
            
            gun.transform.rotation = Quaternion.Slerp(gun.transform.rotation, rotation, Time.deltaTime * 20.0f);
            gun.transform.eulerAngles = new Vector3(0, 0, gun.transform.eulerAngles.z); 
        }
        
        if(cooldownCounter >= 15) {
            Vector3 target = Vector3.Lerp(
                barrel.transform.localPosition, 
                new Vector3(
                    0.0f, 
                    -0.2f, 
                    0.0f
                ), 
                1.0f - ((cooldownCounter - 15.0f) / 15.0f)
            );
            
            barrel.transform.localPosition = target;
        } else if (cooldownCounter < 15) {
            Vector3 target = Vector3.Lerp(
                barrel.transform.localPosition, 
                new Vector3(
                    0.0f, 
                    -0.35f, 
                    0.0f
                ), 
                1.0f - (cooldownCounter / 15.0f)
            );
            
            barrel.transform.localPosition = target;
        }
    }
    
    void FixedUpdate() {
        AlienManager alienManager = AlienManager.GetInstance();
        List<Alien> aliens = alienManager.GetAliens();
        bool found = false;
        for(int i = 0; i < aliens.Count; i++) {
            if((aliens[i].transform.position - this.transform.position).magnitude < 2.0f) {
                target = aliens[i];
                
                if(cooldownCounter <= 0) {
                    audioData.Play(0);
        
                    aliens[i].ProcessHit();
                    cooldownCounter = 30;
                }
                
                found = true;
                
                break;
            }
        }
        
        if(!found) target = null;
        
        cooldownCounter -= 1;
    }
}
