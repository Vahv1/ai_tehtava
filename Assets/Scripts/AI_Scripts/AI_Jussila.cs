using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Jussila : PlayerControllerInterface
{
    private NextMove lastmove;
    private Vector2 playerPos;
    private Vector2[] list;
    private Vector2 closest;
    private float closestdis = int.MaxValue;
    private Vector2 secondPreviousStartPos;
    private Vector2 fourthStartPos;
    private Vector2 thirdLastPos;
    private Vector2 previoustartpos;
    private Vector2 startpos;

    // T�M� TULEE TEHT�V�SS� T�YDENT��
    // K�yt� vain PlayerControllerInterfacessa olevia metodeja TIMiss� olevan ohjeistuksen mukaan
    public override void DecideNextMove()
    {
        lastmove = nextMove;
        playerPos = GetPosition();
        list = GetEnemyPositions();

        // Etsit��n l�hin vihollinen.
        foreach (Vector2 vector in list)
        {
            if (Vector2.Distance(playerPos, vector) < closestdis)
            {
                closestdis = Vector2.Distance(playerPos, vector);
                closest = vector;
            }
        }
        // Jos edess� on vihu l�yd��n defaulttina.
        if (GetForwardTileStatus() == 2)
        {
            // Jos l�himm�ll� (vieress�) vihollisella on enemm�n HP kuin itsell�, juokse ja toivo ett� sen AI on huompi kuin sinulla.
            if (GetEnemyHP(closest) > GetHP())
            {
                nextMove = TurnRight;
            }
            else nextMove = Hit;
        }
        // Jos on tyhj�� edess� menn��n siihen ruutuun. Jos edelline siirto oli k��tyminen pidet��n tallessa, ett� voidaan tunnistaa looppi.
        else if (GetForwardTileStatus() == 0)
        {
            if (lastmove == TurnRight || lastmove == TurnLeft)
            {
                fourthStartPos = thirdLastPos;
                thirdLastPos = secondPreviousStartPos;
                secondPreviousStartPos = previoustartpos;
                previoustartpos = startpos;
                startpos = playerPos;
            }
            nextMove = MoveForward;
        }
        // Jos t�rm�t��n sein��n k��nnyt��n oikealle, paitsi jos havaitaan looppi niin kokeillaan vasenta.
        // Ei niin sanotusti rei�t�n toteutus, ett� jumiin t�ll�kin voi j��d�, mutta kuitenkin.
        else if (GetForwardTileStatus() == 1)
        {
            // Voisi tehd� taulukkona jonka yli iteroi, mutta en viitsi.
            if (previoustartpos == playerPos || secondPreviousStartPos == playerPos || thirdLastPos == playerPos || fourthStartPos == playerPos)
            {
                nextMove = TurnLeft;
            }
            else nextMove = TurnRight;
        }

        // Jos ei parempaakaan keksit� niin skipataan vuoro. k�yt�nn�ss� mahdontonta.
        else
        {
            nextMove = Pass;
        }
    }
}