using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Niko Kahilainen
/// </summary>
public class AI_Kahilainen : PlayerControllerInterface
{
    Node[,] grid = new Node[40, 40]; // The game grid, or the memory of our AI
    Node currentNode; // Our player Node
    Node targetNode; // Our current target Node
    List<Node> currentPath; // Our current set path
    bool firstRun = true; // LOADSASPAGHETTI

    // TÄMÄ TULEE TEHTÄVÄSSÄ TÄYDENTÄÄ
    // Käytä vain PlayerControllerInterfacessa olevia metodeja TIMissä olevan ohjeistuksen mukaan
    public override void DecideNextMove()
    {
        if (firstRun) FirstRun();

        // Just something to hopefully circumvent the weird bugs I'm experiencing
        if (GetEnemyPositions().Length == 0)
        {
            nextMove = Pass;
            return;
        }
        // if we're facing an enemy, just punch them. No need to calculate a new path or anything.
        if (GetForwardTileStatus() == 2)
        {
            nextMove = Hit;
            return;
        }

        // Update my current location    
        currentNode.gridPosition = GamePositionToGridPosition(GetPosition());
        ChooseClosestTarget();
        // Find the fastest path
        aStarPathFinding();
        // do Super mega advanced AI stuff
        MoveOrTurnTowardsPath();
    }

    // Just some initializations
    void FirstRun()
    {
        firstRun = false;
        //Initialize the grid with walkable nodes
        for (int i = 0; i < 40; i++)
        {
            for (int j = 0; j < 40; j++)
            {
                Vector2Int pos = new Vector2Int(i, j);
                Node node = new Node(true, pos);
                // node.gCost;
                grid[i, j] = node;
            }
        }
        // Fills the grid array walls
        for (int i = 0; i < 40; i++)
        {
            grid[0, i] = new Node(false, new Vector2Int(0, i));
            grid[39, i] = new Node(false, new Vector2Int(39, i));
            grid[i, 0] = new Node(false, new Vector2Int(i, 0));
            grid[i, 39] = new Node(false, new Vector2Int(i, 39));
        }
        currentNode = new Node(true, GamePositionToGridPosition(Vector2Int.RoundToInt(GetPosition())));
        grid[currentNode.gridPosition.x, currentNode.gridPosition.y] = currentNode;
    }

    /// <summary>
    /// calculates the closest target and chooses that in a quite silly way.
    /// </summary>
    private void ChooseClosestTarget()
    {
        float closestDist = float.MaxValue;
        var enemyPos = GetEnemyPositions();
        foreach (Vector2 pos in enemyPos)
        {
            Vector2Int gridPos = GamePositionToGridPosition(pos);
            Node n = grid[gridPos.x, gridPos.y];
            float tempDist = ManhattanDistance(n, currentNode);

            if (tempDist < closestDist)
            {
                closestDist = tempDist;
                targetNode = n;
            }
        }
        // Debug.Log("Closest enemy at position Column: " + targetNode.gridPosition.x + " Row: " + targetNode.gridPosition.y);
    }

    /// <summary>
    /// Based mostly on Code Monkey's guide on A* pathfinding https://youtu.be/alU04hvz6L4
    /// </summary>
    void aStarPathFinding()
    {
        Node start = currentNode;

        var openList = new List<Node>() { start };
        var closedList = new List<Node>();

        foreach (Node n in grid)
        {
            n.gCost = int.MaxValue;
            n.cameFrom = null;
        }

        start.gCost = 0;
        start.hCost = ManhattanDistance(start, targetNode);

        while (openList.Count > 0)
        {
            Node current = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < current.fCost)
                {
                    current = openList[i];
                }
            }

            if (current.gridPosition == targetNode.gridPosition)
            {
                TracePath(current);
                return;
            }

            openList.Remove(current);
            closedList.Add(current);

