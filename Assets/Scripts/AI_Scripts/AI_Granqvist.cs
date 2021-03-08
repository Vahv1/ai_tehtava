using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Granqvist : PlayerControllerInterface
{
    // TÄMÄ TULEE TEHTÄVÄSSÄ TÄYDENTÄÄ
    // Käytä vain PlayerControllerInterfacessa olevia metodeja TIMissä olevan ohjeistuksen mukaan
    List<int> RUUDUT = new List<int>(); //Tallennetaan ruudun tyypit mitä on edessä
    List<int> KAANNY = new List<int>(); //Tallennetaan tehdyt käännökset
    public override void DecideNextMove()
    {

        //Tässä kohdassa etsitään lähin vihollinenn kentältä ja tallennetaan paikka listalta
        float lahin = Vector2.Distance(GetPosition(), GetEnemyPositions()[0]);
        int vihuPosInd = 0;

        for (int i = 1; i < GetEnemyPositions().Length; i++)
        {
            if (Vector2.Distance(GetPosition(), GetEnemyPositions()[i]) < lahin)
            {
                lahin = Vector2.Distance(GetPosition(), GetEnemyPositions()[i]);
                vihuPosInd = i;
            }
        }


        //Jos edessä on vihollinen niin lyödään
        //Lisätään vihollisen ruutu pelaajan ruutuihin
        if (GetForwardTileStatus() == 2)
        {
            RUUDUT.Add(2);
            nextMove = Hit;
        }



        //Kun edessä on seinä
        else if (GetForwardTileStatus() == 1)
        {


            //Tarkastetaan onko kolme viimeisintä ruutua, jos ovat olleet seiniä
            //Määritellään tehtävä käännös edellisen käännöksen mukaan
            //Tällä rikotaan mahdollinen jumiin jääminen, jos botti ei pääse seinistä irti
            if (RUUDUT[RUUDUT.Count - 1] == 1 && RUUDUT[RUUDUT.Count - 2] == 1 && RUUDUT[RUUDUT.Count - 3] == 1)
            {
                if (KAANNY[KAANNY.Count - 1] == 1)
                {
                    nextMove = TurnRight;
                }
                else
                {
                    nextMove = TurnLeft;
                }
            }

            //Rikotaan käännöslooppi, jos on käännytty neljä kertaa oikealle putkeen, käännytään vasemmalle
            else if (KAANNY.Count > 6 && (KAANNY[KAANNY.Count - 1] == 1 && KAANNY[KAANNY.Count - 2] == 1 && KAANNY[KAANNY.Count - 3] == 1 && KAANNY[KAANNY.Count - 4] == 1))
            {

                KAANNY.Add(2);
                nextMove = TurnLeft;
            }


            //Rikotaan käännöslooppi, jos on käännytty neljä kertaa oikealle putkeen, käännytään oikealle
            else if (KAANNY.Count > 6 && (KAANNY[KAANNY.Count - 1] == 2 && KAANNY[KAANNY.Count - 2] == 2 && KAANNY[KAANNY.Count - 3] == 2 && KAANNY[KAANNY.Count - 4] == 2))
            {

                KAANNY.Add(1);
                nextMove = TurnRight;
            }



            //Määritellään normaali siirto seinään osuessa lähimmän vihollisen position avulla
            //Verrataan vihollisen x ja y koordinaatteja botin omiin
            //Määritellään tämän ja botin oman suunnan mukaan seuraava käännös
            //Mahdollistetaan seuraaminen
            else
            {
                if (GetEnemyPositions()[vihuPosInd].x >= GetPosition().x && GetEnemyPositions()[vihuPosInd].y >= GetPosition().y)
                {
                    if (GetRotation() == new Vector2(0, 1))
                    {

                        RUUDUT.Add(1);
                        KAANNY.Add(1);
                        nextMove = TurnRight;
                    }
                    else if (GetRotation() == new Vector2(0, -1))
                    {

                        RUUDUT.Add(1);
                        KAANNY.Add(2);
                        nextMove = TurnLeft;
                    }
                    else if (GetRotation() == new Vector2(1, 0))
                    {

                        RUUDUT.Add(1);
                        KAANNY.Add(2);
                        nextMove = TurnLeft;
                    }
                    else
                    {

                        RUUDUT.Add(1);
                        KAANNY.Add(1);
                        nextMove = TurnRight;
                    }
                }


                else if (GetEnemyPositions()[vihuPosInd].x < GetPosition().x && GetEnemyPositions()[vihuPosInd].y >= GetPosition().y)
                {
                    if (GetRotation() == new Vector2(0, 1))
                    {

                        RUUDUT.Add(1);
                        KAANNY.Add(2);
                        nextMove = TurnLeft;
                    }
                    else if (GetRotation() == new Vector2(0, -1))
                    {

                        RUUDUT.Add(1);
                        KAANNY.Add(1);
                        nextMove = TurnRight;
                    }
                    else if (GetRotation() == new Vector2(1, 0))
                    {

                        RUUDUT.Add(1);
                        KAANNY.Add(2);
                        nextMove = TurnLeft;
                    }
                    else
                    {

                        RUUDUT.Add(1);
                        KAANNY.Add(1);
                        nextMove = TurnRight;
                    }
                }


                else if (GetEnemyPositions()[vihuPosInd].x >= GetPosition().x && GetEnemyPositions()[vihuPosInd].y < GetPosition().y)
                {
                    if (GetRotation() == new Vector2(0, 1))
                    {

                        RUUDUT.Add(1);
                        KAANNY.Add(1);
                        nextMove = TurnRight;
                    }
                    else if (GetRotation() == new Vector2(0, -1))
                    {

                        RUUDUT.Add(1);
                        KAANNY.Add(2);
                        nextMove = TurnLeft;
                    }
                    else if (GetRotation() == new Vector2(1, 0))
                    {

                        RUUDUT.Add(1);
                        KAANNY.Add(1);
                        nextMove = TurnRight;
                    }
                    else
                    {

                        RUUDUT.Add(1);
                        KAANNY.Add(2);
                        nextMove = TurnLeft;
                    }
                }


                else
                {
                    if (GetRotation() == new Vector2(0, 1))
                    {

                        RUUDUT.Add(1);
                        KAANNY.Add(2);
                        nextMove = TurnLeft;
                    }
                    else if (GetRotation() == new Vector2(0, -1))
                    {

                        RUUDUT.Add(1);
                        KAANNY.Add(1);
                        nextMove = TurnRight;
                    }
                    else if (GetRotation() == new Vector2(1, 0))
                    {

                        RUUDUT.Add(1);
                        KAANNY.Add(1);
                        nextMove = TurnRight;
                    }
                    else
                    {

                        RUUDUT.Add(1);
                        KAANNY.Add(2);
                        nextMove = TurnLeft;
                    }
                }

            }

        }

        else
        {
            RUUDUT.Add(0);


            //Botti kääntyy vihollista kohti, mikäli tämän x tai y koordinaatti on viereisessä ruudussa ja HP on jäljellä yli 1
            //Tarkistetaan oma suunta, jonka mukaan määritellään käännös
            if (GetPosition().x + 1 == GetEnemyPositions()[vihuPosInd].x && GetPosition().y == GetEnemyPositions()[vihuPosInd].y && GetHP() > 1)
            {
                if (GetRotation() == new Vector2(0, 1))
                {

                    nextMove = TurnRight;
                }
                else if (GetRotation() == new Vector2(0, -1))
                {

                    nextMove = TurnLeft;
                }
            }

            else if (GetPosition().x - 1 == GetEnemyPositions()[vihuPosInd].x && GetPosition().y == GetEnemyPositions()[vihuPosInd].y && GetHP() > 1)
            {
                if (GetRotation() == new Vector2(0, 1))
                {

                    nextMove = TurnLeft;
                }
                else if (GetRotation() == new Vector2(0, -1))
                {

                    nextMove = TurnRight;
                }
            }

            else if (GetPosition().y + 1 == GetEnemyPositions()[vihuPosInd].y && GetPosition().x == GetEnemyPositions()[vihuPosInd].x && GetHP() > 1)
            {
                if (GetRotation() == new Vector2(1, 0))
                {

                    nextMove = TurnLeft;
                }
                else if (GetRotation() == new Vector2(-1, 0))
                {

                    nextMove = TurnRight;
                }
            }


            else if (GetPosition().y - 1 == GetEnemyPositions()[vihuPosInd].y && GetPosition().x == GetEnemyPositions()[vihuPosInd].x && GetHP() > 1)
            {
                if (GetRotation() == new Vector2(1, 0))
                {

                    nextMove = TurnRight;
                }
                else if (GetRotation() == new Vector2(-1, 0))
                {

                    nextMove = TurnLeft;
                }
            }



            //Botti kääntyy vihollista karkuun, mikäli tämän x tai y koordinaatti on viereisessä ruudussa ja on 1 HP jäljellä
            //Tarkistetaan oma suunta, jonka mukaan määritellään käännös
            if (GetPosition().x + 1 == GetEnemyPositions()[vihuPosInd].x && GetPosition().y == GetEnemyPositions()[vihuPosInd].y && GetHP() == 1)
            {
                if (GetRotation() == new Vector2(0, 1))
                {

                    nextMove = TurnLeft;
                }
                else if (GetRotation() == new Vector2(0, -1))
                {

                    nextMove = TurnRight;
                }
            }

            else if (GetPosition().x - 1 == GetEnemyPositions()[vihuPosInd].x && GetPosition().y == GetEnemyPositions()[vihuPosInd].y && GetHP() == 1)
            {
                if (GetRotation() == new Vector2(0, 1))
                {

                    nextMove = TurnRight;
                }
                else if (GetRotation() == new Vector2(0, -1))
                {

                    nextMove = TurnLeft;
                }
            }

            else if (GetPosition().y + 1 == GetEnemyPositions()[vihuPosInd].y && GetPosition().x == GetEnemyPositions()[vihuPosInd].x && GetHP() == 1)
            {
                if (GetRotation() == new Vector2(1, 0))
                {

                    nextMove = TurnRight;
                }
                else if (GetRotation() == new Vector2(-1, 0))
                {

                    nextMove = TurnLeft;
                }
            }


            else if (GetPosition().y - 1 == GetEnemyPositions()[vihuPosInd].y && GetPosition().x == GetEnemyPositions()[vihuPosInd].x && GetHP() == 1)
            {
                if (GetRotation() == new Vector2(1, 0))
                {

                    nextMove = TurnLeft;
                }
                else if (GetRotation() == new Vector2(-1, 0))
                {

                    nextMove = TurnRight;
                }
            }



            //Muuten liikutaan eteenpäin
            else
            {
                nextMove = MoveForward;
            }
        }


    }


}