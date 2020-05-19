using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Medkit : AbilityObject {
    public HorrorCharacterController worker;
    public float overallProgress;
    public float phoneProgress;
    public int phoneLevel;
    public int medkitHealAmount = 20;
    public GameObject healSFX;

    // UI Elements
    private int offset = 0; // This is so the bar maxes out at each 50% task milestone
    private int maxValue = 60;
    public ProgressBarPro phoneProgressUI;
    public Text phoneLevelTxt;
    public UIPopUp abilityAlertUI;

    // Use this for initialization
    void Start()
    {
        phoneLevel = 0;
    }

    // Update is called once per frame
    void Update()
    {
        overallProgress = worker.taskProgress;
        phoneProgress = overallProgress - (float)offset;

        phoneProgressUI.SetValue(phoneProgress, maxValue);

        if (overallProgress >= 60 && overallProgress < 100 && phoneLevel == 0)
        {
            phoneLevel = 1;
            
            offset = 60;
            maxValue = 40;
            phoneLevelTxt.text = "CALLING 911";
        }
    }

    public override void ObjectAbility()
    {
        // Play SFX
        StartCoroutine(HealVFX());

        foreach (HorrorCharacterController hcc in GameManager.Instance.player)
        {
            if (!hcc.dead)
                hcc.GetHealthBoost(medkitHealAmount);
        }
    }

    IEnumerator HealVFX()
    {
        healSFX.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        healSFX.SetActive(false);
    }
}
