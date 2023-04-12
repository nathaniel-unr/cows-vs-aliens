using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour {
    private static MoneyManager instance = null;
    public static MoneyManager GetInstance() {   
        return instance;
    }
    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
        } else {
            instance = this;
        }
    }
    
    public GameObject MoneyText = null;
    
    // Member variables
    public int Money = 20;
    
    void Start() {
        SetMoney(Money);
    }
    
    public int GetMoney() {
        return Money;
    }
    
    public void SetMoney(int newMoney) {
        Money = newMoney;
        MoneyText.GetComponent<TextMeshProUGUI>().SetText("Money: $" + Money);
    }
}
