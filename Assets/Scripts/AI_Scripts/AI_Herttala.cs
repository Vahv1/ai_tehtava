using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Herttala : PlayerControllerInterface
{
    //paljonko on liikuttu vapaaseen suuntaan jmumissa olemisen jälkeen
    //paljonko on pyöritty seinien törmäyksen takia = jumissa
    int liikkunut = 0;
    int pyorinta = 0;


    // TÄMÄ TULEE TEHTÄVÄSSÄ TÄYDENTÄÄ
    // Käytä vain PlayerControllerInterfacessa olevia metodeja TIMissä olevan ohjeistuksen mukaan
    public override void DecideNextMove()
    {
        Vector2[] vihut = GetEnemyPositions();
        Vector2 omaSijainti = GetPosition();
        Vector2 kaanto = GetRotation();

        float lyhinEtaisyys = 9999999999999999999f;
        int lahinVihu = -1;
        Vector2 lahinVihuSijainti;


        // Jos edessä vihu, lyö
        if (GetForwardTileStatus() == 2)
        {
            nextMove = Hit;
            pyorinta = 0;
            return;
        }

        //Debug.Log(pyorinta);
        //Debug.Log(liikkunut);

        // Jos edessä seinä niin aRvotaan oikea vai vasen
        if (GetForwardTileStatus() == 1)
        {
            int kumpi = Random.Range(0, 2);
            if (kumpi == 0)
            {
                nextMove = TurnRight;
                pyorinta++;
                return;
            }
            else
            {
                nextMove = TurnLeft;
                pyorinta++;
                return;
            }
        }


        //ARvotaan määrä mitä pitää liikku vapaaseen suuntaan jumissa olemisen jälkeen -> Pääsee pois monista jumikohdista muttei kovin tehokas.
        if (liikkunut > Random.Range(2, 5))
        {
            pyorinta = 0;
            liikkunut = 0;
        }

        //jumissa, arvotaan suunta mihin käännytään ja liikutaan random verran ennenkun jahistetaan vihua taas
        if (pyorinta > 4)
        {
            if (GetForwardTileStatus() == 0)
            {
                nextMove = MoveForward;
                liikkunut++;
                return;
            }

            if (GetForwardTileStatus() == 1)
            {
                int kumpi = Random.Range(0, 2);
                if (kumpi == 0)
                {
                    nextMove = TurnRight;
                    pyorinta++;
                    liikkunut = 0;
                    return;
                }
                else
                {
                    nextMove = TurnLeft;
                    liikkunut = 0;
                    pyorinta++;
                    return;
                }
            }
        }

        //etsii lähimmän vihun ja ottaa sen talteen
        for (int i = 0; i < vihut.Length; i++)
        {
            float etaisyysVihuun = Vector2.Distance(omaSijainti, vihut[i]);
            if (etaisyysVihuun < lyhinEtaisyys)
            {
                lyhinEtaisyys = etaisyysVihuun;
                lahinVihu = i;
                lahinVihuSijainti = vihut[i];
            }
        }




        //--------------------LÄHIN VIHU JA SUUNTAUS JA LIIKKUMINEN


        bool pitaakoKaantya = false;
        float etaisyysX = omaSijainti.x - vihut[lahinVihu].x;
        float etaisyysY = omaSijainti.y - vihut[lahinVihu].y;

        bool xLahempi = true;

        //Tarkistimet onko lähin vihu samalla x tai y koordinaatilla
        bool xSama = false;
        bool ySama = false;

        if (omaSijainti.x == vihut[lahinVihu].x)
        {
            xSama = true;
            nextMove = TurnRight; liikkunut = 0;
            pyorinta++;
        }

        if (omaSijainti.y == vihut[lahinVihu].y)
        {
            ySama = true;
            nextMove = TurnRight; liikkunut = 0;
            pyorinta++;
        }

        //Jos lähin vihu oikealla päin ja suunta oikea niin liikutaan vihua kohti oikealle 
        if (!xSama && kaanto == new Vector2(1, 0) && vihut[lahinVihu].x > omaSijainti.x)
        {
            nextMove = MoveForward;
            pyorinta = 0; liikkunut = 0;
            return;
        }

        //Jos lähin vihu vas päin ja suunta vas niin liikutaan vihua kohti oikealle 
        if (!xSama && kaanto == new Vector2(-1, 0) && vihut[lahinVihu].x < omaSijainti.x)
        {
            nextMove = MoveForward;
            pyorinta = 0; liikkunut = 0;
            return;
        }

        //Jos lähin vihu yläällä päin ja suunta ylös 
        if (!ySama && kaanto == new Vector2(0, 1) && vihut[lahinVihu].y > omaSijainti.y)
        {
            nextMove = MoveForward;
            pyorinta = 0; liikkunut = 0;
            return;
        }

        //Jos lähin vihu alaalla päin ja suunta alas
        if (!ySama && kaanto == new Vector2(0, -1) && vihut[lahinVihu].y < omaSijainti.y)
        {
            nextMove = MoveForward;
            pyorinta = 0;
            liikkunut = 0;
            return;
        }






        if (Mathf.Abs(etaisyysX) > Mathf.Abs(etaisyysY))
        {
            xLahempi = false;
        }

        //vihu on hahmon oikealla ja hahmo ei oikealla ja x on lahempi
        if (xLahempi && etaisyysX < 0 && kaanto != new Vector2(1, 0))
        {
            pitaakoKaantya = true;
        }

        //vihu on hahmon vasen ja hahmo ei vasemmalle ja x on lahempi
        if (xLahempi && etaisyysX > 0 && kaanto != new Vector2(-1, 0))
        {
            pitaakoKaantya = true;
        }


        //vihu on hahmon yl ja hahmo ei yl ja y on lahempi
        if (!xLahempi && etaisyysY < 0 && kaanto != new Vector2(0, 1))
        {
            pitaakoKaantya = true;
        }

        //vihu on hahmon alaalla ja hahmo ei alas ja y on lahempi
        if (!xLahempi && etaisyysY > 0 && kaanto != new Vector2(0, -1))
        {
            pitaakoKaantya = true;
        }


        if (pitaakoKaantya)
        {
            nextMove = TurnRight;
            pyorinta++;
            liikkunut = 0;
        }










    }
}