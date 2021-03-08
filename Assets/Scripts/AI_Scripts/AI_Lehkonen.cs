using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AI_Lehkonen : PlayerControllerInterface
{
    private System.Random rnd = new System.Random();
    private NextMove lastMove;
    private int moveCount = 0;
    private int random;
    // (1,0) = oikealle | (-1,0) = vasemmalle | (0,1) = yl�s | (0,-1) = alas



    // T�M� TULEE T�YDENT��
    // K�yt� vain PlayerControllerInterfacessa olevia metodeja TIMiss� olevan ohjeistuksen mukaan
    public override void DecideNextMove()
    {
        lastMove = nextMove;
        Vector2 self = GetPosition();
        Vector2 selfRot = GetRotation();
        Vector2[] enemies = GetEnemyPositions();
        Vector2 closestEnemy = enemies[0];
        Vector2 targetEnemy = enemies[0];
        bool enemyfound = false;

        //Debug.Log(moveCount);
        // L�hin kohde
        foreach (Vector2 enemy in enemies)
        {
            if (Vector2.Distance(self, enemy) < Vector2.Distance(self, targetEnemy))
            {
                closestEnemy = enemy;
            }
        }
        // Pieni hp kohde, painotuksena l�hin (Ei k�ytet� muuta osiota kuin l�hin taistelua varten, koska kohdistaminen ei toiminut)
        foreach (Vector2 enemy in enemies)
        {
            // Alle 3HP target JA l�hin (ei k�ytet�)
            if (GetEnemyHP(enemy) < 3 && enemy == closestEnemy)
            {
                targetEnemy = enemy;
                enemyfound = true;
                break;
            }
            // Mik� vain alle 3HP target
            else if (GetEnemyHP(enemy) < 3)
            {
                targetEnemy = enemy;
                enemyfound = true;
            }
            // Jos ei l�ydy muita kuin 3HP, otetaan kohteeksi l�hin
            if (!enemyfound)
            {
                targetEnemy = closestEnemy;
            }
        }
        Vector2 targetEnemyRot = GetEnemyRotation(targetEnemy);

        // Tyhm� teko�ly, liikkuu eteenp�in jos edess� on tyhj� ruutu. Yrityst� oli, mutta en saanut Vector2.x toimimaan pidemm�ll� matkalla kuin pelk�st��n takana tai vieress�.
        if (GetForwardTileStatus() == 0)
        {
            // Normi liikett�
            nextMove = MoveForward;
            // Pistet��n v�h�n lis�� random geni� ettei j�� liikkumaan edestakaisin, varmistetaan ettei tapahdu jumiinj��misen v�ltt�misen varmistuksen j�lkeen
            if (moveCount > 5 && lastMove == MoveForward)
            {
                random = rnd.Next(0, 2);
                switch (random)
                {
                    //Right
                    case 0:
                        nextMove = TurnRight;
                        break;
                    //Left
                    case 1:
                        nextMove = TurnLeft;
                        break;
                    default:
                        break;
                }
                moveCount = 0;
            }
            moveCount++;

            //Yritys tehd� jonkinlainen liikkeen seuranta. Vektorien k�ytt� ei luonnistunut, joten menn��n takaisin tyhm��n k�yt�nt�tapaan.
            /*
            // kohde oikealla
            if (Mathf.Abs(targetEnemy.x) - Mathf.Abs(self.x) > 0f)
            {
                Debug.Log("enemy right");
                if (selfRot == Vector2.right)
                {
                    nextMove = MoveForward;
                }
                else
                {
                    nextMove = TurnRight;
                }
            }
            // kohde vasemmalla
            else if (Mathf.Abs(targetEnemy.x) - Mathf.Abs(self.x) < 0f)
            {
                Debug.Log("enemy left");
                if (selfRot == Vector2.left)
                {
                    nextMove = MoveForward;
                }
                else
                {
                    nextMove = TurnRight;
                }
            }
            else
            {
                Debug.Log("x vector match");
                //kohde alapuolella
                if (Mathf.Abs(targetEnemy.x) - Mathf.Abs(self.x) < 0f)
                {
                    Debug.Log("enemy below");
                    if (selfRot == Vector2.down)
                    {
                        nextMove = MoveForward;
                    }
                    else
                    {
                        nextMove = TurnRight;
                    }
                }
                //kohde ylhaalla
                if (Mathf.Abs(targetEnemy.x) - Mathf.Abs(self.x) > 0f)
                {
                    Debug.Log("enemy above");
                    if (selfRot == Vector2.up)
                    {
                        nextMove = MoveForward;
                    }
                    else
                    {
                        nextMove = TurnRight;
                    }
                }
            }
                */

        }
        // jos sein�, random suunta ja kiellet��n aikaisempi k��nn�s.
        else if (GetForwardTileStatus() == 1)
        {
            random = rnd.Next(0, 2);
            switch (random)
            {
                //Right
                case 0:
                    //varmistetaan liikkeen oikeellisuus, ei toisteta aikaisempaan k��nn�st� eli v�ltet��n j��m�st� jumiin
                    if (lastMove != TurnLeft)
                    {
                        nextMove = TurnRight;
                    }
                    else
                    {
                        nextMove = TurnLeft;
                    }
                    break;
                //Left
                case 1:
                    //varmistetaan liikkeen oikeellisuus, ei toisteta aikaisempaa k��nn�st� eli v�ltet��n j��m�st� jumiin
                    if (lastMove != TurnRight)
                    {
                        nextMove = TurnLeft;
                    }
                    else
                    {
                        nextMove = TurnRight;
                    }
                    break;
                default:
                    break;
            }
        }
        //jos vastustaja edess�, yrit� pakenemista jos vastustajan HP on enemm�n (oletetaan k��nn�st�, joten varmistetaan selviytyminen)
        else if (GetForwardTileStatus() == 2)
        {
            //omalla HP:lla ei ole v�li� jos k��ntyy ja ottaa ly�nnin sivulle. Parempi tarkistaa, ett� voittaa kasvotusten.
            if (GetEnemyHP(targetEnemy) > GetHP())
            {
                int random = rnd.Next(0, 2);
                //random valinta mihin k��ntyy pakoon
                switch (random)
                {
                    case 0:
                        nextMove = TurnLeft;
                        break;
                    case 1:
                        nextMove = TurnRight;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                nextMove = Hit;
            }
        }
        // Muuten ei tee mit��n
        else
        {
            nextMove = Pass;
        }

    }
}