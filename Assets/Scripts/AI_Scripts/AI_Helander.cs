using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * My bot is focused on evading hostiles and trying to find a corner where to stalk enemy bots
 */
public class AI_Helander : PlayerControllerInterface
{
    private bool corner = false; //have we found a corner to stalk in
    private HashSet<Vector2> turnPoints = new HashSet<Vector2>(); //Every coordinate where we have turned while NOT in danger
    private HashSet<Vector2> previousStuckPoints = new HashSet<Vector2>(); //All the coordinates where we have turned more than once while NOT in danger
    private bool lastMoveWasTurn = false; //Whether last move was turn while not in danger
    private bool thereWasAWallToo = false; //If there was a wall after turning again due to a wall while not in danger
    private int fourTurns = 0; //Records if we have turned 4 times in a row while not in danger
    private bool ranToWall = false; //if we ran to a wall while being chased

    /*
     * The movement and other actions of bot
     * This one focuses on the part where we are not in danger
     */
    public override void DecideNextMove()
    {
        if (!corner) //if we aren't safely stalking in a corner
        {
            bool danger = false;
            var closeEnemies = EnemiesClose();
            if (closeEnemies.Count > 0)//if there are enemies close go to the danger handler
            {
                danger = DangerHandler(closeEnemies);
            }
            if (!danger) //if we aren't in danger we try to find a corner
            {
                if (fourTurns > 3) //if we made 4 turns we...
                {
                    if (GetForwardTileStatus() == 1) //found a corner or...
                    {
                        corner = true;
                        nextMove = TurnRight;
                        return;
                    }
                    else //we didn't, move on.
                    {
                        nextMove = MoveForward;
                        fourTurns = 0;
                        thereWasAWallToo = false;
                        lastMoveWasTurn = false;
                        return;
                    }
                }
                if (lastMoveWasTurn)
                {
                    if (GetForwardTileStatus() == 1) //if we turned and there's a new wall we may have found a corner
                    {
                        thereWasAWallToo = true;
                        fourTurns = 2;
                    }
                }
                else if (GetForwardTileStatus() == 1) //if last move was not a turn this is just a one of wall
                {
                    int hereAgain = checkIfWeHaveBeenHere();
                    if (hereAgain == 1) //if we have been here once turn right instead of left
                    {
                        nextMove = TurnRight;
                    }
                    else nextMove = TurnLeft; //otherwise we are here for the first time or turning right didn't solve us being stuck
                    lastMoveWasTurn = true;
                    return;
                }
                if (thereWasAWallToo) //make an extra turn to see if we are maybe in corner
                {
                    nextMove = TurnLeft;
                    lastMoveWasTurn = true;
                    fourTurns++;
                    return;
                }
                else if (GetForwardTileStatus() == 0)//move forwards if there is no wall in front of us.
                {
                    nextMove = MoveForward;
                    lastMoveWasTurn = false;
                }
            }
        }
        else //if we are in a corner we'll just stay here stalking for the rest of the game 
        {
            if (GetForwardTileStatus() == 2) nextMove = Hit;
            else nextMove = Pass;
        }
    }

