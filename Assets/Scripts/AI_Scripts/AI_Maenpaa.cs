using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Maenpaa : PlayerControllerInterface
{

    int laskuri = 0;
    // TÄMÄ TULEE TEHTÄVÄSSÄ TÄYDENTÄÄ
    // Käytä vain PlayerControllerInterfacessa olevia metodeja TIMissä olevan ohjeistuksen mukaan
    public override void DecideNextMove()
    {
        // Switch case määrittää botin liikeen
        // Jos edessä on tyhjä tila (case 0), liikutaan eteenpäin
        // Jos edessä on seinä (case 1), käännytään oikealle tai vasemmalle
        // Case 1 valitsee sattumanvaraisesti numeron 1:n ja 4:n väliltä. 
        // Jos numero on jaollinen kahdella, käännytään oikealle. Muuten käännytään vasemmalle.
        // Näin noin joka toinen käännös on vasemmalle ja joka toinen oikealle.
        // Jos edessä on vihollinen (case 2), sitä lyödään
        // Default-toimintona on vuoron passaaminen

        switch (GetForwardTileStatus())
        {
            case 0:
                nextMove = MoveForward; break;
            case 1:
                laskuri = laskuri + 1;
                int jok = Random.Range(1, 4);
                if (jok % 2 == 0)
                {
                    nextMove = TurnRight; break;
                }
                else
                {
                    nextMove = TurnLeft; break;
                }
            case 2:
                nextMove = Hit; break;
            default: nextMove = Pass; break;
        }
    }
}
