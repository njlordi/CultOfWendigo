using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demonologist : HorrorCharacterController {

    public override void AttemptToPerformAbility()
    {
        if (this.dead) { return; }

        if (!abilityIcon.ready)
        {
            // Alert player that he/she has no abilities to use
            ShowAlertMessage("ABILITY NOT READY!", Color.red, 14);
            return;
        }

        if (GameManager.Instance.totalDeadPlayers < 1)
        {
            // Alert player that he/she has no abilities to use
            ShowAlertMessage("No dead players to resurrect!", Color.red, 14);
            return;
        }

        abilityIcon.DisableIcon();
        ShowAlertMessage("CASTING RESURRECT SPELL!", Color.green, 14);
        abilityObject.ObjectAbility();
    }
}
