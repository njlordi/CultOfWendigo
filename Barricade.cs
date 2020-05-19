using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Barricade : MonoBehaviour {

    public GameObject[] board;
    public Transform boostLocation;
    public float boostAmount = 2f;
    public float timeInSecsBeforeReset = 5f;
    public int barricadeLevel = 0;
    public HorrorCharacterController worker;
    public float buildProgress;
    public float barricadeXP;

    private Vector3[] originalPos;
    private Quaternion[] originalRot; // original z rotations of all boards
    private Rigidbody[] rb;

    // Audio
    public GameObject woodSmashSFX;

    // UI Elements
    private int offset = 0; // This is so the bar maxes out at each 25% task milestone
    public ProgressBarPro barricadeXPUI;
    public Text levelNumber;
    public Text barricadeStatus;

	// Use this for initialization
	void Start () {
        barricadeStatus.text = "BUILD BARRICADE";
        levelNumber.text = "0";

        originalPos = new Vector3[4];
        originalRot = new Quaternion[4];
        rb = new Rigidbody[4];

		// record original positions, original Z rotations, and get rigidbodies
        for (int i = 0; i < board.Length; i++)
        {
            originalPos[i] = board[i].transform.localPosition;
            originalRot[i] = board[i].transform.rotation;
            rb[i] = board[i].GetComponent<Rigidbody>();
        }
	}
	
	// Update is called once per frame
	void Update () {

        buildProgress = worker.taskProgress;
        barricadeXP = buildProgress - (float)offset;

        barricadeXPUI.SetValue(barricadeXP, 25f);

        if (buildProgress >= 25 && buildProgress < 50 && barricadeLevel == 0)
        {
            barricadeLevel = 1;
            board[0].SetActive(true);
            offset = 25;
            barricadeStatus.text = "UPGRADING BARRICADE";
            levelNumber.text = "1";
        }
        else if (buildProgress >= 50 && buildProgress < 75 && barricadeLevel == 1)
        {
            barricadeLevel = 2;
            board[1].SetActive(true);
            offset = 50;
            barricadeStatus.text = "UPGRADING BARRICADE";
            levelNumber.text = "2";
        }
        else if (buildProgress >= 75 && buildProgress < 100 && barricadeLevel == 2)
        {
            barricadeLevel = 3;
            board[2].SetActive(true);
            offset = 75;
            barricadeStatus.text = "UPGRADING BARRICADE";
            levelNumber.text = "3";
        }
        else if (buildProgress >= 100 && barricadeLevel == 3)
        {
            barricadeLevel = 4;
            board[3].SetActive(true);
            barricadeStatus.text = "";
            levelNumber.text = "MAX";
            worker.playerStatus = HorrorCharacterController.Status.idle;
        }
    }

    void ResetAllBoardPosAndDisable()
    {

        foreach (Rigidbody r in rb)
        {
            r.isKinematic = true;
        }

        foreach (GameObject go in board)
        {
            go.SetActive(false);
        }

        // Restore original position and original Z rotation of each board
        for (int i = 0; i < board.Length; i++)
        {
            board[i].transform.localPosition = originalPos[i];
            board[i].transform.rotation = originalRot[i];
        }
    }

    public void BreakBarricade()
    {
        StartCoroutine(BreakBarricadeCR());
    }

    IEnumerator BreakBarricadeCR()
    {
        if (barricadeLevel >= 1)
        {
            woodSmashSFX.SetActive(true);
        }

        barricadeXPUI.SetValue(0, 25);
        barricadeLevel = 0;
        worker.taskProgress = 0;
        barricadeStatus.text = "BUILD BARRICADE";
        levelNumber.text = "0";
        offset = 0;

        foreach (Rigidbody r in rb)
        {
            r.isKinematic = false;
            r.AddForce(-boostLocation.transform.position * boostAmount, ForceMode.Impulse);
            yield return null;
        }

        yield return new WaitForSeconds(timeInSecsBeforeReset);

        woodSmashSFX.SetActive(false);
        ResetAllBoardPosAndDisable();

        yield return new WaitForSeconds(10); // DELAY! Incase bot dies by Wendigo before auto-!task
        // If the worker is not dead, and is a bot and not working, then have him start rebuilding
        if (!worker.dead && !worker.humanPlayer 
            && worker.playerStatus != HorrorCharacterController.Status.workingOnTask)
        {
            worker.playerStatus = HorrorCharacterController.Status.workingOnTask;
        }
    }
}
