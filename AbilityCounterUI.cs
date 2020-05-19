using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityCounterUI : MonoBehaviour {

    public HorrorCharacterController worker;
    public ProgressBarPro abilityUI;
    public TextMeshProUGUI nameToDisplay;

	// Use this for initialization
	void Start () {
        nameToDisplay = GetComponent<TextMeshProUGUI>();
	}
	
	// Update is called once per frame
	void Update () {
        if (worker.humanPlayer)
        {
            nameToDisplay.text = worker.playerName;
        }

        abilityUI.SetValue(worker.abilityUses, 3);
	}
}
