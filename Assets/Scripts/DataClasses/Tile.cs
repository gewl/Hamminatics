using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using System.Linq;

public class Tile
{
    public Vector2Int Position { get; private set; }
    List<Tile> neighbors;
    public List<Tile> Neighbors { get { return neighbors; } }

    public int DistanceFromPlayer;
    public bool VisitedByPathfinding;

    public string ID
    {
        get
        {
            string idString = "";

            for (int i = 0; i < 4; i++)
            {
                Direction direction = (Direction)i;
                if (this.ConnectsToNeighbor(direction))
                {
                    idString += "1";
                }
                else
                {
                    idString += "0";
                }
            }

            return idString;
        }
    }

    public Tile(int x, int y)
    {
        Position = new Vector2Int(x, y);
        neighbors = new List<Tile>();
        DistanceFromPlayer = int.MaxValue;
        VisitedByPathfinding = false;
    }

    public void AddNeighbor(Tile tile, bool addFromNeighbor = true)
    {
        if (neighbors.Contains(tile))
        {
            Debug.LogError("Tile at " + Position + " is already connected to tile at " + tile.Position);
            return;
        }
        neighbors.Add(tile);
        if (addFromNeighbor)
        {
            tile.AddNeighbor(this, false);
        }
    }

    public int GetNumberOfNeighbors()
    {
        return neighbors.Count;
    }

    public override int GetHashCode()
    {
        int result = 0;
        result = (result * 418) ^ Position.x;
        result = (result * 418) ^ Position.y;
        return result;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        Tile otherTile = (Tile)obj;

        return otherTile.Position.x == Position.x && otherTile.Position.y == Position.y;
    }
}
