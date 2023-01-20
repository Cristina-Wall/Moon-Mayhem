using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManagerScript : MonoBehaviour
{
    public enum GAMESTATES {
        MENU,
        PAUSED,
        START,
        RESUME,
        OPTIONS,
        HOWTOPLAY,
        FAILED,
        ANIMATION
    }
    
    public static GameManagerScript instance;
    public GameObject titleScreen, pausedScreen, optionsScreen, inGameScreen, howToPlayScreen, failedScreen, animationScreen;
    private static int previousScreen;
    public FailedScript failScript;
    public Camera staticCamera;
    public static bool gameStarted = false;
    public static GAMESTATES gameState = GAMESTATES.MENU;
    public GameObject astronautPrefab;
    private static GameObject astronaut;
    public OxygenLevelScript oxyScript;
    public GameObject rocketPrefab;
    public GameObject rocketWithCameraAngledPrefab;
    public GameObject rocketWithCameraStraightOnPrefab;
    public GameObject rocketBodyPrefab;
    public GameObject rocketBottomPrefab;
    public GameObject rocketLeg1Prefab;
    public GameObject rocketLeg2Prefab;
    public GameObject rocketLeg3Prefab;
    public GameObject rocketLeg4Prefab;
    public GameObject rocketTopPrefab;
    public GameObject rocketWindow1Prefab;
    public GameObject rocketWindow2Prefab;
    public GameObject coinPrefab;
    public GameObject crashSource;
    public Slider soundSlider;
    public Slider musicSlider;
    public float optSoundsVolume = 1.0f;
    public float optMusicVolume = 1.0f;
    private static List<GameObject> gameObjectsList;
    public GameObject lightBeam;
    public static int collectedRocketParts = 0;
    public static int totalRocketParts = 9;
    private static Vector3 lightBeamPosition = new Vector3(0f, 30f, -15f);
    private static Vector3 rocketPosition;
    private static Vector3 rocketRotation;
    
    // Start is called before the first frame update
    void Start()
    {  
        previousScreen = 1;
        instance=this;
        staticCamera.enabled = true;
        SetGameState (GAMESTATES.MENU);
        gameObjectsList = new List<GameObject>();

        soundSlider.onValueChanged.AddListener(delegate { SetSoundVolume(soundSlider.value); });
        musicSlider.onValueChanged.AddListener(delegate { SetMusicVolume(musicSlider.value); });
    }

    /**
    * UI control method
    */
    public static void SetGameState(GAMESTATES state) {
        gameState = state;
        switch (state) {
            case GAMESTATES.MENU:
                SetPreviousScreen(GAMESTATES.MENU);
                instance.titleScreen.SetActive (true);
                instance.inGameScreen.SetActive(false);
                instance.pausedScreen.SetActive(false);
                instance.optionsScreen.SetActive(false);
                instance.howToPlayScreen.SetActive (false);
                instance.failedScreen.SetActive(false);
                instance.animationScreen.SetActive(false);
            break;

            case GAMESTATES.START:
                instance.inGameScreen.SetActive(true);
                instance.titleScreen.SetActive(false);
                instance.howToPlayScreen.SetActive (false);
                instance.animationScreen.SetActive(false);
                StartNewGame();
            break;
         
            case GAMESTATES.OPTIONS:
                instance.titleScreen.SetActive (false);
                instance.optionsScreen.SetActive (true);
                instance.howToPlayScreen.SetActive (false);
                instance.pausedScreen.SetActive (false);
            break;

            case GAMESTATES.PAUSED:
                SetPreviousScreen(GAMESTATES.PAUSED);
                instance.inGameScreen.SetActive (false);
                instance.pausedScreen.SetActive (true);
            break;

            case GAMESTATES.RESUME:
                instance.pausedScreen.SetActive (false);
                instance.inGameScreen.SetActive(true);
            break;

            case GAMESTATES.HOWTOPLAY:
                instance.titleScreen.SetActive (false);
                instance.howToPlayScreen.SetActive (true);
                instance.howToPlayScreen.transform.GetChild(1).gameObject.SetActive(true);
                instance.howToPlayScreen.transform.GetChild(6).gameObject.SetActive(true);
                instance.howToPlayScreen.transform.GetChild(7).gameObject.SetActive(true); 
                instance.howToPlayScreen.transform.GetChild(2).gameObject.SetActive(false);
                instance.howToPlayScreen.transform.GetChild(3).gameObject.SetActive(false);
                instance.howToPlayScreen.transform.GetChild(5).gameObject.SetActive(false);
            break;

            case GAMESTATES.FAILED:
                instance.failedScreen.SetActive(true);
                instance.pausedScreen.SetActive (false);
                instance.inGameScreen.SetActive(false);
            break;

            case GAMESTATES.ANIMATION:
                instance.animationScreen.SetActive(true);
                instance.titleScreen.SetActive(false);
                instance.howToPlayScreen.SetActive (false);
                instance.inGameScreen.SetActive(false);
                instance.pausedScreen.SetActive (false);
                instance.optionsScreen.SetActive(false);
                instance.failedScreen.SetActive(false);
            break;
        }
    }

    /**
    * sets up level for new game
    * resets collected parts, spawns astronaut and rocket pieces
    */
    public static void StartNewGame() {
        collectedRocketParts = 0;
        // disable menu camera
        instance.staticCamera.enabled = false;
        astronaut = Instantiate(instance.astronautPrefab);
        // objects are added to list to allow easy management
        gameObjectsList.Add(astronaut); 
        InstantiateRocketParts();
        // indicates that the game has started
        gameStarted = true; 
        // indicates that the game is running - used to check whether game is paused
        AstronautAnimationScript.gameOn = true; 
    }

    /**
    * starts game
    */
    public void StartGame() {
        SetGameState(GAMESTATES.ANIMATION);
        StartCoroutine(StartAnimationCoroutine());
    }

    public void NoMoreOxygen(){
        // enable menu camera
        staticCamera.enabled = true;
        // set game screen to failed
        SetGameState(GAMESTATES.FAILED);
        // start fade in coroutine
        StartCoroutine(failScript.FadeToFailScreen());
    }

    /**
    * winning game events
    */
    public void WinGame() {
        if(astronaut!=null) {
            lightBeam = Instantiate(instance.lightBeam) as GameObject;
            lightBeam.transform.position = lightBeamPosition;
            //give the rocket a position
            rocketPosition = lightBeamPosition;
            //Instantiate the rocket at this position whith the same rotation as the prefab
            GameObject rocket = Instantiate(instance.rocketPrefab, rocketPosition, Quaternion.identity);
            //add the rocket gameobject to the gameObjectsList 
            gameObjectsList.Add(rocket);
            //Instantiate a line of coins to guide the player to the rocket
            Vector3 startpos = astronaut.transform.position;
            Vector3 endpos = rocket.transform.position;
            Vector3 direction = endpos - startpos;
            float distance = direction.magnitude;
            int numCoins = 30;
            float spacing = distance/(numCoins+1);
            for(int i=1; i<=numCoins; i++) {
                Vector3 coinpos = startpos + direction.normalized * (i * spacing);
                GameObject coin = Instantiate(instance.coinPrefab, coinpos, Quaternion.identity);
            }
            
        }
    }
    public void EndGame() {
        SetGameState(GAMESTATES.ANIMATION);
        StartCoroutine(EndAnimationCoroutine());
    }

    public void ExitGame(){
        // allows exit from application
        Application.Quit();
    }

    public void OptionsON(){
        // set UI to the options screen
        SetGameState (GAMESTATES.OPTIONS);
    }

    /**
    * returns player to menu screen
    * finishes game
    */
    public void MainMenu(){
        // set UI to menu screen
        SetGameState (GAMESTATES.MENU);
        // enables menu camera
        staticCamera.enabled = true;
        // resets game assets
        ResetGame();
    }

    /**
    * sets UI to play directions
    */
    public void HowToPlay(){
        SetGameState (GAMESTATES.HOWTOPLAY);
    }

    /**
    * controls back arrow button
    */
    public void BackButton(){
        if (previousScreen == 1){
            instance.pausedScreen.SetActive(true);
            instance.optionsScreen.SetActive(false);
            instance.howToPlayScreen.SetActive(false);
        }
        else if(previousScreen == 2){
            instance.titleScreen.SetActive(true);
            instance.optionsScreen.SetActive(false);
            instance.howToPlayScreen.SetActive(false);
        }
    }

    /**
    * used to indicate which screen to set when back arrow is pressed
    */
    public static void SetPreviousScreen(GAMESTATES state) 
    {
        if (state == GAMESTATES.MENU){
            previousScreen = 2;
        }
        else if (state == GAMESTATES.PAUSED){
            previousScreen = 1;
        } 
    }

    /**
    * changes directions shown in how to play UI to the game's plot
    */
    public static void SeePlot(){
        instance.howToPlayScreen.transform.GetChild(1).gameObject.SetActive(false);
        instance.howToPlayScreen.transform.GetChild(6).gameObject.SetActive(false);
        instance.howToPlayScreen.transform.GetChild(7).gameObject.SetActive(false);
        instance.howToPlayScreen.transform.GetChild(2).gameObject.SetActive(true);
        instance.howToPlayScreen.transform.GetChild(3).gameObject.SetActive(true);
        instance.howToPlayScreen.transform.GetChild(5).gameObject.SetActive(true);
    }

    /**
    * sets UI to paused screen
    */
    public static void PauseGame(){
        SetGameState (GAMESTATES.PAUSED);
        AstronautAnimationScript.gameOn = false; // game is paused
    }

    /**
    *  returns in-game UI to screen
    */
    public static void ResumeGame(){
        SetGameState (GAMESTATES.RESUME);
        instance.optionsScreen.SetActive(false);
        AstronautAnimationScript.gameOn = true; // game is unpaused
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // if game started, pausing of game is permitted using esc key
        if (gameStarted){
            if (Input.GetKeyDown(KeyCode.Escape) && AstronautAnimationScript.gameOn == true) {
                PauseGame();
            }
            else if (Input.GetKeyDown(KeyCode.Escape) && AstronautAnimationScript.gameOn == false) {
                ResumeGame();
            }
        }
    }

    public static void InstantiateRocketParts() {
        GameObject rocketBody = Instantiate(instance.rocketBodyPrefab) as GameObject;        
        rocketBody.transform.position = new Vector3(29f, -32f, 254f);
        gameObjectsList.Add(rocketBody);
        GameObject rocketBottom = Instantiate(instance.rocketBottomPrefab) as GameObject;
        rocketBottom.transform.position = new Vector3(-83f, -31f, -119f);
        gameObjectsList.Add(rocketBottom);
        GameObject rocketLeg1 = Instantiate(instance.rocketLeg1Prefab) as GameObject;
        rocketLeg1.transform.position = new Vector3(136f, -30f, -56f);
        gameObjectsList.Add(rocketLeg1);
        GameObject rocketLeg2 = Instantiate(instance.rocketLeg2Prefab) as GameObject;
        rocketLeg2.transform.position = new Vector3(466f, -36f, 159f);
        gameObjectsList.Add(rocketLeg2);
        GameObject rocketLeg3 = Instantiate(instance.rocketLeg3Prefab) as GameObject;
        rocketLeg3.transform.position = new Vector3(211f, -30f, 255f);
        gameObjectsList.Add(rocketLeg3);
        GameObject rocketLeg4 = Instantiate(instance.rocketLeg4Prefab) as GameObject;
        rocketLeg4.transform.position = new Vector3(-212f, -30f, 320f);
        gameObjectsList.Add(rocketLeg4);
        GameObject rocketTop = Instantiate(instance.rocketTopPrefab) as GameObject;
        rocketTop.transform.position = new Vector3(364f, -35f, 53f);
        gameObjectsList.Add(rocketTop);
        GameObject rocketWindow1 = Instantiate(instance.rocketWindow1Prefab) as GameObject;
        rocketWindow1.transform.position = new Vector3(272f, -24f, 409f);
        gameObjectsList.Add(rocketWindow1);
        GameObject rocketWindow2 = Instantiate(instance.rocketWindow2Prefab) as GameObject;
        rocketWindow2.transform.position = new Vector3(465f, -23f, -133f);
        gameObjectsList.Add(rocketWindow2);
    }

    /**
    * generate coins
    */
    public void InstantiateCoins() {
        //create coins
        for(int i = 0; i<=20; i++) {
            float randx = Random.Range(-500, 500);
            float randz = Random.Range(-500, 500);
            Vector3 semiRandPosition = astronaut.transform.position + new Vector3(randx,100,randz);
            GameObject coin = Instantiate(instance.coinPrefab, semiRandPosition, Quaternion.identity);
        }
    }

	/**
    * deletes cloned prefabs
    * clears list
    */
    public void ResetGame() {
        for (int i = 0;i<gameObjectsList.Count;i++){
            Destroy(gameObjectsList[i]);
        }
        gameObjectsList.Clear();
        // game has not started
        gameStarted = false;
    }

    public void SetSoundVolume(float soundValue){
        optSoundsVolume = soundValue;
    }

    public void SetMusicVolume(float musicValue){
        optMusicVolume = musicValue;
    }

    /**
    * collect rocket part 
    */
    public void CollectPart(){
        // piece is destroyed
        Destroy(AstronautAnimationScript.selectedPart);
        // amount of pieces is increased
        collectedRocketParts++;
        // amount is updated in the text
        PiecesFoundUpdater.SetCollectedRocketParts(collectedRocketParts);
        // disable pop up message
        inGameScreen.transform.GetChild(3).gameObject.SetActive(false);
        // reset selected part
        AstronautAnimationScript.selectedPart = null;
        // spawn light beam if all parts collected
        if(collectedRocketParts == totalRocketParts) {
            WinGame();
        }
    }

    /**
    * coroutine for starting animation
    */
    public IEnumerator StartAnimationCoroutine() {
        GameObject animRocket = Instantiate(instance.rocketWithCameraAngledPrefab) as GameObject;
        rocketPosition = new Vector3(100f, 100f, 320f);
        rocketRotation = new Vector3(-110f, 90f, 40f);
        animRocket.transform.position = rocketPosition;
        animRocket.transform.Rotate(-180f, 0f, 30f, Space.Self);
        //animRocket.transform.eulerAngles(rocketRotation);

        animationScreen.transform.GetChild(1).gameObject.SetActive(true); // set screen to black
        yield return new WaitForSeconds(1);
        animationScreen.transform.GetChild(1).gameObject.SetActive(false); // get rid of black screen
        animationScreen.transform.GetChild(0).gameObject.SetActive(true); // background colour

        for(int i=0; i<100; i++) {
            rocketPosition = rocketPosition + new Vector3(-1f, -1f, 0f);
            animRocket.transform.position = rocketPosition;            
            if(i==80) {
                GameObject crash = Instantiate(crashSource, rocketPosition, Quaternion.identity);
            }
            yield return null;
        }
        animRocket.transform.GetChild(15).GetComponent<Camera>().enabled = false;
        staticCamera.enabled = true;
        Destroy(animRocket);
        animationScreen.transform.GetChild(0).gameObject.SetActive(false); // no background colour
        animationScreen.transform.GetChild(1).gameObject.SetActive(true); // black screen
        yield return new WaitForSeconds(1);
        animationScreen.transform.GetChild(1).gameObject.SetActive(false); //no black screen
        // set UI to the in-game screen
        SetGameState (GAMESTATES.START);
        // start oxygen depletion coroutine
        oxyScript.StartOxygenDepletion();
        //initiate coins
		InstantiateCoins();
        StopCoroutine(StartAnimationCoroutine());
    }

    /**
    * coroutine for ending animation
    */
    public IEnumerator EndAnimationCoroutine() {
        yield return new WaitForSeconds(1);
        for (int i = 0;i<gameObjectsList.Count;i++){
            Destroy(gameObjectsList[i]);
        }
        gameObjectsList.Clear();
        Destroy(lightBeam);

        GameObject animRocket = Instantiate(instance.rocketWithCameraStraightOnPrefab) as GameObject;
        rocketPosition = lightBeamPosition + new Vector3(0f, -30f, 0f);
        animRocket.transform.position = rocketPosition;

        animationScreen.transform.GetChild(1).gameObject.SetActive(true); //black screen
        yield return new WaitForSeconds(2);
        animationScreen.transform.GetChild(1).gameObject.SetActive(false); //no black screen
        animationScreen.transform.GetChild(0).gameObject.SetActive(true);

        yield return new WaitForSeconds(2);
        for(int i=0; i<600; i++) {
            rocketPosition = rocketPosition + new Vector3(1f, 1f, 0f);
            animRocket.transform.position = rocketPosition;
            yield return null;
        }
        
        animationScreen.transform.GetChild(0).gameObject.SetActive(false);
        animationScreen.transform.GetChild(1).gameObject.SetActive(true); //black screen
        yield return new WaitForSeconds(2);
        Destroy(animRocket);
        
        MainMenu();
        StopCoroutine(EndAnimationCoroutine());
    }
}
