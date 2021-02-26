using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIScript : PlayerControllerInterface
{
    // TÄMÄ METODI TULEE TEHTÄVÄSSÄ TÄYDENTÄÄ
    // Käytä vain PlayerControllerInterfacessa olevia metodeita
    public override void DecideNextMove()
    {
        if (GetForwardTileStatus() == 0)
        {
            nextMove = MoveForward;
        }
        else
        {
            nextMove = Hit;
        }
    }
}
