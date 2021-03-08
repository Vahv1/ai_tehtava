using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Muraja : PlayerControllerInterface
{
    // T�M� TULEE TEHT�V�SS� T�YDENT��  
    int kaannoksiaOikeaan = 0;
    int kaannoksiaVasempaan = 0;
    int askel = 0;


    // K�yt� vain PlayerControllerInterfacessa olevia metodeja TIMiss� olevan ohjeistuksen mukaan
    public override void DecideNextMove()
    {
        //pelaajan sijainnin koordinaatit, ideana on j�tt�� majakka, ja jos koordinaatit ovat samat kuin aiemmassa, tehd��n t�ysk��nn�s ymp�ri.
        Vector2 sijainti = GetPosition();
        double sijaintiX = sijainti.x;
        double sijaintiY = sijainti.y;
        double majakkaX;
        double majakkaY;
        //satunnaisgeneraattori


        // Tyhm� teko�ly, liikkuu eteenp�in jos edess� on tyhj� ruutu
        //switch casella katsotaan onko edess� mit�
        switch (GetForwardTileStatus())
        {
            case 0: nextMove = MoveForward; break; //jos edess� on tyhj�� liiku
            case 1:
                if (kaannoksiaOikeaan < 5)
                //k��nnyt��n oikealle, laitetaan laskuri monesko k��nn�s on menossa ja asetetaan sijainti, jos k��nn�ksen j�lkeen on sein�, jatketaan k��ntymist�                        
                {
                    switch (GetForwardTileStatus())
                    {
                        case 0:
                            //joka kymmenes askel vasempaan p�in
                            if (askel == 10)
                            {
                                nextMove = TurnLeft;
                                askel = 0;
                            }
                            else
                                nextMove = MoveForward;
                            askel++;
                            break;
                        case 1:
                            nextMove = TurnRight;
                            if (GetForwardTileStatus() == 1)
                            {
                                //jos k��n�ksi� oikeaan on kolmella jaollinen, k��nnyt��n vasemmalle
                                if (kaannoksiaOikeaan / 3 == 1)
                                {
                                    nextMove = TurnLeft;
                                }
                                else
                                {
                                    nextMove = TurnRight;
                                    if (GetForwardTileStatus() == 1)
                                    {
                                        nextMove = TurnRight;
                                        //jos toinenkin k��nn�s on sein�, tarkistetaan onko vastap�inen sein� siit� sein�, vai palataanko takaisin
                                        if (GetForwardTileStatus() == 1)
                                        {
                                            nextMove = TurnRight;
                                            switch (GetForwardTileStatus())
                                            {
                                                case 0: nextMove = MoveForward; break;
                                                case 1: nextMove = TurnRight; break;
                                                case 2: Hit(); break;
                                                default: Pass(); break;
                                            }
                                        }

                                    }
                                }
                            }
                            kaannoksiaOikeaan++;
                            break;
                        case 2: Hit(); break;
                        default: nextMove = Pass; break;
                    }

                }
                else if (kaannoksiaOikeaan >= 5)
                {
                    sijainti = GetPosition();
                    majakkaX = sijaintiX;
                    majakkaY = sijaintiY;
                    nextMove = TurnLeft;

                    //switch casen sis�ll� olevan toiminnon voisi ratkaista toisellakin tapaa kuin if-elsell�
                    switch (GetForwardTileStatus())
                    {
                        case 0:
                            //joka kymmenes askel oikeaan p�in
                            if (askel == 3)
                            {
                                nextMove = TurnRight;
                                askel = 0;
                            }
                            else
                                nextMove = MoveForward;
                            askel++;
                            break;
                        case 1:
                            nextMove = TurnLeft;
                            //k��nnyt��n oikealle, laitetaan laskuri monesko k��nn�s on menossa ja asetetaan sijainti                        
                            {
                                nextMove = TurnLeft;
                                if (GetForwardTileStatus() == 1)
                                {
                                    //jos k��nn�ksi� vasempaan on kolmella jaollinen, k��nnyt��n oikealle.
                                    if (kaannoksiaVasempaan / 3 == 1)
                                    {
                                        nextMove = TurnRight;
                                    }
                                    else
                                    {
                                        nextMove = TurnLeft;
                                        if (GetForwardTileStatus() == 1)
                                        {
                                            nextMove = TurnLeft;
                                            //jos toinenkin k��nn�s on sein�, tarkistetaan onko vastap�inen sein� siit� sein�, vai palataanko takaisin
                                            if (GetForwardTileStatus() == 1)
                                            {
                                                nextMove = TurnLeft;
                                                switch (GetForwardTileStatus())
                                                {
                                                    case 0: nextMove = MoveForward; break;
                                                    case 1: nextMove = TurnLeft; break;
                                                    case 2: Hit(); break;
                                                    default: Pass(); break;
                                                }
                                            }

                                        }
                                    }
                                }
                                kaannoksiaVasempaan++;
                            }
                            //kun ollaan tehty 4 kierros, katsotaan ollaanko samassa kohdassa kuin aikaisemmin.
                            if (kaannoksiaVasempaan >= 4)
                            {
                                //jos majakka vastaa sijaintia, tiedet��n ett� kierret��n keh��, t�ysk��nn�s
                                if (majakkaX == sijaintiX && majakkaY == sijaintiY)
                                {
                                    nextMove = TurnRight;
                                    if (GetForwardTileStatus() == 1)
                                    {
                                        nextMove = TurnRight;
                                    }

                                }
                                else
                                    nextMove = TurnRight;
                                kaannoksiaVasempaan = 0;
                            }
                            kaannoksiaOikeaan = 0;
                            break;
                        case 2: Hit(); break;
                        default: nextMove = Pass; break;
                    }
                }
                break;
            case 2: nextMove = Hit; break; //jos edess� on vihu, ly�
            default: nextMove = Pass; break;
        }

    }

}