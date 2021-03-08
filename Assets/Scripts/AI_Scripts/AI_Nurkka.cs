using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AI_Nurkka : PlayerControllerInterface
{
    const int EMPTY = 0, WALL = 1, ENEMY = 2;
    struct WeightedMove : IComparable<WeightedMove>
    {
        public int weight;
        public NextMove move;

        public WeightedMove(int weight, NextMove nextMove)
        {
            this.weight = weight;
            this.move = nextMove;
        }

        public int CompareTo(WeightedMove other)
        {
            // J‰rjestet‰‰n arvot painoarvon mukaan painavimmasta v‰hiten painavaan
            // ja saman painoarvon alkiot satunnaisesti kesken‰‰n
            return (other.weight * 10 - this.weight * 10) + UnityEngine.Random.Range(-15, 15);
        }
    }

    private List<WeightedMove> moves;
    private List<Vector2> visited;
    private List<Vector2> walls;

    private void Awake()
    {
        moves = new List<WeightedMove>();
        visited = new List<Vector2>();
        walls = new List<Vector2>();
    }

    private void AddMove(int weight, NextMove move)
    {
        moves.Add(new WeightedMove(weight, move));
    }

    private void ChooseAction()
    {
        bool result = moves.Any();
        moves.Sort();
        nextMove = !moves.Any() ? Pass : moves[0].move;
        moves.Clear();
    }

    public override void DecideNextMove()
    {
        // Muistetaan k‰ydyt ruudut
        visited.Add(GetPosition());

        // Varotaan
        foreach (Vector2 enemy in GetEnemyPositions())
        {
            if (enemy + GetEnemyRotation(enemy) == GetPosition())
            {
                // Jokin botti voi hyˆk‰t‰! Tehd‰‰n asialle jotain
                switch (GetForwardTileStatus())
                {
                    case EMPTY:
                        // L‰hdet‰‰n karkuun jos voidaan
                        AddMove(5, MoveForward);
                        break;
                    case WALL:
                        Vector2 enemyDirection = GetPosition() - enemy;

                        // Vihollinen voi hyˆk‰t‰ takaata tai vierest‰, k‰‰nnyt‰‰n satunnaiseen suuntaan
                        AddMove(5, TurnRight);
                        AddMove(5, TurnLeft);
                        break;
                    case ENEMY:
                        // Osoitetaan mahdooliseen hyˆkk‰‰j‰‰n p‰in, ei auta muu kuin lyˆd‰
                        AddMove(20, Hit);
                        break;
                }
            }
        }

        // K‰‰nnyt‰‰n jos ei olla jo k‰‰ntyneen‰ l‰himm‰n vihollisen suuntaan
        var nearest = GetEnemyPositions()[0];
        var distance = 1000f;
        foreach (Vector2 enemy in GetEnemyPositions())
        {
            float d = Vector2.Distance(GetPosition(), enemy);
            if (d < distance)
            {
                nearest = enemy;
                distance = d;
            }
        }

        var nearestDir = (nearest - GetPosition()).normalized;
        if (Math.Abs(nearestDir.x) > Math.Abs(nearestDir.y))
        {
            nearestDir.y = 0;
            if (nearestDir.x < 0) nearestDir.x = -1;
            else nearestDir.x = 1;
        }
        if (Math.Abs(nearestDir.y) > Math.Abs(nearestDir.x))
        {
            nearestDir.x = 0;
            if (nearestDir.y < 0) nearestDir.y = -1;
            else nearestDir.y = 1;
        }

        if (GetRotation() != nearestDir)
        {
            AddMove(3, TurnRight);
            AddMove(4, TurnLeft);
        }

        // Liikutaan
        var v = GetPosition() + GetRotation();
        switch (GetForwardTileStatus())
        {
            case WALL:
                if (!walls.Contains(v))
                    walls.Add(v);
                AddMove(3, TurnRight);
                AddMove(4, TurnLeft);
                break;
            case EMPTY:
                if (visited.Contains(v))
                    AddMove(1, MoveForward); // Ruudussa on jo k‰yty, ‰l‰ liiku ellei ole pakko
                else
                    AddMove(3, MoveForward);
                break;
            case ENEMY:
                AddMove(4, Hit);
                break;
        }

        // Erikoistapaus: j‰‰d‰‰n paikalleen jos sein‰t ymp‰rˆiv‰t tarpeeksi hyvin
        var nearWalls = walls
            .Where(wall => Vector2.Distance(GetPosition(), wall) <= 1.5)
            .ToList();
        switch (nearWalls.Count())
        {
            case 2:
                if (Vector2.Distance(nearWalls[0], nearWalls[1]) <= 1.5 && GetForwardTileStatus() == EMPTY)
                    AddMove(6, Pass);
                break;
            case 3:
                // Jos p‰‰st‰‰n t‰nne ollaan ehk‰ k‰yt‰nnˆss‰ voitettu jo
                AddMove(5, Pass);
                break;
        }

        // P‰‰tet‰‰n
        ChooseAction();
    }
}