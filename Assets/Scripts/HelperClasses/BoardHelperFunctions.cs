﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BoardHelperFunctions {

    static public Tile GetTileAtPosition(this GameBoard board, Vector2Int position)
    {
        return board.Tiles[position.x, position.y];
    }

    static public List<Tile> GetTilesWhere(this GameBoard board, Func<Tile, bool> testFunc)
    {
        return board.Tiles.Cast<Tile>().Where(testFunc).ToList();
    }

    static public List<Tile> GetBorderTiles(this GameBoard board, Direction direction)
    {
        Func<Tile, bool> positionTest = t => t.Position.x == 0;

        switch (direction)
        {
            case Direction.Up:
                positionTest = t => t.Position.y == 0;
                break;
            case Direction.Right:
                positionTest = t => t.Position.x == board.Width - 1;
                break;
            case Direction.Down:
                positionTest = t => t.Position.y == board.Width - 1;
                break;
            default:
                break;
        }

        return board
            .Tiles
            .Cast<Tile>()
            .Where(positionTest)
            .ToList();
    }

    public static Direction GetDirectionFromPosition(Vector2Int startingPosition, Vector2Int targetPosition)
    {
        if (targetPosition.x > startingPosition.x && targetPosition.y == startingPosition.y)
        {
            return Direction.Right;
        }
        else if (targetPosition.x < startingPosition.x && targetPosition.y == startingPosition.y)
        {
            return Direction.Left;
        }
        else if (targetPosition.x == startingPosition.x && targetPosition.y > startingPosition.y)
        {
            return Direction.Down;
        }
        else if (targetPosition.x == startingPosition.x && targetPosition.y < startingPosition.y)
        {
            return Direction.Up;
        }
        else
        {
            Debug.LogError("Positions " + startingPosition + " and " + targetPosition + " are not linear.");
            return Direction.Right;
        }
    }

    static public bool AreTwoPositionsLinear(Vector2Int position1, Vector2Int position2)
    {
        return AreTwoTilesLinear(BoardController.CurrentBoard.GetTileAtPosition(position1), BoardController.CurrentBoard.GetTileAtPosition(position2));
    }

    static public bool AreTwoTilesLinear(Tile tile1, Tile tile2)
    {
        if (tile1.Position.x != tile2.Position.x && tile1.Position.y != tile2.Position.y)
        {
            return false;
        }

        return true;
    }

    static public bool AreTwoTilesLinearlyReachable(Tile tile1, Tile tile2)
    {
        if (!AreTwoTilesLinear(tile1, tile2))
        {
            return false;
        }

        Direction toTile2 = GetDirectionBetweenTiles(tile1, tile2);

        Tile activeTile = tile1;

        while (activeTile != tile2)
        {
            activeTile = activeTile.GetDirectionalNeighbor(toTile2);

            if (activeTile == null)
            {
                return false;
            }
        }

        return true;
    }

    static public int GetLinearDistanceBetweenTiles(Tile tile1, Tile tile2)
    {
        Vector2Int position1 = tile1.Position;
        Vector2Int position2 = tile2.Position;

        return GetLinearDistanceBetweenPositions(position1, position2);
    }

    static public int GetLinearDistanceBetweenPositions(Vector2Int position1, Vector2Int position2)
    {
        if (position1.x == position2.x && position1.y == position2.y)
        {
            Debug.LogError("Attempted to find linear distance between non-linear tiles.");
            return 0;
        }

        return Mathf.Max(Math.Abs(position1.x - position2.x), Mathf.Abs(position1.y - position2.y));
    }

    static public Direction GetDirectionBetweenTiles(Tile startingTile, Tile destinationTile)
    {
        if (!AreTwoTilesLinear(startingTile, destinationTile))
        {
            Debug.LogError("Tried to find direction between non-linear tiles.");
            return Direction.Down;
        }

        int xDifference = destinationTile.Position.x - startingTile.Position.x;
        int yDifference = destinationTile.Position.y - startingTile.Position.y;

        if (xDifference < 0)
        {
            return Direction.Left;
        }
        else if (xDifference > 0)
        {
            return Direction.Right;
        }
        else if (yDifference > 0)
        {
            return Direction.Down;
        }
        else
        {
            return Direction.Up;
        }
    }

    static public List<Tile> GetDirectlyReachableTiles(Vector2Int position)
    {
        return GetDirectlyReachableTiles(BoardController.CurrentBoard.GetTileAtPosition(position));
    }

    static public List<Tile> GetDirectlyReachableTiles(Tile startingTile)
    {
        List<Tile> reachableTiles = new List<Tile>();

        Tile leftNeighbor = startingTile.GetDirectionalNeighbor(Direction.Left);
        while (leftNeighbor != null)
        {
            reachableTiles.Add(leftNeighbor);

            leftNeighbor = leftNeighbor.GetDirectionalNeighbor(Direction.Left);
        }
            
        Tile upNeighbor = startingTile.GetDirectionalNeighbor(Direction.Up);
        while (upNeighbor != null)
        {
            reachableTiles.Add(upNeighbor);

            upNeighbor = upNeighbor.GetDirectionalNeighbor(Direction.Up);
        }

        Tile downNeighbor = startingTile.GetDirectionalNeighbor(Direction.Down);
        while (downNeighbor != null)
        {
            reachableTiles.Add(downNeighbor);

            downNeighbor = downNeighbor.GetDirectionalNeighbor(Direction.Down);
        }

        Tile rightNeighbor = startingTile.GetDirectionalNeighbor(Direction.Right);
        while (rightNeighbor != null)
        {
            reachableTiles.Add(rightNeighbor);

            rightNeighbor = rightNeighbor.GetDirectionalNeighbor(Direction.Right);
        }

        return reachableTiles;
    }

    static public List<Direction> FindPathBetweenTiles(Tile startingTile, Tile destinationTile)
    {
        bool tileFound = false;
        // Key: Reachable tile
        // Value: Tile from which Key can be reached ('parent' tile)
        Dictionary<Tile, Tile> reachableTiles = new Dictionary<Tile, Tile>();

        Queue<Tile> tilesToCheck = new Queue<Tile>();
        tilesToCheck.Enqueue(startingTile);

        // Breadth-first search starting from initial tile until destination tile found.
        while (!tileFound)
        {
            Tile currentTile = tilesToCheck.Dequeue();

            foreach (Tile neighbor in currentTile.Neighbors)
            {
                if (reachableTiles.ContainsKey(neighbor))
                {
                    continue;
                }

                reachableTiles[neighbor] = currentTile;

                if (neighbor == destinationTile)
                {
                    tileFound = true;
                    break;
                }
                tilesToCheck.Enqueue(neighbor);
            }
        }

        List<Direction> path = new List<Direction>();

        // Work backwards from destination tile, adding a step to path with each step backward.
        Tile tile = destinationTile;
        Tile parent = reachableTiles[tile];

        while (tile != startingTile)
        {
            parent = reachableTiles[tile];
            path.Add(GetDirectionBetweenTiles(parent, tile));
            tile = parent;
        }

        path.Reverse();

        return path;
    }

    static public List<Tile> GetTilesOnPath(Tile startingTile, List<Direction> path)
    {
        List<Tile> tilesOnPath = new List<Tile>();

        Tile lastTile = startingTile;
        for (int i = 0; i < path.Count; i++)
        {
            Direction nextStep = path[i];
            Tile nextTile = lastTile.GetDirectionalNeighbor(nextStep);

            tilesOnPath.Add(nextTile);

            lastTile = nextTile;
        }

        return tilesOnPath;
    }
}
