using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarRepair : AbilityObject {

    public GameObject[] car;
    public GameObject whiteSmoke;
    public GameObject blackSmoke;
    public float overallProgress;
    public float carProgress;
    public int carLevel = 0;

    public HorrorCharacterController mechanic;

    public float lightsDuration;
    public GameObject alarmSFX;
    public GameObject leftDimmers;
    public GameObject rightDimmers;
    public GameObject leftPointlight;
    public GameObject rightPointlight;

    // UI Elements
    private int offset = 0; // This is so the bar maxes out at each 50% task milestone
    private int maxValue; // This is for the UI ratio to work
    public ProgressBarPro carProgressUI;
    public Text carLevelTxt;
    public UIPopUp abilityAlertUI;

    // Use this for initialization
    void Start()
    {
        maxValue = 15;

        foreach (GameObject c in car)
        {
            c.gameObject.SetActive(false);
        }

        car[0].SetActive(true);
        blackSmoke.SetActive(true);
    }
	
	// Update is called once per frame
	void Update () {

        overallProgress = mechanic.taskProgress;
        carProgress = overallProgress - (float)offset;

        carProgressUI.SetValue(carProgress, maxValue);

        if (overallProgress >= 15 && overallProgress < 50 && carLevel == 0)
        {
            carLevel = 1;
            offset = 15;
            maxValue = 85;

            blackSmoke.SetActive(true);
            whiteSmoke.SetActive(false);

            car[0].SetActive(false);
            car[1].SetActive(true);
            car[2].SetActive(false);
            car[3].SetActive(false);
            car[4].SetActive(false);
        }
        else if (overallProgress >= 50 && overallProgress < 75 && carLevel == 1)
        {
            carLevel = 2;

            blackSmoke.SetActive(true);
            whiteSmoke.SetActive(false);

            car[0].SetActive(false);
            car[1].SetActive(false);
            car[2].SetActive(true);
            car[3].SetActive(false);
            car[4].SetActive(false);
        }
        else if (overallProgress >= 75 && overallProgress < 100 && carLevel == 2)
        {
            carLevel = 3;
            blackSmoke.SetActive(false);
            whiteSmoke.SetActive(true);

            car[0].SetActive(false);
            car[1].SetActive(false);
            car[2].SetActive(false);
            car[3].SetActive(true);
            car[4].SetActive(false);
        }
        else if (overallProgress >= 100 && carLevel == 3)
        {
            carLevel = 4;

            blackSmoke.SetActive(false);
            whiteSmoke.SetActive(false);

            car[0].SetActive(false);
            car[1].SetActive(false);
            car[2].SetActive(false);
            car[3].SetActive(false);
            car[4].SetActive(true);
        }

    }

    public override void ObjectAbility()
    {
        base.ObjectAbility();

        StartCoroutine(CarAlarm());
    }

    IEnumerator CarAlarm()
    {
        float duration = lightsDuration;

        // Play SFX
        alarmSFX.SetActive(true);

        while (duration > 0)
        {
            leftDimmers.SetActive(true);
            rightDimmers.SetActive(false);
            leftPointlight.SetActive(false);
            rightPointlight.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            leftDimmers.SetActive(false);
            rightDimmers.SetActive(true);
            leftPointlight.SetActive(true);
            rightPointlight.SetActive(false);
            yield return new WaitForSeconds(0.5f);
            duration -= 1;
            yield return null;
        }

        /****** Turn everything OFF *******************/
        leftDimmers.SetActive(true);
        rightDimmers.SetActive(true);
        leftPointlight.SetActive(false);
        rightPointlight.SetActive(false);
        alarmSFX.SetActive(false);
    }
}
