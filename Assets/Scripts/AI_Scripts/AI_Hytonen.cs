using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = System.Random;
using Vector2 = UnityEngine.Vector2;

namespace Assets.Scripts
{

    public class AI_Hytonen : PlayerControllerInterface
    {
        private readonly List<Room> _rooms = new List<Room>();
        private Room _currentRoom;
        private Room _previousRoom;
        private Vector2 _botPreviousPosition;
        private Vector2 _botPreviousRotation;
        private Vector2 _botCurrentPosition;
        private Vector2 _botCurrentRotation;
        private Vector2 _targetEnemyPosition;
        private Vector2 _targetEnemyRotation;
        private int _targetEnemyHP;
        private Direction _botCurrentDirection;
        private readonly Random _rand = new Random();
        private int _forwardTileStatus;
        private int rounds = 0;

        /*
         * Enumerations for different Tile statuses.
         */
        public enum TileStatus
        {
            Empty = 0,
            Wall = 1,
            Player = 2
        }

        /*
         * Enumerations for different Directions.
         */
        public enum Direction
        {
            NotSet = 0,
            Up = 1,
            Right = 2,
            Down = 3,
            Left = 4
        }

        /*
         * Class for each "room" that the bot moves into.
         */
        public class Room
        {
            public Vector2 Location { get; set; }

            public List<Direction> PossibleDirections = new List<Direction>() { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

            public List<Direction> MovedDirections = new List<Direction>();

            public List<Direction> EnteredDirections = new List<Direction>();

            public Room(Vector2 location)
            {
                Location = location;
            }
        }

        /*
         * Main method for the exercise which is called each turn to decide next move for the bot.
         */
        public override void DecideNextMove()
        {
            // Get status of the tile bot is facing
            _forwardTileStatus = GetForwardTileStatus();

            // Store bot's current position
            _botCurrentPosition = GetPosition();
            // Store bot's current rotation
            _botCurrentRotation = GetRotation();
            // Store bot's direction based on rotation
            _botCurrentDirection = DirectionFromRotation(_botCurrentRotation);

            // draw debug line from position towards current rotation
            Debug.DrawLine(_botCurrentPosition, _botCurrentPosition + _botCurrentRotation * 2.0f, Color.blue, 0.1f);

            // Get room from list of stored rooms based on bot location
            var existingRoom = _rooms.FirstOrDefault(room => room.Location == _botCurrentPosition);
            // Check if room exists already
            if (existingRoom == null)
            {
                // Create new room using current bot location
                existingRoom = new Room(_botCurrentPosition);
                _rooms.Add(existingRoom);
            }

            // Check if bot has really moved since last turn
            if (_botCurrentPosition != _botPreviousPosition)
            {
                // Store previous room
                _previousRoom = _currentRoom;
                // Store current room
                _currentRoom = existingRoom;
                // Store direction from which the room was entered from
                if (!_currentRoom.EnteredDirections.Contains(OppositeDirectionForDirection(_botCurrentDirection)))
                    _currentRoom.EnteredDirections.Add(OppositeDirectionForDirection(_botCurrentDirection));

                // Store movement direction for previous room
                if (_previousRoom != null)
                {
                    if (!_previousRoom.MovedDirections.Contains(_botCurrentDirection))
                    {
                        _previousRoom.MovedDirections.Add(_botCurrentDirection);
                    }
                }
            }

            // update closest target position (and rotation)
            UpdateClosestTarget();
            // Get directions to current enemy target
            var directionsToEnemy = DirectionsToEnemy();
            // Get Direction to enemy if it's right next to the bot
            var dirToEnemy = DirectionToEnemyNextToTheBot();

            // Check if bot has enemy right next to it
            if (dirToEnemy != Direction.NotSet)
            {
                // Enemy is in front of the bot
                if (_forwardTileStatus == (int)TileStatus.Player)
                {
                    nextMove = Hit;
                }
                else // Need to turn to face the enemy
                {
                    nextMove = NextMoveBasedOnTargetDirection(dirToEnemy);
                }
            }
            // Empty grid position ahead
            else if (_forwardTileStatus == (int)TileStatus.Empty)
            {
                // Intersect possible directions with current directions to enemy
                var directions = _currentRoom.PossibleDirections.Intersect(directionsToEnemy).ToList();
                if (directions.Count == 0)
                {
                    // All available directions to choose from
                    directions = _currentRoom.PossibleDirections;
                }

                var dir = directions.OrderBy(x => _rand.Next()).Take(1).First();

                nextMove = NextMoveBasedOnTargetDirection(dir);
            }
            // Wall ahead
            else if (_forwardTileStatus == (int)TileStatus.Wall)
            {
                // Remove current direction from the list of possible directions since there is a wall
                _currentRoom.PossibleDirections.Remove(_botCurrentDirection);

                // Intersect possible directions with current directions to enemy
                var directions = _currentRoom.PossibleDirections.Intersect(directionsToEnemy).ToList();
                if (directions.Count == 0)
                {
                    // All available directions to choose from
                    directions = _currentRoom.PossibleDirections;
                }

                var dir = directions.OrderBy(x => _rand.Next()).Take(1).First();
                nextMove = NextMoveBasedOnTargetDirection(dir);
            }

            // Store bot's previous position and rotation
            _botPreviousPosition = GetPosition();
            _botPreviousRotation = GetRotation();
        }

