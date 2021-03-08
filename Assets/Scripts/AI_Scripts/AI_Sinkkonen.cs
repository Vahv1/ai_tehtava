using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Sinkkonen : PlayerControllerInterface
{
    public Vector2[] enemyPositions;
    public Vector2 myPos;
    public Vector2 myRot;
    public Vector2 closestPos;
    public bool wasStuck;
    public bool ignoreDangerRecognition = false;
    public int consecutiveHits = 0;

    public override void DecideNextMove()
    {
        // Otetaan perusinfot talteen
        myPos = GetPosition();
        myRot = GetRotation();
        enemyPositions = GetEnemyPositions();
        closestPos = GetClosestEnemyPosition();
        int forwardTile = GetForwardTileStatus();

        // Aina jos vihollinen edess‰, niin lyˆd‰‰n
        if (GetForwardTileStatus() == 2)
        {
            nextMove = Hit;
            return;
        }

        // Jos oltiin viime vuorolla jumissa ja k‰‰nnyttiin randomsuuntaan, nyt joko liikutaan eteenp‰in
        // tai k‰‰nnyt‰‰n uudestaan randomsuuntaan jos edess‰ on viel‰kin sein‰
        if (wasStuck)
        {
            if (forwardTile == 0)
            {
                nextMove = MoveForward;
                wasStuck = false;
            }
            else
            {
                TurnRandomDirection();
                wasStuck = true;
            }
            return;
        }

        // Katsotaanko p‰‰sisikˆ l‰hemm‰s l‰himm‰ist‰ vihollista jos liikkuisi eteenp‰in
        if (Vector2.Distance(closestPos, myPos + (Vector2)transform.right) < Vector2.Distance(closestPos, myPos))
        {
            // Jos p‰‰st‰isiin l‰hemm‰s ja edess‰ tyhj‰‰, liikutaan eteenp‰in
            if (forwardTile == 0)
            {
                nextMove = MoveForward;
                wasStuck = false;
            }
            // Jos p‰‰st‰isi l‰hemm‰s, mutta ei ole tyhj‰‰ edess‰, k‰‰nnyt‰‰n randomilla
            else if (forwardTile == 1)
            {
                TurnRandomDirection();
                wasStuck = true; // Kerrotaan ett‰ oltiin jumissa, vaikka oli oikea suunta
            }
        }
        // Jos ei p‰‰st‰isi l‰hemm‰s liikkumalla eteen, k‰‰nnyt‰‰n kohti vihollista
        else
        {
            TurnToEnemy();
        }

        // Tsekataan ettei olla liikkumassa vihollisen eteen
        // siis jos vihollinen on molemmilla akseleilla yhden p‰‰ss‰ ja k‰‰ntynyt minua kohti niin lyˆd‰‰n
        // Ei tarkisteta jos ignoreDangerRecognition on p‰‰ll‰
        // Tƒƒ TOIMII TOSI HUONOSTI, TODO
        if (Math.Abs(closestPos.x - myPos.x) == 1 && Math.Abs(closestPos.y - myPos.y) == 1 && !ignoreDangerRecognition)
        {
            // jos olen vihun oikealla puolen ja se on k‰‰ntynyt oikealle
            if (closestPos.x - myPos.x < 0 && GetEnemyRotation(closestPos).x == 1)
            {
                TurnToEnemy();
                // Jos rotaation on jo oikein ja TurnToEnemy ei antanut k‰‰nnˆst‰ ja aiemmasta on p‰‰ll‰ moveForward, vaihdetaan se Hittiin
                if (nextMove == MoveForward) nextMove = Hit;
                consecutiveHits++;
            }
            // jos olen vihun vasemmalla puolen ja se on k‰‰ntynyt vasemmalle
            else if (closestPos.x - myPos.x > 0 && GetEnemyRotation(closestPos).x == -1)
            {
                TurnToEnemy();
                if (nextMove == MoveForward) nextMove = Hit;
                consecutiveHits++;
            }

            // jos olen vihun yl‰puolella ja se on k‰‰ntynyt ylˆs
            if (closestPos.y - myPos.y < 0 && GetEnemyRotation(closestPos).y == 1)
            {
                TurnToEnemy();
                if (nextMove == MoveForward) nextMove = Hit;
                consecutiveHits++;
            }
            // jos olen vihun alapuolella ja se on k‰‰ntynyt alas
            if (closestPos.x - myPos.y > 0 && GetEnemyRotation(closestPos).y == -1)
            {
                TurnToEnemy();
                if (nextMove == MoveForward) nextMove = Hit;
                consecutiveHits++;
            }
        }

        if (consecutiveHits > 3)
        {
            ignoreDangerRecognition = true;
        }

        if (nextMove == MoveForward || nextMove == TurnLeft || nextMove == TurnRight)
        { 
            consecutiveHits = 0;
            // Laitetaan dangerRecognition takaisin p‰‰lle kun ollaan liikuttu oletetusta jumista eteenp‰in
            if (nextMove == MoveForward) { ignoreDangerRecognition = false; }
        }
    }

    public void TurnRandomDirection()
    {
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            nextMove = TurnLeft;
        }
        else { nextMove = TurnRight; }
    }

    public void TurnToEnemy()
    {
        if (closestPos.x != myPos.x)
        {
            TurnLeftOrRight();
        }
        else
        {
            TurnUpOrDown();
        }
    }

    public void TurnLeftOrRight()
    {
        if (closestPos.x < myPos.x && myRot != new Vector2(-1, 0))
        {
            TurnToFace("left");
        }
        else
        {
            TurnToFace("right");
        }
    }

    public void TurnUpOrDown()
    {
        if (closestPos.y < myPos.y && myRot != new Vector2(0, -1))
        {
            TurnToFace("down");
        }
        else
        {
            TurnToFace("up");
        }
    }

    public void TurnToFace(string dir)
    {
        switch (dir)
        {
            case "left":
                if (myRot == new Vector2(1, 0) || myRot == new Vector2(0, 1))
                {
                    nextMove = TurnLeft;
                }
                else if (myRot == new Vector2(0, -1))
                {
                    nextMove = TurnRight;
                }
                break;

            case "right":
                if (myRot == new Vector2(0, 1) || myRot == new Vector2(-1, 0))
                {
                    nextMove = TurnRight;
                }
                else if (myRot == new Vector2(0, -1))
                {
                    nextMove = TurnLeft;
                }
                break;

            case "up":
                if (myRot == new Vector2(0, -1) || myRot == new Vector2(1, 0))
                {
                    nextMove = TurnLeft;
                }
                else if (myRot == new Vector2(-1, 0))
                {
                    nextMove = TurnRight;
                }
                break;

            default: //down
                if (myRot == new Vector2(0, 1) || myRot == new Vector2(1, 0))
                {
                    nextMove = TurnRight;
                }
                else if (myRot == new Vector2(-1, 0))
                {
                    nextMove = TurnLeft;
                }
                break;
        }
    }

    // Antaa l‰himm‰n vihollisen sijainnin
    public Vector2 GetClosestEnemyPosition()
    {
        Vector2 closest = new Vector2(-50, -50);
        foreach (Vector2 pos in enemyPositions)
        {
            if (Vector2.Distance(pos, myPos) < Vector2.Distance(closest, myPos))
            {
                closest = pos;
            }
        }
        return closest;
    }
}
