using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;  
using UnityEngine;

public static class TileExtensions {

    public static Tile GetNeighborWhere(this Tile tile, System.Predicate<Tile> predicate)
    {
        return tile.Neighbors.Find(predicate);
    }

    public static Tile GetDirectionalNeighbor(this Tile tile, Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return tile.GetNeighborWhere(t => t.Position.y == tile.Position.y - 1);
            case Direction.Right:
                return tile.GetNeighborWhere(t => t.Position.x == tile.Position.x + 1);
            case Direction.Down:
                return tile.GetNeighborWhere(t => t.Position.y == tile.Position.y + 1);
            case Direction.Left:
                return tile.GetNeighborWhere(t => t.Position.x == tile.Position.x - 1);
            default:
                return null;
        }
    }

    public static bool HasNeighborWhere(this Tile tile, System.Predicate<Tile> predicate)
    {
        return tile.Neighbors.Exists(predicate);
    }

    public static bool CheckThatNeighbor(this Tile tile, Vector2Int position, Predicate<Tile> predicate)
    {
        return tile.HasNeighborWhere(t => t.Position == position) && predicate(tile.GetNeighborWhere(t => t.Position == position));
    }

    public static Tile GetRandomNeighbor(this Tile tile, bool checkForWallEligibility = false)
    {
        int neighborIndex = new System.Random().Next(0, tile.Neighbors.Count);
        Tile neighborToRemove = tile.Neighbors[neighborIndex];

        if (checkForWallEligibility && tile.Neighbors.Count < 2 || neighborToRemove.GetNumberOfNeighbors() < 2)
        {
            return null;
        }
        return neighborToRemove;
    }

    public static bool HasNeighbors(this Tile tile, int numberOfNeighbors)
    {
        return tile.Neighbors.Count >= numberOfNeighbors;
    }

    public static void RemoveNeighbor(this Tile tile, Tile neighbor, bool removeFromNeighbor = true)
    {
        tile.Neighbors.Remove(neighbor);
        if (removeFromNeighbor)
        {
            neighbor.RemoveNeighbor(tile, false);
        }
    }

    public static void RemoveRandomNeighbor(this Tile tile)
    {
        int neighborIndex = new System.Random().Next(0, tile.Neighbors.Count);
        Tile neighborToRemove = tile.Neighbors[neighborIndex];

        if (tile.Neighbors.Count < 2 || neighborToRemove.GetNumberOfNeighbors() < 2)
        {
            return;
        }

        tile.RemoveNeighbor(neighborToRemove);
    }

    public static bool IsConnectedToTile(this Tile tile, Tile tileToCheck)
    {
        return tile.Neighbors.Contains(tileToCheck);
    }

    public static bool IsConnectedToTile(this Tile tile, Vector2Int position)
    {
        return tile.Neighbors.Any(neighbor => neighbor.Position == position);
    }

    public static bool ConnectsToNeighbor(this Tile tile, Direction testDirection)
    {
        Vector2Int testPosition = tile.Position;

        switch (testDirection)
        {
            case Direction.Up:
                testPosition.y--;
                break;
            case Direction.Down:
                testPosition.y++;
                break;
            case Direction.Left:
                testPosition.x--;
                break;
            case Direction.Right:
                testPosition.x++;
                break;
            default:
                break;
        }

        return tile.IsConnectedToTile(testPosition);
    }

    public static bool IsOccupied(this Tile tile, ScenarioState state)
    {
        return state.IsTileOccupied(tile);
    }

    public static bool IsUnoccupied(this Tile tile, ScenarioState state)
    {
        return !state.IsTileOccupied(tile);
    }

    public static bool CheckThat(this Tile tile, Predicate<Tile> predicate)
    {
        return predicate(tile);
    }

    #region Enemy turn calculation methods
    // All tiles within [range] steps from starting tile, along cardinal directions.
    static public List<Tile> GetTilesWithinLinearRange(this Tile startingTile, int range)
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

    // All tiles within [range] steps from starting tile.
    static public List<Tile> GetAllTilesWithinRange(this Tile startingTile, int range, List<Tile> checkedTiles = null)
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
            resultsOfRecursion.AddRange(GetAllTilesWithinRange(potentialTargets[i], range, checkedTiles));
        }

        return potentialTargets.Union(resultsOfRecursion).ToList();
    }

    // All tiles exactly [range] distance from starting tile, in steps
    static public List<Tile> GetAllTilesAtDistance(this Tile startingTile, int range, List<Tile> checkedTiles = null)
    {
        if (range == 0)
        {
            return new List<Tile> { startingTile };
        }
        if (checkedTiles == null)
        {
            checkedTiles = new List<Tile> { startingTile };
        }

        List<Tile> touchedTiles = new List<Tile>();

        for (int i = 0; i < startingTile.Neighbors.Count; i++)
        {
            Tile potentialTarget = startingTile.Neighbors[i];

            if (checkedTiles.Contains(potentialTarget))
            {
                continue; 
            }

            checkedTiles.Add(potentialTarget);
            touchedTiles.Add(potentialTarget);
        }

        range--;
        List<Tile> resultsOfRecursion = new List<Tile>();
        for (int i = 0; i < touchedTiles.Count; i++)
        {
            resultsOfRecursion.AddRange(GetAllTilesAtDistance(touchedTiles[i], range, checkedTiles));
        }

        return resultsOfRecursion;
    }

    static public List<EntityTurnTargets> GetAllPossibleEntityTurns(this Tile startingTile, int enemyMovementRange, int enemyAttackRange)
    {
        return startingTile
            .GetAllTilesWithinRange(enemyMovementRange)
            .SelectMany(moveTile => moveTile.GetTilesWithinLinearRange(enemyAttackRange),
                (moveTile, attackTile) => new EntityTurnTargets(moveTile, attackTile))
            .DistinctBy(possibleTurn => possibleTurn.targetAttackTile)
            .ToList();
    }

    #endregion
}
