using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AI_Rautiainen : PlayerControllerInterface
{

    float nykyinenLeveys;
    float nykyinenKorkeus;
    int jumissa = 0;
    int kaanto;
    int edellinenKaanto;
    int samatKaannot = 0;
    bool eteen;
    Vector2 edellinenSijainti;


    public override void DecideNextMove()
    {
        // Oma ja vihollisten sijainnit
        Vector2[] vihut = GetEnemyPositions();
        Vector2 omaSijainti = GetPosition();


        // Paikalleen j‰‰nnin varmistus
        if (omaSijainti == edellinenSijainti)
        {
            jumissa++;
        }
        else jumissa = 0;

        edellinenSijainti = GetPosition();

        // Lyˆd‰‰n aina kun mahdollista
        if (GetForwardTileStatus() == 2)
        {
            nextMove = Hit;
            return;
        }

        // V‰lill‰ seuraamisen takia j‰‰ jumiin, kun vihu on paikallaan sein‰n takana. Estet‰‰n t‰ll‰ jumiin j‰‰nti
        if (jumissa > 5)
        {
            if (GetForwardTileStatus() == 0)
            {
                nextMove = MoveForward;
            }

            else nextMove = TurnLeft;
            return;
        }



        // vihollisten ollessa tarpeeksi l‰hell‰, aloitetaan jahtaaminen. Toimii hieman bugisesti v‰lill‰, mutta oikein yleens‰. :D
        // Ei aleta seuraamaan jos oma selk‰ on viholliseen p‰in
        foreach (Vector2 vihu in vihut)
        {
            // floatin pyˆrist‰minen ei meinannut onnistua. Kierret‰‰n se t‰ll‰.
            double vihux = vihu.x;
            double vihuy = vihu.y;
            double omax = omaSijainti.x;
            double omay = omaSijainti.y;

            // Ylh‰‰ll‰ oleviin vihollisiin k‰‰ntyminen.
            if ((GetRotation().x == 1 || GetRotation().x == -1) && vihu.y > omaSijainti.y && vihu.y - omaSijainti.y < 3 && Math.Round(vihux, 2) == Math.Round(omax, 2))
            {
                if (GetRotation().x == 1) nextMove = TurnLeft;
                else nextMove = TurnRight;
                return;
            }




            // Alhaalla oleviin vihollisiin k‰‰ntyminen.
            if ((GetRotation().x == 1 || GetRotation().x == -1) && vihu.y < omaSijainti.y && omaSijainti.y - vihu.y < 3 && Math.Round(vihux, 2) == Math.Round(omax, 2))
            {
                if (GetRotation().x == -1) nextMove = TurnLeft;
                else nextMove = TurnRight;
                return;
            }



            // Oikealla oleviin vihollisiin k‰‰ntyminen.
            if ((GetRotation().y == 1 || GetRotation().y == -1) && vihu.x > omaSijainti.x && vihu.x - omaSijainti.x < 3 && Math.Round(vihuy, 2) == Math.Round(omay, 2))
            {
                if (GetRotation().y == 1) nextMove = TurnRight;
                else nextMove = TurnLeft;
                return;
            }


            // Vasemmalla oleviin vihollisiin k‰‰ntyminen.
            if ((GetRotation().x == -1 || GetRotation().x == 1) && vihu.x < omaSijainti.x && omaSijainti.x - vihu.x < 3 && Math.Round(vihuy, 2) == Math.Round(omay, 2))
            {
                if (GetRotation().y == -1) nextMove = TurnLeft;
                else nextMove = TurnRight;
                return;
            }
        }




        // Tyhm‰ teko‰ly, liikkuu eteenp‰in jos edess‰ on tyhj‰ ruutu
        if (GetForwardTileStatus() == 0)
        {
            nextMove = MoveForward;
            kaanto = 0;
        }

        // Jos j‰‰ kiert‰m‰‰n neliˆt‰. T‰m‰ est‰‰ jumiin j‰‰nnin ja k‰‰nt‰‰ v‰lill‰ eri suuntaan.
        // Halusin v‰ltt‰‰ randomin k‰ytˆn joten kokeilin t‰t‰.
        else if (samatKaannot > 5)
        {
            nextMove = TurnLeft;
            samatKaannot = 0;
        }


        // K‰‰ntyminen ja samalla edellisen kohdan laskurin kasvatus.
        else
        {

            nextMove = TurnRight;
            kaanto = 1;
            if (edellinenKaanto == kaanto) samatKaannot++;
            edellinenKaanto = 1;

        }

    }
}