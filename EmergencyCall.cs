using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EmergencyCall : MonoBehaviour {

    public float nextLineDelay = 2f;
    public Text dialog;
    public int progressionLevel = 0;

    public bool firstConvoAlreadyStarted = false;

    public string[] line;

	// Use this for initialization
	void Start () {
        
        dialog = GetComponentInChildren<Text>();
	}
	
	// Update is called once per frame
	void Update () {

        if (GameManager.Instance.player[2].dead)
        {
            // Player is dead
            transform.GetChild(0).gameObject.SetActive(false);
            progressionLevel = 0;
            dialog.text = "";
            return;
        }

        if (GameManager.Instance.isGameRunning && !firstConvoAlreadyStarted)
        {
            StartCoroutine(ConversationPopUp(line[progressionLevel]));
            firstConvoAlreadyStarted = true;
        }

        int convo = (int)GameManager.Instance.player[2].taskProgress;


        if (convo >= 10 && progressionLevel < 1)
        {
            GoToNextLine();
        }
        else if (convo >= 20 && progressionLevel < 2)
        {
            GoToNextLine();
        }
        else if (convo >= 30 && progressionLevel < 3)
        {
            GoToNextLine();
        }
        else if (convo >= 40 && progressionLevel < 4)
        {
            GoToNextLine();
        }
        else if (convo >= 50 && progressionLevel < 5)
        {
            GoToNextLine();
        }
        else if (convo >= 60 && progressionLevel < 6)
        {
            GoToNextLine();
        }
        else if (convo >= 70 && progressionLevel < 7)
        {
            GoToNextLine();
        }
        else if (convo >= 80 && progressionLevel < 8)
        {
            GoToNextLine();
        }
        else if (convo >= 90 && progressionLevel < 9)
        {
            GoToNextLine();
        }
    }

    public void GoToNextLine()
    {
        progressionLevel++;
        StartCoroutine(ConversationPopUp(line[progressionLevel]));
    }

    public void Reset()
    {
        dialog.text = "";
        GameManager.Instance.player[2].taskProgress = 0f;
        progressionLevel = 0;
    }

    // displays next line of conversation with brief blank-text intermission
    IEnumerator ConversationPopUp(string newText)
    {
        dialog.text = "";

        yield return new WaitForSeconds(nextLineDelay);

        newText = newText.Replace("NEWLINE", "\n");
        dialog.text = newText;
    }
}
