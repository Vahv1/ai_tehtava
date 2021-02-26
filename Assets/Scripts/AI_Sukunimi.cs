using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Sukunimi : PlayerControllerInterface
{
    // TÄMÄ METODI TULEE TEHTÄVÄSSÄ TÄYDENTÄÄ
    // käytä vain PlayerControllerInterfacessa olevia metodeita
    public override void DecideNextMove()
    {
        // Tyhmä tekoäly, liikkuu eteenpäin jos edessä on tyhjä ruutu, muuten lyö
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
