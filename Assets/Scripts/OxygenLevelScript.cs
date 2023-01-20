using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OxygenLevelScript : MonoBehaviour
{
    public GameManagerScript gameManager;
    public static OxygenLevelScript oxyScript;
    public Slider oxygenBar; //slider in UI
    public int currOxygen = 0;
    public int maxOxygen = 100;
    public int oxygenDepletionRate = 10;
    private int oxygenIncrease = 5;

    void Start()
    {
        oxygenBar = GetComponent<Slider>();
    }

    // sets up coroutine and variables
    public void StartOxygenDepletion(){
        currOxygen = maxOxygen;
        SetOxygen(maxOxygen);
        StartCoroutine(ReduceOxygen());
    }

    // sets oxygen level
    public void SetOxygen(int oxy) {
        oxygenBar.value = oxy;
    }

    // method to increase oxygen when coin is picked up
    public void IncreaseOxygen() {
        if((currOxygen+=oxygenIncrease) <= maxOxygen) {
            currOxygen+=oxygenIncrease;
            SetOxygen(currOxygen);
        }
        else {
            currOxygen = maxOxygen;
            SetOxygen(currOxygen);
        }
    }

	// method calls EndGame method from gameManager
    public void EndGame() {
        gameManager.EndGame();
    }

	// coroutine to reduce oxygen
    IEnumerator ReduceOxygen() {
        while (currOxygen > 0) {
            // if game is unpaused
            if (AstronautAnimationScript.gameOn){
                yield return new WaitForSeconds(30);
                currOxygen -= oxygenDepletionRate;
                SetOxygen(currOxygen);
            }
        }
        // if player runs out of oxygen, set up game over screen
        GameManagerScript.instance.NoMoreOxygen();
        StopCoroutine(ReduceOxygen());
    }
}