        /*
         * Returns Direction based on provided rotation.
         */
        private Direction DirectionFromRotation(Vector2 rotation)
        {
            Direction dir;

            if (rotation == Vector2.up) dir = Direction.Up;
            else if (rotation == Vector2.down) dir = Direction.Down;
            else if (rotation == Vector2.left) dir = Direction.Left;
            else if (rotation == Vector2.right) dir = Direction.Right;
            else dir = Direction.NotSet;

            return dir;
        }

        /*
         * Returns opposite direction for provided direction.
         */
        private Direction OppositeDirectionForDirection(Direction dir)
        {
            Direction oppositeDirection = Direction.Up;
            switch (dir)
            {
                case Direction.Up:
                    oppositeDirection = Direction.Down;
                    break;
                case Direction.Right:
                    oppositeDirection = Direction.Left;
                    break;
                case Direction.Down:
                    oppositeDirection = Direction.Up;
                    break;
                case Direction.Left:
                    oppositeDirection = Direction.Right;
                    break;
            }

            return oppositeDirection;
        }

        /*
         * Returns next move based on target direction.
         */
        private NextMove NextMoveBasedOnTargetDirection(Direction targetDirection)
        {
            NextMove newMove;

            int dirDiff = targetDirection - _botCurrentDirection;

            if (dirDiff == 3)
                newMove = TurnLeft;
            else if (dirDiff == -3)
                newMove = TurnRight;
            else if (dirDiff == -2)
                newMove = TurnLeft;
            else if (dirDiff == 2)
                newMove = TurnRight;
            else if (dirDiff == -1)
                newMove = TurnLeft;
            else if (dirDiff == 1)
                newMove = TurnRight;
            else newMove = MoveForward;

            return newMove;
        }

        /*
         * Updates closest enemy target based on distance.
         */
        private void UpdateClosestTarget()
        {
            var enemyPositions = GetEnemyPositions();
            float closestDistance = float.MaxValue;
            foreach (var enemyPosition in enemyPositions)
            {
                var distance = Vector2.Distance(_botCurrentPosition, enemyPosition);
                var hp = GetEnemyHP(enemyPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    _targetEnemyPosition = enemyPosition;
                    _targetEnemyRotation = GetEnemyRotation(enemyPosition);
                    _targetEnemyHP = hp;
                }
            }

            // draw debug line towards the enemy
            Debug.DrawLine(_botCurrentPosition, _targetEnemyPosition, Color.red, 0.5f);
        }

        /*
         * Returns list of Directions to the enemy based on locations.
         */
        private List<Direction> DirectionsToEnemy()
        {
            List<Direction> directions = new List<Direction>();

            // "use" 10 rounds to track target position by providing directions to target
            if (rounds < 10)
            {
                if (_botCurrentPosition.y < _targetEnemyPosition.y)
                {
                    directions.Add(Direction.Up);
                }

                if (_botCurrentPosition.x < _targetEnemyPosition.x)
                {
                    directions.Add(Direction.Right);
                }

                if (_botCurrentPosition.y > _targetEnemyPosition.y)
                {
                    directions.Add(Direction.Down);
                }

                if (_botCurrentPosition.x > _targetEnemyPosition.x)
                {
                    directions.Add(Direction.Left);
                }

            }
            // "use" another 10 rounds doing something else
            // Allowing random movement time to time helps to prevent bot from going to infinite movement loop in same spot
            else if (rounds > 20) rounds = 0;

            // Increase rounds counter
            rounds++;

            return directions;
        }

        /*
         * Returns Direction to the enemy if enemy is next to the bot, Direction.NotSet otherwise.
         */
        private Direction DirectionToEnemyNextToTheBot()
        {
            if (_botCurrentPosition + Vector2.up == _targetEnemyPosition)
            {
                return Direction.Up;
            }
            else if (_botCurrentPosition + Vector2.right == _targetEnemyPosition)
            {
                return Direction.Right;
            }
            else if (_botCurrentPosition + Vector2.down == _targetEnemyPosition)
            {
                return Direction.Down;
            }
            else if (_botCurrentPosition + Vector2.left == _targetEnemyPosition)
            {
                return Direction.Left;
            }
            else
            {
                return Direction.NotSet;
            }
        }
    }
}
