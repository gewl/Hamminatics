using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardHelperFunctions : MonoBehaviour {

    #region Exposed methods for data retrieval
    static public Tile GetTileAtPosition(Vector2Int position)
    {
        return BoardController.CurrentBoard.Tiles[position.x, position.y];
    }

    static public List<Tile> GetPotentialTargets(Vector2Int startingPosition, int range)
    {
        Tile startingTile = GetTileAtPosition(startingPosition);

        return GetPotentialTargets(startingTile, range);
    }

    static public List<Tile> GetPotentialTargets(Tile startingTile, int range)
    {
        List<Tile> potentialTargets = new List<Tile>();

        foreach (Direction direction in Enum.GetValues(typeof(Direction)))
        {
            int rangeRemaining = range;
            Tile nextTile = startingTile.GetDirectionalNeighbor(direction);

            while (rangeRemaining > 0 && nextTile != null)
            {
                potentialTargets.Add(nextTile);

                nextTile = nextTile.GetDirectionalNeighbor(direction);
                rangeRemaining--;
            }
        }

        return potentialTargets;
    }

    static public List<Tile> GetPotentialTargetsRecursively(Vector2Int position, int range)
    {
        return GetPotentialTargetsRecursively(GetTileAtPosition(position), range);
    }

    // For branching/multidirectional searching.
    static public List<Tile> GetPotentialTargetsRecursively(Tile startingTile, int range, List<Tile> checkedTiles = null)
    {
        if (range == 0)
        {
            return new List<Tile>();
        }
        List<Tile> potentialTargets = new List<Tile>();

        if (checkedTiles == null)
        {
            checkedTiles = new List<Tile> { startingTile };
        }

        for (int i = 0; i < startingTile.Neighbors.Count; i++)
        {
            Tile potentialTarget = startingTile.Neighbors[i];

            if (checkedTiles.Contains(potentialTarget))
            {
                continue; 
            }

            potentialTargets.Add(potentialTarget);
            checkedTiles.Add(potentialTarget);
        }

        range--;
        List<Tile> resultsOfRecursion = new List<Tile>();
        for (int i = 0; i < potentialTargets.Count; i++)
        {
            resultsOfRecursion.AddRange(GetPotentialTargetsRecursively(potentialTargets[i], range, checkedTiles));
        }

        return potentialTargets.Union(resultsOfRecursion).ToList();
    }

    static public bool AreTwoPositionsLinear(Vector2Int position1, Vector2Int position2)
    {
        return AreTwoTilesLinear(BoardController.CurrentBoard.GetTileAt(position1), BoardController.CurrentBoard.GetTileAt(position2));
    }

    static public bool AreTwoTilesLinear(Tile tile1, Tile tile2)
    {
        if (tile1.Position.x != tile2.Position.x && tile1.Position.y != tile2.Position.y)
        {
            return false;
        }

        return true;
    }

    static public int GetLinearDistanceBetweenTiles(Tile tile1, Tile tile2)
    {
        Vector2Int position1 = tile1.Position;
        Vector2Int position2 = tile2.Position;

        if (position1.x == position2.x && position1.y == position2.y)
        {
            Debug.LogError("Attempted to find linear distance between non-linear tiles.");
            return 0;
        }

        return Mathf.Max(Math.Abs(position1.x - position2.x), Mathf.Abs(position1.y - position2.y));
    }

    static public List<Tile> GetDirectlyReachableTiles(Vector2Int position)
    {
        return GetDirectlyReachableTiles(BoardController.CurrentBoard.GetTileAt(position));
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

    #endregion
}
