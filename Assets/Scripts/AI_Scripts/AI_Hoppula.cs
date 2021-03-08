using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Hoppula : PlayerControllerInterface
{
    // TÄMÄ TULEE TEHTÄVÄSSÄ TÄYDENTÄÄ
    // Käytä vain PlayerControllerInterfacessa olevia metodeja TIMissä olevan ohjeistuksen mukaan


    private Vector2 nearestEnemy = Vector2.positiveInfinity;
    private Vector2 playerPosition;
    private Vector2 playerRotation;
    private bool weDidTurn;
    private bool weDidMove;
    private bool weDidAttack;
    private bool lastTurnFacingWall;
    private bool lastTurnTurnedClockwise;
    private int consecutiveTurns;

    // This holds the grid of level. Not used for anything.
    private int[,] grid = new int[40, 40];

    public override void DecideNextMove()
    {
        // Update our knowledge of the world
        UpdateEnemiesAndGrid();
        playerPosition = GetPosition();
        playerRotation = GetRotation();

        // directionDegrees is the direction to nearest enemy in degrees. Value is between 0-180 and 0-(-180)
        // The game seems to give old/wrong information about player's current position.
        // This should give correct results.
        float directionDegrees = 0;
        if (weDidMove)
        {
            directionDegrees = Mathf.Atan2((nearestEnemy - playerPosition + playerRotation).y, (nearestEnemy - playerPosition + playerRotation).x) * Mathf.Rad2Deg;
        }
        else
        {
            directionDegrees = Mathf.Atan2((nearestEnemy - playerPosition).y, (nearestEnemy - playerPosition).x) * Mathf.Rad2Deg;
        }

        weDidTurn = false;
        weDidMove = false;
        weDidAttack = false;

        // The if-hell brain of the bot. 
        switch (GetForwardTileStatus())
        {
            case 2:
                // There is enemy ahead, let's smack it in the head.
                Hit();
                lastTurnFacingWall = false;
                weDidAttack = true;
                break;

            case 0:
                // There is open space ahead. We will either turn or move forward.

                if (lastTurnFacingWall)
                {
                    // We were against a wall last turn, so let's go forward at least one step.
                    // Avoids getting stuck in one square turning back and forth.
                    // Move step is taken at the end.
                    // We can set lastTurnFacingWall to false because we are not facing a wall.
                    lastTurnFacingWall = false;
                    break;
                }

                // The following if-hell causes the bot to follow nearest enemy.
                // We simply turn towards the nearest enemy, and move forward when there is no need to turn.
                else if (directionDegrees <= 45 && directionDegrees >= -45)
                {
                    // Enemy is on right side
                    if (playerRotation.x < 0.5f)
                    { // We are facing left
                        if (directionDegrees >= 0)
                        { // Enemy is above, turn right
                            TurnRight();
                            weDidTurn = true;
                        }
                        else if (directionDegrees < 0)
                        { // Enemy is below, turn left
                            TurnLeft();
                            weDidTurn = true;
                        }
                    }
                    else if (playerRotation.y > 0.5f)
                    { // We are facing up, turn right
                        TurnRight();
                        weDidTurn = true;
                    }
                    else if (playerRotation.y < -0.5f)
                    { // We are facing down, turn left
                        TurnLeft();
                        weDidTurn = true;
                    }
                }

                else if (directionDegrees <= 135 && directionDegrees >= 45)
                { // Enemy is above
                    if (playerRotation.y < -0.5f)
                    { // We are facing down
                        if (directionDegrees >= 90)
                        { // Enemy is on left side, so turn towards it
                            TurnRight();
                            weDidTurn = true;
                        }
                        else if (directionDegrees < 90)
                        { // Enemy is on right, so turn towards it
                            TurnLeft();
                            weDidTurn = true;
                        }
                    }
                    else if (playerRotation.x < -0.5f)
                    { // We are facing left, so turn right
                        TurnRight();
                        weDidTurn = true;
                    }
                    else if (playerRotation.x > 0.5f)
                    { // We are facing right, so turn left
                        TurnLeft();
                        weDidTurn = true;
                    }
                }

                else if (directionDegrees >= -135 && directionDegrees <= -45)
                { // Enemy is below
                    if (playerRotation.y > 0.5f)
                    { // We are facing up

                        if (directionDegrees <= -90)
                        { // Enemy is on left side, turn left
                            TurnLeft();
                            weDidTurn = true;
                        }
                        else if (directionDegrees > -90)
                        { // Enemy is on right side, turn right
                            TurnRight();
                            weDidTurn = true;
                        }
                    }
                    else if (playerRotation.x > 0.5f)
                    { // We are facing right, so turn right
                        TurnRight();
                        weDidTurn = true;
                    }
                    else if (playerRotation.x < -0.5f)
                    { // We are facing left, so turn left
                        TurnLeft();
                        weDidTurn = true;
                    }
                }

                else
                { // Enemy is on left side
                    if (playerRotation.x > 0.5f)
                    { // We are facing right
                        if (directionDegrees <= 180)
                        { // Enemy is above, turn left
                            TurnLeft();
                            weDidTurn = true;
                        }
                        else if (directionDegrees > -180)
                        { // Enemy is below, turn right
                            TurnRight();
                            weDidTurn = true;
                        }
                    }
                    else if (playerRotation.y > 0.5f)
                    { // We are facing up, turn left
                        TurnLeft();
                        weDidTurn = true;
                    }
                    else if (playerRotation.y < -0.5f)
                    { // We are facing down,  turn right
                        TurnRight();
                        weDidTurn = true;
                    }
                }

                // We can set lastTurnFacingWall to false because we are not facing a wall.
                lastTurnFacingWall = false;
                consecutiveTurns = 0;
                break;

            case 1:
                // There is a wall ahead

                if (lastTurnFacingWall)
                { // We faced a wall last turn as well, so let's keep turning the same way we did.
                    if (lastTurnTurnedClockwise)
                    {
                        if (consecutiveTurns < 4)
                        {
                            TurnRight();
                            weDidTurn = true;
                            consecutiveTurns++;
                        }
                        else
                        {
                            // We have been making same turns 4 times, let's change direction.
                            TurnLeft();
                            weDidTurn = true;
                            consecutiveTurns = 0;
                            lastTurnTurnedClockwise = false;
                        }
                        lastTurnFacingWall = true;
                    }
                    else
                    {
                        if (consecutiveTurns < 4)
                        {
                            TurnLeft();
                            weDidTurn = true;
                            consecutiveTurns++;
                            lastTurnFacingWall = true;
                        }
                        else
                        {
                            // We have been making same turns 4 times, let's change direction.
                            TurnRight();
                            weDidTurn = true;
                            consecutiveTurns = 0;
                            lastTurnTurnedClockwise = true;
                        }
                    }
                }
                // The bot is against a wall.
                // The following if-hell causes the bot to turn towards nearest enemy.
                else if (playerRotation.x > 0.5f)
                { // We are facing right
                    if (directionDegrees > 0)
                    { // Enemy is above
                        TurnLeft();
                        lastTurnTurnedClockwise = false;
                        weDidTurn = true;
                    }
                    else
                    { // Enemy is below
                        TurnRight();
                        lastTurnTurnedClockwise = true;
                        weDidTurn = true;
                    }
                }
                else if (playerRotation.x < -0.5f)
                { // We are facing left
                    if (directionDegrees < 180 && directionDegrees > 0)
                    { // Enemy is above
                        TurnRight();
                        lastTurnTurnedClockwise = true;
                        weDidTurn = true;
                    }
                    else
                    { // Enemy is below
                        TurnLeft();
                        lastTurnTurnedClockwise = false;
                        weDidTurn = true;
                    }
                }
                else if (playerRotation.y > 0.5f)
                { // We are facing up
                    if (directionDegrees < 90 && directionDegrees > -90)
                    { // Enemy is on right side
                        TurnRight();
                        lastTurnTurnedClockwise = true;
                        weDidTurn = true;
                    }
                    else
                    { // Enemy is on left side
                        TurnLeft();
                        lastTurnTurnedClockwise = false;
                        weDidTurn = true;
                    }
                }
                else
                { // We are facing down
                    if (directionDegrees > -90 && directionDegrees < 90)
                    { // Enemy is on right side
                        TurnLeft();
                        lastTurnTurnedClockwise = false;
                        weDidTurn = true;
                    }
                    else
                    { // Enemy is on left side
                        TurnRight();
                        lastTurnTurnedClockwise = true;
                        weDidTurn = true;
                    }
                }

                // We were against a wall, so let's remember that for the next turn
                lastTurnFacingWall = true;
                break;
        }

        // Check if we have taken an action this turn. If not, take a step.
        if (!weDidTurn && !weDidMove && !weDidAttack)
        {
            bool shouldMove = true;

            // The following if-hell prevents the bot from stepping right in front of an enemy.
            if (playerRotation.x > 0.5f || playerRotation.x < -0.5f)
            { // Player is facing left or right
                if (GetEnemyRotation(playerPosition + playerRotation + new Vector2(0, 1)).y < -0.5f)
                { // There is an enemy one square left/right and up, and it's facing down
                  // We should skip turn
                    shouldMove = false;
                }
                else if (GetEnemyRotation(playerPosition + playerRotation + new Vector2(0, -1)).y > 0.5f)
                { // There is an enemy one square left/right and down, and it's facing up
                  // We should skip turn
                    shouldMove = false;
                }
            }
            else if (playerRotation.y > 0.5f || playerRotation.y < -0.5f)
            { // Player is up or down
                if (GetEnemyRotation(playerPosition + playerRotation + new Vector2(1, 0)).x < -0.5f)
                { // There is an enemy one square up/down and right, and it's facing left
                  // We should skip turn
                    shouldMove = false;
                }
                else if (GetEnemyRotation(playerPosition + playerRotation + new Vector2(-1, 0)).x > 0.5f)
                { // There is an enemy one square up/down and left, and it's facing right
                  // We should skip turn
                    shouldMove = false;
                }
            }

            if (shouldMove)
            {
                MoveForward();
                weDidMove = true;
            }
        }

        // Skip the turn if we didn't do anything
        if (!weDidTurn && !weDidMove && !weDidAttack)
        {
            Pass();
        }
    }


    // Updates enemy related data and creates map the world
    private void UpdateEnemiesAndGrid()
    {
        nearestEnemy = Vector2.positiveInfinity;
        float distanceToNearestEnemy = Vector2.Distance(playerPosition, nearestEnemy);

        // Go through every enemy's position and find the nearest one.
        // Also maps the world. Over time the bot learns the map, but I ended up not
        // using this feature for anything.
        Vector2[] enemyPositions = GetEnemyPositions();
        foreach (Vector2 enemyPos in enemyPositions)
        {
            grid[(int)(enemyPos.x + 19.5f), (int)(enemyPos.y + 19.5f)] = 1;
            float distanceToPlayer = Vector2.Distance(enemyPos, playerPosition);

            if (distanceToNearestEnemy > distanceToPlayer)
            {
                nearestEnemy = enemyPos;
                distanceToNearestEnemy = distanceToPlayer;
            }
        }

        grid[(int)(playerPosition.x + 19.5f), (int)(playerPosition.y + 19.5f)] = 1;
    }
}