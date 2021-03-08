using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Laitinen : PlayerControllerInterface
{

    // T�M� TULEE TEHT�V�SS� T�YDENT��
    // K�yt� vain PlayerControllerInterfacessa olevia metodeja TIMiss� olevan ohjeistuksen mukaan
    public override void DecideNextMove()
    {
        // Tyhm� teko�ly, liikkuu eteenp�in jos edess� on tyhj� ruutu

        int kohde = 0;
        Vector2 position;
        kohde = GetForwardTileStatus();
        goToEnemy();
        //if (GetForwardTileStatus() == 0)
        //{
        //    nextMove = MoveForward;    
        //}
        //else 
        //{
        //    nextMove = TurnLeft;
        //}

        //Funktio hakee vihollisten positiot ja etsii l�himm�n niist�
        void goToEnemy()
        {
            position = GetPosition();
            Vector2 pahis;
            float x = 0f;
            float y = 0f;
            int c;
            int[] distance = new int[GetEnemyPositions().Length];


            //Otetaan talteen kaikki linnunreitti� menev�t et�isyydet
            for (int i = 0; i < GetEnemyPositions().Length; i++)
            {
                pahis = GetEnemyPositions()[i];
                x = pahis.x;
                y = pahis.y;

                //Lasketaan hypotenuusa
                c = (int)Mathf.Round(Mathf.Sqrt(x * x + y * y));

                distance[i] = c;
            }

            //Otetaan lyhin matka 
            var lyhin = Mathf.Min(distance);


            //Otetaan l�himm�n vihollisen koordinaatit
            for (int i = 0; i < distance.Length; i++)
            {
                if (distance[i] == distance[lyhin])
                {
                    var vihu = GetEnemyPositions()[i];
                    x = vihu.x;
                    y = vihu.y;
                    break;
                }
            }

            //Valitettavasti en ehtinyt tehd� loppuun niin l�himm�n vihollisen etsiv� teko�ly j�� kesken.
            //Siit� olikin tulossa melkonen if-sotku

            //print("Pelaaja :" + position);
            //print("Vihollinen " + "(" + x + ", " + y + ")");
            // switch (kohde)
            // {
            //     case 0:
            //         if ((position.x < x) && GetRotation().x != 1)
            //         {
            //             nextMove = TurnRight;
            //             break;
            //         }
            //         if (position.x < x)
            //         {
            //             nextMove = MoveForward;
            //             break;
            //         }
            //         if ((position.x > x) && GetRotation().x != -1)
            //         {
            //             nextMove = TurnLeft;
            //             break;
            //         }
            //         if ((position.x > x))
            //         {
            //             nextMove = MoveForward;
            //             break;
            //         }
            //         if((position.y < y) && GetRotation().y != 1 && GetRotation().y == -1)
            //         {
            //             nextMove = TurnRight;
            //         }
            //         break;
            //     case 1:
            //         //nextMove = TurnLeft;
            //         break;
            //     case 2:
            //         Hit();
            //         break;
            //}

            //Switch jolla tarkistetaan edess� oleva ruutu.
            //Ruutu on tyhj� 0 = liiku eteenp�in
            //Ruudussa on sein� 1 = K��nny vasemmalle
            //Ruudussa vihollinen 2 = Ly� vihollista
            //Toista
            //J�� jumiin looppeihin




            switch (kohde)
            {
                case 0:
                    nextMove = MoveForward;
                    break;
                case 1:
                    var rand = Random.Range(-2, 2);
                    if (rand < 0)
                        nextMove = TurnLeft;
                    else nextMove = TurnRight;
                    break;
                case 2:
                    Hit();
                    break;
            }


        }
    }
}