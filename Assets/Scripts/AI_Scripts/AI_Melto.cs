using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Melto : PlayerControllerInterface
{
    int kaannos;
    int askel;
    bool kaannosOikeaan;
    // T�M� TULEE TEHT�V�SS� T�YDENT��
    // K�yt� vain PlayerControllerInterfacessa olevia metodeja TIMiss� olevan ohjeistuksen mukaan
    public override void DecideNextMove()
    {
        switch (GetForwardTileStatus())
        {
            // Enint��n 10 askelta eteenp�in, sitten k��nnyt��n oikealle
            // ja taas jos p��see liikkumaan 8 askelta suoraan, k��nnyt��n vasemmalle.
            // N�in ei pit�isi j��d� jumiin.
            case 0:
                if (askel < 10)
                {
                    nextMove = MoveForward;
                    askel++;
                    break;
                }
                else if (kaannosOikeaan)
                {
                    nextMove = TurnRight;
                    kaannosOikeaan = false;
                    askel = 0;
                    break;
                }
                else if (!kaannosOikeaan)
                {
                    nextMove = TurnLeft;
                    kaannosOikeaan = true;
                    askel = 0;
                    break;
                }
                break;
            // Kaksi kertaa k��nnyt��n oikealle, sitten kaksi kertaa vasemmalle
            // T�m�n j�lkeen kaannos nollataan ja aloitetaan alusta
            case 1:
                if (kaannos < 2)
                {
                    nextMove = TurnRight;
                    kaannos++;
                    break;
                }
                else
                {
                    nextMove = TurnLeft;
                    kaannos++;
                    if (kaannos == 3) kaannos = 0;
                    break;
                }
            case 2:
                nextMove = Hit;
                break;
            default:
                nextMove = Pass;
                break;
        }
    }
}