using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* Displays the name of the chracter
/* References the players neck for display location */
public class NameLabel : MonoBehaviour {

    public GameObject playerRef;
    public float distanceAboveHead; // 0.33 is good
    public float movementSpeed = 50;

    private Text statusText;
    private bool statusInitialized = false;

	// Use this for initialization
	void Start () {
        statusText = GetComponentInChildren<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        if (GameManager.Instance.isGameRunning)
        {
            Vector3 location = new Vector3(playerRef.transform.position.x,
                playerRef.transform.position.y + distanceAboveHead, playerRef.transform.position.z);

            transform.position = Vector3.MoveTowards(transform.position,
            Camera.main.WorldToScreenPoint(location), movementSpeed * Time.deltaTime);
        }

        if (GameManager.Instance.isGameRunning && !statusInitialized)
        {
            statusInitialized = true;
            statusText.text = "Type: !task to start working";
        }
    }
}