            var neighbours = GetNodeNeighbours(current);
            foreach (Node neighbour in neighbours)
            {
                if (closedList.Contains(neighbour)) continue;

                float tentativeGCost = current.gCost + ManhattanDistance(current, neighbour);

                // This bit of spaghetti will hopefully reduce the staircasing event, where the path zigzags from the start node to the destination.
                if (current.cameFrom != null)
                {
                    var DistToNeighbor = ManhattanDistance(current, neighbour);
                    var distToneighborFromParen = ManhattanDistance(current.cameFrom, neighbour);
                    // Debug.Log("dist to neighbor is: " + DistToNeighbor);
                    // Debug.Log("dist to neighbor from parent is: " + distToneighborFromParen);
                    if (!Mathf.Approximately(distToneighborFromParen, 2 * DistToNeighbor))
                    {
                        // Debug.Log("stupod move lets add penalty");
                        // I sure as hell hope this is enough to deter staircasing
                        tentativeGCost += 10;
                        // Debug.Log("tentative gcost is " + tentativeGCost);
                    }
                }

                if (tentativeGCost < neighbour.gCost)
                {
                    neighbour.cameFrom = current;
                    neighbour.gCost = tentativeGCost;
                    neighbour.hCost = ManhattanDistance(neighbour, targetNode);

                    if (!openList.Contains(neighbour))
                    {
                        openList.Add(neighbour);
                    }
                }
            }
        }
    }

    /// <summary>
    /// The function we use to trace back the path.
    /// </summary>
    /// <param name="end"></param>
    void TracePath(Node end)
    {
        var path = new List<Node>() { end };
        var current = end;
        while (current.cameFrom != null)
        {
            path.Add(current.cameFrom);
            current = current.cameFrom;
        }
        path.Reverse();
        // Currently the first index of the path is our current position. We don't need this, so snipetysnap :D
        path.RemoveAt(0);
        currentPath = path;
    }


    // The brains of our AI
    private void MoveOrTurnTowardsPath()
    {
        Vector2Int forwardTileGridPos = currentNode.gridPosition + Vector2Int.RoundToInt(GetRotation());
        // This prevented some index out of bounds errors I encountered
        if (currentPath.Count == 0) return;
        // If we are already facing the desired node
        if (currentPath[0].gridPosition == forwardTileGridPos)
        {
            // Check if it contains the enemy.
            if (GetForwardTileStatus() == 2)
            {
                // Hit that fool!;
                // Debug.Log("I pity this fool! Bam!");
                nextMove = Hit;
                return;
            }
            else if (GetForwardTileStatus() == 1)
            {
                // else if it's a wall, update the memory grid to remember a wall here. This is my "smart feature"
                grid[forwardTileGridPos.x, forwardTileGridPos.y].isWalkable = false;
                // Debug.Log("No wall has ever stopped me, except for this one! Rats!");
                // Then we need to update our pathfinding and check what we should do in order to not waste a turn
                aStarPathFinding();
                MoveOrTurnTowardsPath();
                return;
            }
            else
            {
                // We should be able to freely move now. Yay!
                nextMove = MoveForward;
            }
        }
        else
        {
            // We are facing the wrong way. Darn!
            var rotation = GetRotation();
            var desiredRotation = currentNode.gridPosition - currentPath[0].gridPosition;
            var signedAngle = Vector2.SignedAngle(rotation, desiredRotation);
            if (Mathf.Approximately(signedAngle, 180f))
            {
                // randomly turn left or right
                if (UnityEngine.Random.value < 0.5) nextMove = TurnLeft;
                else nextMove = TurnRight;
            }
            else if (signedAngle < 0)
            {
                // Debug.Log("turning left!");
                nextMove = TurnLeft;
            }
            else
            {
                // Debug.Log("Turning right!");
                nextMove = TurnRight;
            }
        }
    }

    /// <summary>
    /// The position you get out of the methods provided by the 
    /// interface aren't the real grid positions we want, so this helper 
    /// function turns them into something I can use with my grid array
    /// </summary>
    /// <param name="posToConvert">The position we want to be converted</param>
    /// <returns>Coordinates which we can use with the grid array</returns>
    Vector2Int GamePositionToGridPosition(Vector2 posToConvert)
    {
        // Kiitos Jussi :D
        return new Vector2Int(Mathf.FloorToInt(posToConvert.x + 20), Mathf.FloorToInt(posToConvert.y + 20));
    }

    // --------------------------------------------
    // CLASSES ETC NEEDED FOR A* ALGORITHM
    // --------------------------------------------

    public class Node
    {
        public bool isWalkable;
        public Vector2Int gridPosition;

        public int startCost;
        public float gCost;
        public float hCost;
        public Node cameFrom;

        public float fCost
        {
            get
            {
                return gCost + hCost;
            }
        }
        public Node(bool _walkable, Vector2Int pos)
        {
            isWalkable = _walkable;
            gridPosition = pos;
        }
    }

    /// <summary>
    /// Returns the viable neighbours of the given node. In theory, it should ignore the neighbours we can't use(as in, it SHOULD ignore walls)
    /// </summary>
    /// <param name="node">The node from which we want the neighbours from</param>
    /// <returns>a list of viable neighbours</returns>
    List<Node> GetNodeNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        Vector2Int[] posOffsets = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        // Index out of bounds? what's that? Can I eat it?
        for (int i = 0; i < posOffsets.Length; i++)
        {
            Vector2Int pos = node.gridPosition + posOffsets[i];
            Node neighbour = grid[pos.x, pos.y];
            if (neighbour.isWalkable)
            {
                neighbours.Add(neighbour);
            }
        }
        return neighbours;
    }

    /// <summary>
    /// A function which calculates the manhattan distance of given nodes. Doesn't really matter which node is the end or the beginning since we're dealing with absolute values anyway(?).
    /// The heuristic D (the value we're multiplying our end result)is just something I just randomly chose, not sure if it really matters in the end.
    /// </summary>
    /// <param name="a">The start node=</param>
    /// <param name="b">the end node?</param>
    /// <returns>The distance between the given nodes based on the Manhattan algorithm</returns>
    float ManhattanDistance(Node a, Node b)
    {
        float x = Mathf.Abs(a.gridPosition.x - b.gridPosition.x);
        float y = Mathf.Abs(a.gridPosition.y - b.gridPosition.y);
        return 5 * (x + y);
    }

    // A function I used to calculate the distance between the nodes. It didn't work out quite as well as I had hoped
    float CalculateDistanceCost(Node a, Node b)
    {
        float dist = Vector2.Distance(a.gridPosition, b.gridPosition);
        return dist;
    }
}