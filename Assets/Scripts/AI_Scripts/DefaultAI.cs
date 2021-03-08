using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultAI : PlayerControllerInterface
{
    public override void DecideNextMove()
    {
        if (GetForwardTileStatus() == 0)
        {
            nextMove = MoveForward;
        }
        else
        {
            nextMove = Pass;
        }
    }
}
