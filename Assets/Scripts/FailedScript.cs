using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FailedScript : MonoBehaviour
{
    public GameManagerScript gameManager;
    private Color transparencyBlack;
    public float fadingSpeed;
    
    // Start is called before the first frame update
    void Start()
    {
        transparencyBlack = GetComponent<Image>().color;
        SetBackgroundAndTextTransparent();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /**
    * coroutine to fade screen to fail screen and finally the main menu
    */
    public IEnumerator FadeToFailScreen(){
        while (transparencyBlack.a < 1){
            // a value (transparency) of the image colour is increased 
            transparencyBlack.a += (fadingSpeed * Time.deltaTime);
            // image colour is set to altered
            GetComponent<Image>().color = transparencyBlack;
            yield return null;
        }
        yield return new WaitForSeconds(3);
        SetBackgroundAndTextTransparent();
        gameManager.MainMenu();
        StopCoroutine(FadeToFailScreen());
    }

    // black screen is removed
    public void SetBackgroundAndTextTransparent(){
        transparencyBlack.a = 0;
        GetComponent<Image>().color = transparencyBlack;
    }
}
