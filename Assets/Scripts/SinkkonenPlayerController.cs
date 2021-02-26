using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinkkonenPlayerController : PlayerControllerInterface
{
    // T�M� METODI TULEE TEHT�V�SS� T�YDENT��
    // k�yt� vain PlayerControllerInterfacessa olevia metodeita
    public override void DecideNextMove()
    {
        if (GetForwardTileStatus() == 0)
        {
            nextMove = MoveForward;
        }
        else if (GetForwardTileStatus() == 1)
        {
            nextMove = TurnLeft;
        }
        else if (GetForwardTileStatus() == 2)
        {
            nextMove = Hit;
        }
    }
}
