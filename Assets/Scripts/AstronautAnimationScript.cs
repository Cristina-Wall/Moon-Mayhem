using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AstronautAnimationScript : MonoBehaviour
{
    Animator animator;
    public static bool gameOn, sprintEnabled;
    private CharacterController cc;
    public Transform feetPosition;
    public LayerMask mask;
    public Camera thirdPersonPerspective;
    public GameObject astronaut;
    public float movementSpeed, jumpForce, rotationSpeed, sprintSpeed;
    private float speed;
    private AudioSource breathing;
    public AudioSource backgroundMusic;
    public Slider staminaBar;
    public static GameObject selectedPart = null;
    private float yPosition = 0f;
    private float gravityValue = -1.62f;
    private float sprintMeter = 0f;
    private OxygenLevelScript oxyScript;

    // Start is called before the first frame update
    void Start()
    {
        oxyScript = FindObjectOfType<OxygenLevelScript>();
        animator = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
        // stamina bar is set up
        staminaBar = GameManagerScript.instance.inGameScreen.transform.GetChild(4).gameObject.GetComponent<Slider>();
        breathing = thirdPersonPerspective.GetComponent<AudioSource>();
        backgroundMusic = astronaut.GetComponent<AudioSource>();
        // current speed is set
        speed = movementSpeed;
        // sprint is allowed
        sprintEnabled = true;
        // third person camera is set
        thirdPersonPerspective.enabled = true;
        backgroundMusic.volume = GameManagerScript.instance.optMusicVolume;
        // rocket piece pop up is set to false
        GameManagerScript.instance.inGameScreen.transform.GetChild(3).gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // background music vloume is reset if changed in options
        backgroundMusic.volume = GameManagerScript.instance.optMusicVolume;
        // run animation
        if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.W)  || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1) {
                animator.SetBool("isRunning", true);
            }
        }
        else {
            animator.SetBool("isRunning", false);
        }
    }

    void FixedUpdate()
    {
        // if game is unpaused
        if (gameOn){
            // check if astronaut is on ground
            var grounded =  Physics.Raycast(feetPosition.position, -Vector3.up, out RaycastHit hit, 0.75f, mask);
            cc.SimpleMove(transform.forward * speed * Input.GetAxis("Vertical"));
            transform.Rotate(0, Input.GetAxis("Horizontal") * rotationSpeed, 0);
            
            // if sprint is enabled, increase speed when left shift is pressed
            if (Input.GetKey(KeyCode.LeftShift) && sprintEnabled){
                speed = sprintSpeed;
                sprintMeter += 15f * Time.fixedDeltaTime;
            }
            else {
                // speed is reset to walk
                speed = movementSpeed;
                // if the user stops sprinting, meter is decreased
                if (sprintMeter > 0f && sprintMeter < 100f) sprintMeter  -= 1f;
            }

            // jump allowed if astronaut is grounded - to prevent double jumps
            if (grounded){
                // if space is pressed, increase the astronaut's y position
                if (Input.GetButton("Jump")){
                    yPosition += Mathf.Sqrt(jumpForce * -1.5f * gravityValue);
                    
                }
            }

            // make astronaut jump 
            yPosition += gravityValue * Time.deltaTime;
            cc.Move(new Vector3( 0, yPosition, 0));

            // update stamina bar
            staminaBar.value = 1 - (sprintMeter/100);

            // if the user runs out of stamina
            if (sprintMeter >= 100f){
                StartCoroutine(breathingSound());
            }  

            // allows user to pick up piece by pressing enter
            if (selectedPart != null){
                if (Input.GetKey(KeyCode.Return)){
                    GameManagerScript.instance.CollectPart();
                }   
            }
        }
    }

    // coroutine when stamina is depleted
    private IEnumerator breathingSound(){
        breathing.volume = GameManagerScript.instance.optSoundsVolume;
        // sprinting is not allowed while resting
        sprintEnabled = false;
        breathing.loop = true;

        while (sprintMeter > 0f){
            // breathing plays until stamina is replenished
            breathing.Play();
            sprintMeter -= 25f;
            yield return new WaitForSeconds(breathing.clip.length);
        }

        sprintMeter = 0f;
        breathing.loop = false;
        sprintEnabled = true;
        StopCoroutine(breathingSound());
    }

    /**
    * handles trigger events
    */
    void OnTriggerEnter(Collider other) {
        // toggles rocket piece pick up pop up when close to the piece
        if(other.tag == "Rocket") {
            GameManagerScript.instance.inGameScreen.transform.GetChild(3).gameObject.SetActive(true);
            selectedPart = other.gameObject.transform.parent.gameObject;
        }

        if (other.tag == "Coin") {
            //destroy the coin
            Destroy(other.gameObject.transform.parent.gameObject);
            //increase oxygen
            oxyScript.IncreaseOxygen();
        }
        
        if (other.tag == "FixedRocket") {
            //GameManagerScript.EndGame();
            oxyScript.EndGame();
            //destroy rocket
            //Destroy(other.gameObject.transform.parent.gameObject);
        }
    }

    // resets selected part and disables pop up when player exits the collider
    void OnTriggerExit(Collider other) {
        if (other.tag == "Rocket") {
            GameManagerScript.instance.inGameScreen.transform.GetChild(3).gameObject.SetActive(false);
            selectedPart = null;
        }
    }

    public Vector3 GetCurrPosition() {
        return transform.position;
    }
}
