using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Ritoranta : PlayerControllerInterface
{
    float rng = 0.0f; //muuttuja jolla k��nnell��n bottia
    //Vector2[] enemies;

    public override void DecideNextMove()
    {
        //enemies = GetEnemyPositions();
        if (GetForwardTileStatus() == 2) //jos vihu edess� niin m�tk�ist��n sit�
            nextMove = Hit;
        else if (GetForwardTileStatus() == 1) //jos taas sein� edess� k��nnyt��n random suuntaan
        {
            rng = Random.Range(-1.0f, 1.0f); //k�ytet��n Unityn Random-luokkaa
            if (rng < 0)
                nextMove = TurnRight;
            else
                nextMove = TurnLeft;
        }
        else //jos ei vihu eik� sein� edess�, liikutaan eteenp�in
            nextMove = MoveForward;
    }


    //-------------
    // Ei k�yt�ss�:

    private Vector2 ClosestEnemy(Vector2[] positions)
    {
        if (positions.Length == 0)
            return new Vector2();
        Vector2 EPosition = positions[0];
        Vector2 PPosition = GetPosition();
        float distance = GetDistance(PPosition, EPosition);
        float helper = 0.0f;
        foreach (Vector2 v in positions)
        {
            helper = GetDistance(PPosition, v);
            if (helper < distance)
            {
                EPosition = v;
                distance = helper;
            }
        }
        return EPosition;
    }

    private float GetDistance(Vector2 player, Vector2 enemy)
    {
        float dist = (enemy.x - player.x) + (enemy.y - player.y);
        //Debug.Log(dist);
        return Mathf.Abs(dist);
    }
}