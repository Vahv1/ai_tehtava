using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Niskakoski : PlayerControllerInterface
{
    // Apumuuttujat
    private int nextTile;
    private int turns;              // Laskee k‰‰nnˆksi‰
    private int forwardMoves;       // Laskee eteenp‰inliikkumisia
    private bool turnLeft;          // Apumuuttuja satunnaisten k‰‰nnˆsten suhteen
    private bool firstIteration;    // Apumuuttuja vihollisten l‰pik‰ynniss‰

    // Omat statusmuuttujat
    private Vector2 myPos;
    private Vector2 myRot;
    private float myRotX;
    private float myRotY;

    // Vihollisten statusmuuttujat
    private Vector2[] enemyPos;
    private float targetEnemyDistance;
    private Vector2 targetEnemyPos;

    // Apumuuttuja osoittamaan l‰himm‰n vihollisen suunta omaan positioon n‰hden
    private int direction;

    // K‰yt‰ vain PlayerControllerInterfacessa olevia metodeja TIMiss‰ olevan ohjeistuksen mukaan
    public override void DecideNextMove()
    {
        // Selvitet‰‰n oma status
        myPos = GetPosition();

        direction = 0;

        // P‰ivitet‰‰n vihollisten sijainnit
        enemyPos = GetEnemyPositions();

        // Alustetaan lista-apumuuttuja
        firstIteration = true;

        // Haetaan l‰himm‰n vihollisen sijainti
        foreach (Vector2 pos in enemyPos)
        {
            // Listan ensimm‰inen alkio targetoidaan alustavasti
            if (firstIteration)
            {
                targetEnemyPos = pos;
                targetEnemyDistance = Vector2.Distance(myPos, pos);
                firstIteration = false;
            }
            // T‰m‰n j‰lkeen verrataan seuraavia alkioita. Targetoidaan aina l‰hin vihollinen.
            else if (targetEnemyDistance > Vector2.Distance(myPos, pos))
            {
                targetEnemyPos = pos;
                targetEnemyDistance = Vector2.Distance(myPos, pos);
            }
        }

        // Selvitet‰‰n mihin suuntaan minun tulee liikkua saavuttaakseen kohteen
        // lopputulosta k‰ytet‰‰n switch-casessa.
        // Vihollinen on...
        //
        // Alavasemmalla,       direction == -3
        // Yl‰vasemmalla,       direction == -1
        // Suoraan vasemmalla,  direction == -6
        //
        // Alaoikealla,         direction == 1
        // Yl‰oikealla,         direction == 3
        // Suoraan oikealla,    direction == -2
        // 
        // Suoraan alapuolella, direction == 4
        // Suoraan yl‰puolella, direction == 6
        //
        // Jos kohde on x-akselilla vasemmalla
        if (targetEnemyPos.x < myPos.x) direction -= 2;
        // Jos kohde on x-akselilla oikealla
        else if (targetEnemyPos.x > myPos.x) direction += 2;
        // Jos kohde on samalla x-akselilla
        else if (targetEnemyPos.x == myPos.x) direction += 5;

        // Jos kohde on y-akselilla alapuolella
        if (targetEnemyPos.y < myPos.y) direction -= 1;
        // Jos kohde on y-akselilla yl‰puolella
        else if (targetEnemyPos.y > myPos.y) direction += 1;
        // Jos kohde on samalla y-akselilla
        else if (targetEnemyPos.y == myPos.y) direction -= 4;

        // Haetaan oma rotaationi
        myRot = GetRotation();
        myRotX = Mathf.Round(myRot.x);
        myRotY = Mathf.Round(myRot.y);

        // Selvitet‰‰n edess‰ olevan ruudun tila
        // 0 = tyhj‰
        // 1 = sein‰
        // 2 = pelaaja
        nextTile = GetForwardTileStatus();

        switch (nextTile)
        {
            // Jos edess‰ on tyhj‰ ruutu
            case 0:
                // Jos vihollinen on ylh‰‰ll‰, alhaalla, oikella tai vasemmalla
                if (targetEnemyDistance <= 1 && (direction == -6 || direction == -2 || direction == 4 || direction == 6))
                {
                    // Toimitaan oman orientaation mukaan
                    switch (direction)
                    {
                        // Vihollinen vasemmalla
                        // Oletus on ett‰ me emme katso t‰llˆin vasemmalle koska edess‰ oleva ruutu on tyhj‰.
                        case -6:
                            // Jos katse on oikealle
                            if (myRotX == 1.0) nextMove = TurnRight;
                            // Jos katse on alas
                            else if (myRotY == -1.0) nextMove = TurnRight;
                            // Jos katse on ylˆs
                            else if (myRotY == 1.0) nextMove = TurnLeft;
                            break;

                        // Vihollinen oikealla
                        // Oletus on ett‰ me emme katso t‰llˆin oikealle...
                        case -2:
                            // Jos katse on vasemmalle
                            if (myRotX == -1.0) nextMove = TurnRight;
                            // Jos katse on alas
                            else if (myRotY == -1.0) nextMove = TurnLeft;
                            // Jos katse on ylˆs
                            else if (myRotY == 1.0) nextMove = TurnRight;
                            break;

                        // Vihollinen ylh‰‰ll‰
                        // Oletus on ett‰ me emme katso t‰llˆin ylˆs...
                        case 6:
                            // Jos katse on vasemmalle
                            if (myRotX == -1.0) nextMove = TurnRight;
                            // Jos katse on oikealle
                            else if (myRotX == 1.0) nextMove = TurnLeft;
                            // Jos katse on alas
                            else if (myRotY == -1.0) nextMove = TurnLeft;
                            break;

                        // Vihollinen alhaalla
                        // Oletus on ett‰ me emme katso t‰llˆin alas...
                        case 4:
                            // Jos katse on vasemmalle
                            if (myRotX == -1.0) nextMove = TurnLeft;
                            // Jos katse on oikealle
                            else if (myRotX == 1.0) nextMove = TurnRight;
                            // Jos katse on ylˆs
                            else if (myRotY == 1.0) nextMove = TurnLeft;
                            break;
                    }
                }
                // Mik‰li vihollisia ei ole l‰hiet‰isyydell‰
                else
                {
                    // Liikutaan 15x eteenp‰in
                    if (forwardMoves < 15)
                    {
                        nextMove = MoveForward;
                        forwardMoves++;
                    }
                    // Aina kun 15x on liikuttu eteenp‰in, niin tehd‰‰n k‰‰nnˆs vasempaan
                    else if (turnLeft)
                    {
                        nextMove = TurnLeft;
                        turnLeft = false;
                        forwardMoves = 0;
                    }
                    // Ja seuraavan 15x eteenp‰inliikkeen j‰lkeen oikeaan
                    else if (!turnLeft)
                    {
                        nextMove = TurnRight;
                        turnLeft = true;
                        forwardMoves = 0;
                    }
                    // N‰in robotin ei pit‰isi j‰‰d‰ jumiin
                }
                break;

            // Jos edess‰ on sein‰
            case 1:
                // Robotti laskee k‰‰nnˆksi‰, jotta ei j‰‰d‰ jumiin
                if (turns < 3)
                {
                    nextMove = TurnLeft;
                    turns++;
                }
                // Kun on k‰‰nnytty 3 kertaa samaan suuntaan, vaihdetaan k‰‰ntymissuuntaa
                else
                {
                    nextMove = TurnRight;
                    turns++;
                    if (turns == 5) turns = 0;
                }
                break;

            case 2: // Jos edess‰ on vihollinen niin hyˆk‰t‰‰n aina
                nextMove = Hit;
                break;
        }

        // Logit auttamaan debuggauksessa
        // Debug.Log("My position: " + myPos);
        // Debug.Log("Target enemy position: " + targetEnemyPos);
        // Debug.Log("Target distance: " + targetEnemyDistance);
        // Debug.Log("Direction to move: " + direction);
    }
}