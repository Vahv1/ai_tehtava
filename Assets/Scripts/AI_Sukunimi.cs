using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Sukunimi : PlayerControllerInterface
{
    // T�M� METODI TULEE TEHT�V�SS� T�YDENT��
    // k�yt� vain PlayerControllerInterfacessa olevia metodeita
    public override void DecideNextMove()
    {
        // Tyhm� teko�ly, liikkuu eteenp�in jos edess� on tyhj� ruutu, muuten ly�
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
