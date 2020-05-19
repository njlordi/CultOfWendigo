using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spellbook : AbilityObject {
    public HorrorCharacterController worker;
    public float overallProgress;
    public float readingProgress;
    public int spellLevel;

    // UI Elements
    private int offset = 0; // This is so the bar maxes out at each 50% task milestone
    public ProgressBarPro spellProgressUI;
    public Text spellLevelTxt;
    public UIPopUp abilityAlertUI;

    // Use this for initialization
    void Start () {
        spellLevel = 0;
    }
	
	// Update is called once per frame
	void Update () {
        overallProgress = worker.taskProgress;
        readingProgress = overallProgress - (float)offset;

        spellProgressUI.SetValue(readingProgress, 50f);
    
        if (overallProgress >= 50 && overallProgress < 100 && spellLevel == 0)
        {

            spellLevel = 1;

            offset = 50;
            spellLevelTxt.text = "CASTING BANISH SPELL";

        }
    }

    public override void ObjectAbility()
    {
        foreach (HorrorCharacterController hcc in GameManager.Instance.player)
        {
            if (hcc.dead)
            {
                hcc.playerStatus = HorrorCharacterController.Status.returningFromDead;
                return; // found a dead player, stop searching 
            }
        }
    }
}
