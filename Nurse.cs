using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nurse : HorrorCharacterController {

    public override void AttemptToPerformAbility()
    {
        if (this.dead) { return; }

        if (!abilityIcon.ready)
        {
            // Alert player that he/she has no abilities to use
            ShowAlertMessage("ABILITY NOT READY!", Color.red, 14);
            return;
        }

        abilityIcon.DisableIcon();
        ShowAlertMessage("USING MEDICAL KIT!", Color.green, 14);
        abilityObject.ObjectAbility();
    }
}
