using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Hanninen : PlayerControllerInterface
{

    //Alustetaan muuttujat, joiden tarkoitus on pit�� kirjaa edellisen vuoron paikasta, suunnasta ja siit� t�rm�ttink� sein��n
    Vector2 previousPosition = new Vector2(0f, 0f);
    int previousDirection = 1;
    bool collidedWithWallLastTurn = false;

    public override void DecideNextMove()
    {
        //alustetaan metodin sis�iset muuttujat
        int randomDirection = 1;
        int ownDirection = 2;
        bool enemyBehind = false;

        //haetaan oma positio ja rotaatio ja vihollisten rotaatiot 
        Vector2 ownPosition = GetPosition();
        Vector2 ownRotation = GetRotation();
        Vector2[] enemies = GetEnemyPositions();

        //tallennetaan pelaajan nykyinen kulkusuunta ownDirection muuttujaan, jotta sit� on helpompi k�ytt�� my�hemmin, muotoseikka
        //SUUNNAT:
        //0 = yl�s
        //1 = alas
        //2 = oikealle
        //3 = vasemmalle
        if (ownRotation.y > 0.5f)
            ownDirection = 0;
        else if (ownRotation.y < -0.5f)
            ownDirection = 1;
        else if (ownRotation.x == 1f)
            ownDirection = 2;
        else if (ownRotation.x == -1f)
            ownDirection = 3;

        //Jos edess� on vihollinen ly�d��n sit� eik� edes k�yd� loppukoodia l�pi
        if (GetForwardTileStatus() == 2)
        {
            nextMove = Hit;
            collidedWithWallLastTurn = false;
            return;
        }

        //lis�t��n hieman arvaamattomuutta pelaajan liikkeisiin arpomalla random arvon v�lill� 1-99 
        int randomMovementCheck = Random.Range(1, 100);
        //annetaan pieni mahdollisuus satunnaiseen liikkumiseen
        //ei voi tapahtua jos oltiin jumissa sein�ss� edellisell� vuorolla, jotta t�m� ei vaikeuta loopista ulosp��sy�
        //satunnainen liikkuminen voi joskus aiheuttaa huonoja ratkaisuja, mutta tekee ainakin teoriassa botista hankalamman seurata
        if (randomMovementCheck > 94 && !collidedWithWallLastTurn)
        {
            //arvotaan k��nnytt�nk� oikealle vai vasemmalle
            randomDirection = Random.Range(1, 3);
            if (randomDirection == 1)
            {
                nextMove = TurnLeft;
                //asetetaan nykyinen sijainti muuttujiin seuraavaa vuoroa varten
                previousPosition = ownPosition;
                previousDirection = 1;
            }

            else if (randomDirection == 2)
            {
                nextMove = TurnRight;
                //asetetaan nykyinen sijainti muuttujiin seuraavaa vuoroa varten
                previousPosition = ownPosition;
                previousDirection = 2;
            }
            collidedWithWallLastTurn = false;
            return;
        }

        //k�yd��n for-loopilla l�pi kaikkien vihollisten sijainnit
        for (int i = 0; i < enemies.Length; i++)
        {
            //asetetaan sijainnit muuttujiin
            float enemyX = enemies[i].x;
            float enemyY = enemies[i].y;

            //k�yd��n switch casella l�pi, mihin suuntaan pelaaja on menossa
            switch (ownDirection)
            {
                //yl�s
                case 0:
                    //tarkistetaan onko vihollinen pelaajan takana
                    //t�ss� tapauksessa kun pelaaja on menossa yl�s, jos vihollisella on sama x-koordinaatti ja y-koordinaatti on pienemmi kuin pelaajan, katsotaan vihollisen olevan takana
                    //kuitenkaan kaukaisista vihollisista ei kannata v�litt��, joten vain 1-3 ruudun p��ss� olevat botit huomioidaan
                    if ((enemyY < (ownPosition.y - 0.5f) && enemyY > (ownPosition.y - 3.5f)) && enemyX == ownPosition.x)
                        enemyBehind = true;
                    break;
                //alas
                case 1:
                    //sama tehd��n kaikille muillle suunnille
                    if ((enemyY > (ownPosition.y + 0.5f) && enemyY < (ownPosition.y + 3.5f)) && enemyX == ownPosition.x)
                        enemyBehind = true;
                    break;
                //oikealle
                case 2:
                    if ((enemyX < (ownPosition.x - 0.5f) && enemyX > (ownPosition.x - 3.5f)) && enemyY == ownPosition.y)
                        enemyBehind = true;
                    break;
                //vasemmalle
                case 3:
                    if ((enemyX > (ownPosition.y + 0.5f) && enemyX < (ownPosition.y + 3.5f)) && enemyY == ownPosition.y)
                        enemyBehind = true;
                    break;
            }
        }

        //jos edelt�v�ss� for-loopissa k�vi ilmi, ett� vihollinen on takana, k��nnyt��n satunnaiseen suuntaan
        if (enemyBehind)
        {
            //arvotaan k��nnytt�nk� oikealle vai vasemmalle
            randomDirection = Random.Range(1, 3);
            if (randomDirection == 1)
            {
                nextMove = TurnLeft;
                collidedWithWallLastTurn = false;
                previousPosition = ownPosition;
                previousDirection = 1;
                return;
            }
            else if (randomDirection == 2)
            {
                nextMove = TurnRight;
                collidedWithWallLastTurn = false;
                previousPosition = ownPosition;
                previousDirection = 2;
                return;
            }
            enemyBehind = false;
        }

        //switch case p��t�ksen tekoa varten, jos sit� ei ole viel� tehty
        switch (GetForwardTileStatus())
        {
            case 0: //tyhj� ruutu, liikutaan suoraan eteenp�in
                nextMove = MoveForward;
                collidedWithWallLastTurn = false;
                return;
            case 1: //sein�
                    //kerrotaa muuttujalle, ett� t�ll� vuorolla t�rm�ttiin sein��n
                collidedWithWallLastTurn = true;
                //jos edellisen vuoron positio on sama kuin nykyinen positio (eli ollaan jumissa) ei arvota suunnanvaihtoa
                if (previousPosition == ownPosition)
                {
                    //katsotaan mihin suuntaan viime vuorolla siirryttiin ja siirryt��n samaan suuntaan. t�m�n pit�isi est�� pahimmat jumitilanteet
                    //esimerkki: pelaaja menee kolmen sein�n ymp�r�im��n nurkaukseen. pelkk� random k��ntyminen voi aiheuttaa pelaajan k��ntym��n
                    //jatkuvasti edes takaisin, pysyen nurkassa.
                    //jos puolestaan k��nnyt��n samaan suuntaan kuin viime kerralla, p��st��n varmasti pois jumista kahdella vuorolla, ellei vihollinen satu samaan aikaan olemaan sel�n takana
                    if (previousDirection == 1)
                    {
                        nextMove = TurnLeft;
                        previousPosition = ownPosition;
                        previousDirection = 1;

                    }
                    else
                    {
                        nextMove = TurnRight;
                        previousPosition = ownPosition;
                        previousDirection = 2;
                    }
                    return;
                }
                else
                {
                    //jos viime vuorolla ei osuttu sein��n (ei olla jumissa) voidaan valita random k��ntymissuunta
                    randomDirection = Random.Range(1, 3);
                    if (randomDirection == 1)
                    {
                        nextMove = TurnLeft;
                        previousPosition = ownPosition;
                        previousDirection = 1;
                    }

                    else if (randomDirection == 2)
                    {
                        nextMove = TurnRight;
                        previousPosition = ownPosition;
                        previousDirection = 2;
                    }
                    return;
                }

        }


    }
}