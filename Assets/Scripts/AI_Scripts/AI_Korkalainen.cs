using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Korkalainen : PlayerControllerInterface
{
	bool evadedLastTurn = false;

	public override void DecideNextMove()
	{
		// Tarkistetaan väistettiinkö edellisellä vuorolla takana olevaa vihollista,
		// ettei jäädä loputtomaan silmukkaan pyörimään, jos esim. seinän takana on
		// pysähtynyt vihollinen ja itse on kolmen seinän ympäröimänä.
		if (evadedLastTurn)
		{
			evadedLastTurn = false;
			// Tyhjää edessä, liikutaan sinne.
			if (GetForwardTileStatus() == 0) nextMove = MoveForward;
			// Seinä, odotetaan seuraavaa kääntymistä.
			else if (GetForwardTileStatus() == 1) nextMove = Pass;
			// Vihollinen, lyödään.
			else nextMove = Hit;
			return;
		}

		// Lisäominaisuus: takana tulevan vihollisen väistäminen
		// Etsitään ensimmäinen tarpeeksi lähellä oleva vihollinen, joka on pelaajan takana.
		// Jos sellainen löytyy, käännytään johonkin suuntaan.
		Vector2[] enemyPositions = GetEnemyPositions();
		Vector2 playerPos = GetPosition();
		Vector2 closestEnemyPos = new Vector2(50, 50);
		foreach (Vector2 enemy in enemyPositions)
		{
			float distance = Vector2.Distance(playerPos, enemy);
			// Jos etäisyys on tarpeeksi pientä, tarkistetaan onko vihollinen pelaajan takana
			if (distance < 4.0)
			{
				closestEnemyPos = enemy;

				// Katsotaan, mihin suuntaan pelaaja on menossa.
				Vector2 playerRotation = GetRotation();
				// Pelaaja menee ylös, katsotaan onko lähin vihollinen takana
				if (playerRotation == Vector2.up)
				{
					// Jos pelaajalla ja vihollisella on sama x ja vihollisen y on pienempi kuin
					// pelaajan y, täytyy vihollisen siis olla pelaajan takana, jolloin käännytään.
					if (closestEnemyPos.x == playerPos.x && closestEnemyPos.y < playerPos.y)
					{
						Turn();
						evadedLastTurn = true;
						return;
					}
				}
				// Pelaaja menee alas
				else if (playerRotation == Vector2.down)
				{
					// Sama kuin ylhäällä, mutta y menee toisin päin.
					if (closestEnemyPos.x == playerPos.x && playerPos.y < closestEnemyPos.y)
					{
						Turn();
						evadedLastTurn = true;
						return;
					}
				}
				// Pelaaja menee vasemmalle
				else if (playerRotation == Vector2.left)
				{
					// Jos y:t on samat, ja pelaajan x on pienempi kuin vihollisen, vihollinen on takana.
					if (closestEnemyPos.y == playerPos.y && playerPos.x < closestEnemyPos.x)
					{
						Turn();
						evadedLastTurn = true;
						return;
					}
				}
				// Pelaaja menee oikealle
				else
				{
					// Sama kuin ylempi, mutta toisin päin.
					if (closestEnemyPos.y == playerPos.y && closestEnemyPos.x < playerPos.x)
					{
						Turn();
						evadedLastTurn = true;
						return;
					}
				}
			}
		}

		// "Normaali" liikkuminen:

		// Liikutaan eteenpäin, jos edessä on tyhjä ruutu.
		// Satunnaisesti kuitenkin käännytään, jotta ei jäädä jumiin silmukkaan.
		if (GetForwardTileStatus() == 0)
		{
			if (Random.Range(0, 10) == 1) Turn();
			else nextMove = MoveForward;
		}
		// Seinä vastassa, käännytään.
		else if (GetForwardTileStatus() == 1)
		{
			Turn();
		}
		// Pelaaja vastassa, lyödään.
		else
		{
			nextMove = Hit;
		}
	}

	// Käännytään satunnaisesti vasemmalle tai oikealle, jotta ei jäädä silmukkaan.
	public void Turn()
	{
		if (Random.Range(0, 2) == 1) nextMove = TurnLeft;
		else nextMove = TurnRight;
	}
}