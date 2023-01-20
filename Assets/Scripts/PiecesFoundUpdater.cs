using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PiecesFoundUpdater : MonoBehaviour
{
    //public GameObject astronautPrefab;
    public static PiecesFoundUpdater instance;
    private static TextMeshProUGUI text;
    public static int totalRocketParts = 9;
    public static int collectedRocketParts = 0;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        collectedRocketParts = GameManagerScript.collectedRocketParts;
        SetTotalRocketParts(GameManagerScript.totalRocketParts);
        UpdatePiecesFoundText();
    }

    // Update is called once per frame
    void Update()
    {

        
    }

    /**
    * update text with new collected number
    */
    static void UpdatePiecesFoundText() {
        text.text = "Pieces found: " + collectedRocketParts.ToString() + "/" + totalRocketParts.ToString();
    }

    /**
    * sets new amount of collected pieces
    */
    public static void SetCollectedRocketParts(int newCollectedParts) {
        collectedRocketParts = newCollectedParts;
        UpdatePiecesFoundText();
    }

    /**
    * sets total collected pieces
    */
    public static void SetTotalRocketParts(int newTotalParts) {
        totalRocketParts = newTotalParts;
    }
}