    /*
     * Sorry for the very complicated if statements but there may be a bug in the program that causes 
     * the positions to change with 0.0001 sometimes so even though we seem to be in the same spot we are not
     * Checks if we have been here before
     * returns 0 if never 1 if once and 2 if more than 1 time
     */
    public int checkIfWeHaveBeenHere()
    {
        foreach (Vector2 coordinate in previousStuckPoints) //if we have been here many times
        {
            if (Mathf.Round(Mathf.Abs(GetPosition().x) - Mathf.Abs((coordinate.x))) == 0 && Mathf.Round(Mathf.Abs(GetPosition().y) - Mathf.Abs(coordinate.y)) == 0)
            {
                return 2;
            }
        }
        foreach (Vector2 coordinate in turnPoints) //check if this is the first time here
        {
            if (Mathf.Round(Mathf.Abs(GetPosition().x) - Mathf.Abs((coordinate.x))) == 0 && Mathf.Round(Mathf.Abs(GetPosition().y) - Mathf.Abs(coordinate.y)) == 0)
            {
                previousStuckPoints.Add(GetPosition()); //if it is add it to coordinates of places where we have turned before
                return 1;
            }
        }
        turnPoints.Add(GetPosition()); //add current coordinate to spots we've been to before
        return 0;
    }
    /*
     * Sorry for the repetition couldn't come up with a better way :(
     * If there are enemies within a 7x7 field of us what do we do.
     * enemies = coordinates of close by enemies.
     * Decides our next move if we are in danger and returns true. Otherwise returns false and let's decide next move continue normally
     * We are in danger if enemies behind us are facing the same way as we. OR
     * Enemies in front of us are moving to our path
     */
    private bool DangerHandler(HashSet<Vector2> enemies)
    {
        thereWasAWallToo = false; //we can't be searching for corners if we are in danger
        float closest = 50; //distance to closest threat. Since the real maximum is 6 this can be used as a substitute of no dangers close
        Vector2 me = GetPosition();
        Dictionary<float, Vector2> threatLevel = new Dictionary<float, Vector2>(); //Dictionary with a float that means the distance with a modifier meaning threat level, Vector is the enemy's coordinate
        if (GetForwardTileStatus() == 2) //if there is an enemy directly in front of us hit
        {
            nextMove = Hit;
            return true;
        }
        Vector2 myRotation = GetRotation();
        if (myRotation.x == 1) //if we are going right our threats are:
        {
            foreach (Vector2 enemy in enemies)
            {
                if (me.x - enemy.x > 0)//enemies chasing us
                {
                    if (myRotation.x == GetEnemyRotation(enemy).x)
                    {
                        float dist = Mathf.Abs(me.x - enemy.x) + Mathf.Abs(me.y - enemy.y);
                        if (threatLevel.ContainsKey(dist)) continue;
                        threatLevel.Add(dist, enemy);
                    }
                }
                if (me.x - enemy.x < 0)
                {
                    if (me.y - enemy.y < 0)//those that are in front of us and going down
                    {
                        if (GetEnemyRotation(enemy).y == -1)
                        {
                            float dist = Mathf.Abs(me.x - enemy.x) + Mathf.Abs(me.y - enemy.y) - 0.1f;
                            if (threatLevel.ContainsKey(dist)) continue;
                            threatLevel.Add(dist, enemy);
                        }

                    }

                    if (me.y - enemy.y > 0)
                    {
                        if (GetEnemyRotation(enemy).y == 1) //or up
                        {
                            float dist = Mathf.Abs(me.x - enemy.x) + Mathf.Abs(me.y - enemy.y) - 0.05f;
                            if (threatLevel.ContainsKey(dist)) continue;
                            threatLevel.Add(dist, enemy);
                        }
                    }
                }
            }
            foreach (float key in threatLevel.Keys) //find the smallest key because they are closest to us and thus the biggest threat
            {
                if (key < closest)
                {
                    closest = key;
                }
            }
            if (closest == 50) return false; //if there was no keys basically we are not in danger
            else
            {
                Vector2 enemyRot;
                try
                {
                    enemyRot = GetEnemyRotation(threatLevel[closest]); //try to get the closest enemy's rotation and later act on it
                }
                catch (KeyNotFoundException) //if something went wrong we'll just continue normally as if we aren't in danger
                {
                    return false;
                }
                if (myRotation.x == enemyRot.x) //if the enemy is facing the same way as us and a threat they are chasing us
                {
                    return BeingChased();

                }
                else if (enemyRot.y == -1) //The enemy is going downwards turn right. Now it's a chase scenario and can be handled with the being chased function
                {
                    nextMove = TurnRight;
                }
                else nextMove = TurnLeft; //otherwise the enemy is going up wards 
                return true;
            }
        }
        if (myRotation.x == -1) //Basically the same as the x == 1 but different directions Agan sorry for repetition
        {
            foreach (Vector2 enemy in enemies)
            {
                if (me.x - enemy.x < 0)
                {
                    if (myRotation.x == GetEnemyRotation(enemy).x)
                    {
                        float dist = Mathf.Abs(me.x - enemy.x) + Mathf.Abs(me.y - enemy.y);
                        if (threatLevel.ContainsKey(dist)) continue;
                        threatLevel.Add(dist, enemy);
                    }
                }
                if (me.x - enemy.x > 0)
                {
                    if (me.y - enemy.y < 0)
                    {
                        if (GetEnemyRotation(enemy).y == -1)
                        {
                            float dist = Mathf.Abs(me.x - enemy.x) + Mathf.Abs(me.y - enemy.y) - 0.1f;
                            if (threatLevel.ContainsKey(dist)) continue;
                            threatLevel.Add(dist, enemy);
                        }
                    }

                    if (me.y - enemy.y > 0)
                    {
                        if (GetEnemyRotation(enemy).y == 1)
                        {
                            float dist = Mathf.Abs(me.x - enemy.x) + Mathf.Abs(me.y - enemy.y) - 0.05f;
                            if (threatLevel.ContainsKey(dist)) continue;
                            threatLevel.Add(dist, enemy);
                        }
                    }
                }
            }
            foreach (float key in threatLevel.Keys)
            {
                if (key < closest)
                {
                    closest = key;
                }
            }
            if (closest == 50) return false;
            else
            {
                Vector2 enemyRot;
                try
                {
                    enemyRot = GetEnemyRotation(threatLevel[closest]);
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
                if (myRotation.x == enemyRot.x)
                {
                    return BeingChased();
                }
                else if (enemyRot.y == -1)
                {
                    nextMove = TurnLeft;
                }
                else nextMove = TurnRight;
                return true;
            }
        }
        if (myRotation.y > 0.5) //same as x cases but different directions. Had to put y>0.5 cause == just didn't work for some reason
        {
            foreach (Vector2 enemy in enemies)
            {
                if (me.y - enemy.y > 0)
                {
                    if (myRotation.y == GetEnemyRotation(enemy).y)
                    {
                        float dist = Mathf.Abs(me.x - enemy.x) + Mathf.Abs(me.y - enemy.y);
                        if (threatLevel.ContainsKey(dist)) continue;
                        threatLevel.Add(dist, enemy);
                    }
                }
                if (me.y - enemy.y < 0)
                {
                    if (me.x - enemy.x < 0)
                    {
                        if (GetEnemyRotation(enemy).x == -1)
                        {
                            float dist = Mathf.Abs(me.x - enemy.x) + Mathf.Abs(me.y - enemy.y) - 0.1f;
                            if (threatLevel.ContainsKey(dist)) continue;
                            threatLevel.Add(dist, enemy);
                        }
                    }

                    if (me.x - enemy.x > 0)
                    {
                        if (GetEnemyRotation(enemy).x == 1)
                        {
                            float dist = Mathf.Abs(me.x - enemy.x) + Mathf.Abs(me.y - enemy.y) - 0.05f;
                            if (threatLevel.ContainsKey(dist)) continue;
                            threatLevel.Add(dist, enemy);
                        }
                    }
                }
            }
            foreach (float key in threatLevel.Keys)
            {
                if (key < closest)
                {
                    closest = key;
                }
            }
            if (closest == 50) return false;
            else
            {
                Vector2 enemyRot;
                try
                {
                    enemyRot = GetEnemyRotation(threatLevel[closest]);
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
                if (myRotation.y == enemyRot.y)
                {
                    return BeingChased();
                }
                else if (enemyRot.x == -1)
                {
                    nextMove = TurnLeft;
                }
                else
                    nextMove = TurnRight;
                return true;
            }
        }
        if (myRotation.y < -0.5) //last sorry for repetition
        {
            foreach (Vector2 enemy in enemies)
            {
                if (me.y - enemy.y < 0)
                {
                    if (myRotation.y == GetEnemyRotation(enemy).y)
                    {
                        float dist = Mathf.Abs(me.x - enemy.x) + Mathf.Abs(me.y - enemy.y);
                        if (threatLevel.ContainsKey(dist)) continue;
                        threatLevel.Add(dist, enemy);
                    }
                }
                if (me.y - enemy.y > 0)
                {
                    if (me.x - enemy.x < 0)
                    {
                        if (GetEnemyRotation(enemy).x == -1)
                        {
                            float dist = Mathf.Abs(me.x - enemy.x) + Mathf.Abs(me.y - enemy.y) - 0.1f;
                            if (threatLevel.ContainsKey(dist)) continue;
                            threatLevel.Add(dist, enemy);
                        }
                    }

                    if (me.x - enemy.x > 0)
                    {
                        if (GetEnemyRotation(enemy).x == 1)
                        {
                            float dist = Mathf.Abs(me.x - enemy.x) + Mathf.Abs(me.y - enemy.y) - 0.05f;
                            if (threatLevel.ContainsKey(dist)) continue;
                            threatLevel.Add(dist, enemy);
                        }
                    }
                }
            }
            foreach (float key in threatLevel.Keys)
            {
                if (key < closest)
                {
                    closest = key;
                }
            }
            if (closest == 50) return false;
            else
            {
                Vector2 enemyRot;
                try
                {
                    enemyRot = GetEnemyRotation(threatLevel[closest]);
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
                if (myRotation.y == enemyRot.y)
                {
                    return BeingChased();
                }
                else if (enemyRot.y == -1)
                {
                    nextMove = TurnRight;
                }
                else nextMove = TurnLeft;
                return true;
            }
        }
        return false; //if we reached this point we are not in danger 

    }

    /*
     * Gets all enemies that are in a 7x7 field from us.
     */
    private HashSet<Vector2> EnemiesClose()
    {
        Vector2[] enemies = GetEnemyPositions();
        Vector2 me = GetPosition();
        HashSet<Vector2> closeEnemies = new HashSet<Vector2>();
        foreach (Vector2 enemyPos in enemies) //sort out all enemies whose distance from us on both x and y axis is 3 or less
        {
            if (Mathf.Round(Mathf.Abs(me.x - enemyPos.x)) <= 3 && Mathf.Round(Mathf.Abs(me.y - enemyPos.y)) <= 3)
            {
                closeEnemies.Add(enemyPos);
            }
        }
        return closeEnemies;
    }
    /*
     * the moves we take if we are being chased.
     * returns true because we are always in danger while being chased.
     */
    private bool BeingChased()
    {
        if (ranToWall)//if we ran to a wall while being chased
        {
            if (GetForwardTileStatus() == 0) //we simply move forwards after a turn
            {
                nextMove = MoveForward;
                ranToWall = false;
                return true;
            }
            else //There is another wall turn again and take the chaser head on. (has to be wall if it was enemy we would've hit)
            {
                nextMove = TurnLeft;
                ranToWall = false;
                return true;
            }
        }
        if (GetForwardTileStatus() == 0)//run away if we can
        {
            nextMove = MoveForward;
        }
        else //we ran to a wall turn left
        {
            nextMove = TurnLeft;
            ranToWall = true;
        }
        return true;
    }
}