using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIScript : PlayerControllerInterface
{
    // T�M� METODI TULEE TEHT�V�SS� T�YDENT��
    // K�yt� vain PlayerControllerInterfacessa olevia metodeita
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
