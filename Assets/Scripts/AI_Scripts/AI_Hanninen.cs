using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Hanninen : PlayerControllerInterface
{

    //Alustetaan muuttujat, joiden tarkoitus on pitää kirjaa edellisen vuoron paikasta, suunnasta ja siitä törmättinkö seinään
    Vector2 previousPosition = new Vector2(0f, 0f);
    int previousDirection = 1;
    bool collidedWithWallLastTurn = false;

    public override void DecideNextMove()
    {
        //alustetaan metodin sisäiset muuttujat
        int randomDirection = 1;
        int ownDirection = 2;
        bool enemyBehind = false;

        //haetaan oma positio ja rotaatio ja vihollisten rotaatiot 
        Vector2 ownPosition = GetPosition();
        Vector2 ownRotation = GetRotation();
        Vector2[] enemies = GetEnemyPositions();

        //tallennetaan pelaajan nykyinen kulkusuunta ownDirection muuttujaan, jotta sitä on helpompi käyttää myöhemmin, muotoseikka
        //SUUNNAT:
        //0 = ylös
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

        //Jos edessä on vihollinen lyödään sitä eikä edes käydä loppukoodia läpi
        if (GetForwardTileStatus() == 2)
        {
            nextMove = Hit;
            collidedWithWallLastTurn = false;
            return;
        }

        //lisätään hieman arvaamattomuutta pelaajan liikkeisiin arpomalla random arvon välillä 1-99 
        int randomMovementCheck = Random.Range(1, 100);
        //annetaan pieni mahdollisuus satunnaiseen liikkumiseen
        //ei voi tapahtua jos oltiin jumissa seinässä edellisellä vuorolla, jotta tämä ei vaikeuta loopista ulospääsyä
        //satunnainen liikkuminen voi joskus aiheuttaa huonoja ratkaisuja, mutta tekee ainakin teoriassa botista hankalamman seurata
        if (randomMovementCheck > 94 && !collidedWithWallLastTurn)
        {
            //arvotaan käännyttänkö oikealle vai vasemmalle
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

        //käydään for-loopilla läpi kaikkien vihollisten sijainnit
        for (int i = 0; i < enemies.Length; i++)
        {
            //asetetaan sijainnit muuttujiin
            float enemyX = enemies[i].x;
            float enemyY = enemies[i].y;

            //käydään switch casella läpi, mihin suuntaan pelaaja on menossa
            switch (ownDirection)
            {
                //ylös
                case 0:
                    //tarkistetaan onko vihollinen pelaajan takana
                    //tässä tapauksessa kun pelaaja on menossa ylös, jos vihollisella on sama x-koordinaatti ja y-koordinaatti on pienemmi kuin pelaajan, katsotaan vihollisen olevan takana
                    //kuitenkaan kaukaisista vihollisista ei kannata välittää, joten vain 1-3 ruudun päässä olevat botit huomioidaan
                    if ((enemyY < (ownPosition.y - 0.5f) && enemyY > (ownPosition.y - 3.5f)) && enemyX == ownPosition.x)
                        enemyBehind = true;
                    break;
                //alas
                case 1:
                    //sama tehdään kaikille muillle suunnille
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

        //jos edeltävässä for-loopissa kävi ilmi, että vihollinen on takana, käännytään satunnaiseen suuntaan
        if (enemyBehind)
        {
            //arvotaan käännyttänkö oikealle vai vasemmalle
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

        //switch case päätöksen tekoa varten, jos sitä ei ole vielä tehty
        switch (GetForwardTileStatus())
        {
            case 0: //tyhjä ruutu, liikutaan suoraan eteenpäin
                nextMove = MoveForward;
                collidedWithWallLastTurn = false;
                return;
            case 1: //seinä
                    //kerrotaa muuttujalle, että tällä vuorolla törmättiin seinään
                collidedWithWallLastTurn = true;
                //jos edellisen vuoron positio on sama kuin nykyinen positio (eli ollaan jumissa) ei arvota suunnanvaihtoa
                if (previousPosition == ownPosition)
                {
                    //katsotaan mihin suuntaan viime vuorolla siirryttiin ja siirrytään samaan suuntaan. tämän pitäisi estää pahimmat jumitilanteet
                    //esimerkki: pelaaja menee kolmen seinän ympäröimään nurkaukseen. pelkkä random kääntyminen voi aiheuttaa pelaajan kääntymään
                    //jatkuvasti edes takaisin, pysyen nurkassa.
                    //jos puolestaan käännytään samaan suuntaan kuin viime kerralla, päästään varmasti pois jumista kahdella vuorolla, ellei vihollinen satu samaan aikaan olemaan selän takana
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
                    //jos viime vuorolla ei osuttu seinään (ei olla jumissa) voidaan valita random kääntymissuunta
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