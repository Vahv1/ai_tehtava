using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Sukunimi : PlayerControllerInterface
{
    // TÄMÄ TULEE TEHTÄVÄSSÄ TÄYDENTÄÄ
    // Käytä vain PlayerControllerInterfacessa olevia metodeja TIMissä olevan ohjeistuksen mukaan
    public override void DecideNextMove()
    {
        // Tyhmä tekoäly, liikkuu eteenpäin jos edessä on tyhjä ruutu
        if (GetForwardTileStatus() == 0)
        {
            nextMove = MoveForward;
        }
        // Muuten ei tee mitään
        else
        {
            nextMove = Pass;
        }
    }
}
