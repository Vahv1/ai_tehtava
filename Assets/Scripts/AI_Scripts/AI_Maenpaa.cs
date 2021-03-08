using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Maenpaa : PlayerControllerInterface
{

    int laskuri = 0;
    // T�M� TULEE TEHT�V�SS� T�YDENT��
    // K�yt� vain PlayerControllerInterfacessa olevia metodeja TIMiss� olevan ohjeistuksen mukaan
    public override void DecideNextMove()
    {
        // Switch case m��ritt�� botin liikeen
        // Jos edess� on tyhj� tila (case 0), liikutaan eteenp�in
        // Jos edess� on sein� (case 1), k��nnyt��n oikealle tai vasemmalle
        // Case 1 valitsee sattumanvaraisesti numeron 1:n ja 4:n v�lilt�. 
        // Jos numero on jaollinen kahdella, k��nnyt��n oikealle. Muuten k��nnyt��n vasemmalle.
        // N�in noin joka toinen k��nn�s on vasemmalle ja joka toinen oikealle.
        // Jos edess� on vihollinen (case 2), sit� ly�d��n
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
