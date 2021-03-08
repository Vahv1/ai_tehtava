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
    // (1,0) = oikealle | (-1,0) = vasemmalle | (0,1) = ylös | (0,-1) = alas



    // TÄMÄ TULEE TÄYDENTÄÄ
    // Käytä vain PlayerControllerInterfacessa olevia metodeja TIMissä olevan ohjeistuksen mukaan
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
        // Lähin kohde
        foreach (Vector2 enemy in enemies)
        {
            if (Vector2.Distance(self, enemy) < Vector2.Distance(self, targetEnemy))
            {
                closestEnemy = enemy;
            }
        }
        // Pieni hp kohde, painotuksena lähin (Ei käytetä muuta osiota kuin lähin taistelua varten, koska kohdistaminen ei toiminut)
        foreach (Vector2 enemy in enemies)
        {
            // Alle 3HP target JA lähin (ei käytetä)
            if (GetEnemyHP(enemy) < 3 && enemy == closestEnemy)
            {
                targetEnemy = enemy;
                enemyfound = true;
                break;
            }
            // Mikä vain alle 3HP target
            else if (GetEnemyHP(enemy) < 3)
            {
                targetEnemy = enemy;
                enemyfound = true;
            }
            // Jos ei löydy muita kuin 3HP, otetaan kohteeksi lähin
            if (!enemyfound)
            {
                targetEnemy = closestEnemy;
            }
        }
        Vector2 targetEnemyRot = GetEnemyRotation(targetEnemy);

        // Tyhmä tekoäly, liikkuu eteenpäin jos edessä on tyhjä ruutu. Yritystä oli, mutta en saanut Vector2.x toimimaan pidemmällä matkalla kuin pelkästään takana tai vieressä.
        if (GetForwardTileStatus() == 0)
        {
            // Normi liikettä
            nextMove = MoveForward;
            // Pistetään vähän lisää random geniä ettei jää liikkumaan edestakaisin, varmistetaan ettei tapahdu jumiinjäämisen välttämisen varmistuksen jälkeen
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

            //Yritys tehdä jonkinlainen liikkeen seuranta. Vektorien käyttö ei luonnistunut, joten mennään takaisin tyhmään käytäntötapaan.
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
        // jos seinä, random suunta ja kielletään aikaisempi käännös.
        else if (GetForwardTileStatus() == 1)
        {
            random = rnd.Next(0, 2);
            switch (random)
            {
                //Right
                case 0:
                    //varmistetaan liikkeen oikeellisuus, ei toisteta aikaisempaan käännöstä eli vältetään jäämästä jumiin
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
                    //varmistetaan liikkeen oikeellisuus, ei toisteta aikaisempaa käännöstä eli vältetään jäämästä jumiin
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
        //jos vastustaja edessä, yritä pakenemista jos vastustajan HP on enemmän (oletetaan käännöstä, joten varmistetaan selviytyminen)
        else if (GetForwardTileStatus() == 2)
        {
            //omalla HP:lla ei ole väliä jos kääntyy ja ottaa lyönnin sivulle. Parempi tarkistaa, että voittaa kasvotusten.
            if (GetEnemyHP(targetEnemy) > GetHP())
            {
                int random = rnd.Next(0, 2);
                //random valinta mihin kääntyy pakoon
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
        // Muuten ei tee mitään
        else
        {
            nextMove = Pass;
        }

    }
}