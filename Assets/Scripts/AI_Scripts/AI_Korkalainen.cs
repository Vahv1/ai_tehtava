using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Korkalainen : PlayerControllerInterface
{
	bool evadedLastTurn = false;

	public override void DecideNextMove()
	{
		// Tarkistetaan v�istettiink� edellisell� vuorolla takana olevaa vihollista,
		// ettei j��d� loputtomaan silmukkaan py�rim��n, jos esim. sein�n takana on
		// pys�htynyt vihollinen ja itse on kolmen sein�n ymp�r�im�n�.
		if (evadedLastTurn)
		{
			evadedLastTurn = false;
			// Tyhj�� edess�, liikutaan sinne.
			if (GetForwardTileStatus() == 0) nextMove = MoveForward;
			// Sein�, odotetaan seuraavaa k��ntymist�.
			else if (GetForwardTileStatus() == 1) nextMove = Pass;
			// Vihollinen, ly�d��n.
			else nextMove = Hit;
			return;
		}

		// Lis�ominaisuus: takana tulevan vihollisen v�ist�minen
		// Etsit��n ensimm�inen tarpeeksi l�hell� oleva vihollinen, joka on pelaajan takana.
		// Jos sellainen l�ytyy, k��nnyt��n johonkin suuntaan.
		Vector2[] enemyPositions = GetEnemyPositions();
		Vector2 playerPos = GetPosition();
		Vector2 closestEnemyPos = new Vector2(50, 50);
		foreach (Vector2 enemy in enemyPositions)
		{
			float distance = Vector2.Distance(playerPos, enemy);
			// Jos et�isyys on tarpeeksi pient�, tarkistetaan onko vihollinen pelaajan takana
			if (distance < 4.0)
			{
				closestEnemyPos = enemy;

				// Katsotaan, mihin suuntaan pelaaja on menossa.
				Vector2 playerRotation = GetRotation();
				// Pelaaja menee yl�s, katsotaan onko l�hin vihollinen takana
				if (playerRotation == Vector2.up)
				{
					// Jos pelaajalla ja vihollisella on sama x ja vihollisen y on pienempi kuin
					// pelaajan y, t�ytyy vihollisen siis olla pelaajan takana, jolloin k��nnyt��n.
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
					// Sama kuin ylh��ll�, mutta y menee toisin p�in.
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
					// Sama kuin ylempi, mutta toisin p�in.
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

		// Liikutaan eteenp�in, jos edess� on tyhj� ruutu.
		// Satunnaisesti kuitenkin k��nnyt��n, jotta ei j��d� jumiin silmukkaan.
		if (GetForwardTileStatus() == 0)
		{
			if (Random.Range(0, 10) == 1) Turn();
			else nextMove = MoveForward;
		}
		// Sein� vastassa, k��nnyt��n.
		else if (GetForwardTileStatus() == 1)
		{
			Turn();
		}
		// Pelaaja vastassa, ly�d��n.
		else
		{
			nextMove = Hit;
		}
	}

	// K��nnyt��n satunnaisesti vasemmalle tai oikealle, jotta ei j��d� silmukkaan.
	public void Turn()
	{
		if (Random.Range(0, 2) == 1) nextMove = TurnLeft;
		else nextMove = TurnRight;
	}
}