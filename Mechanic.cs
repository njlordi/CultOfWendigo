using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;

public class Mechanic : HorrorCharacterController {

    public override void AttemptToPerformAbility()
    {
        if (this.dead) { return; }

        if (!abilityIcon.ready)
        {
            // Alert player that he/she has no abilities to use
            ShowAlertMessage("ABILITY NOT READY!", Color.red, 14);
            return;
        }

        if (GameManager.Instance.enemiesProvokedByMechanic)
        {
            // Alert player that the ability is already invoked
            ShowAlertMessage("Already provoking next attack!", Color.red, 14);
            return;
        }

        abilityIcon.DisableIcon();
        GameManager.Instance.enemiesProvokedByMechanic = true;
        ShowAlertMessage("PROVOKING ENEMY!", Color.green, 14);
        abilityObject.ObjectAbility();
    }
}
