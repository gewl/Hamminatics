using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using System.Linq;  

public class Tile
{
    public Vector2Int Position { get; private set; }
    List<Tile> neighbors;

    public string ID
    {
        get
        {
            string idString = "";

            for (int i = 0; i < 4; i++)
            {
                Direction direction = (Direction)i;
                if (ConnectsToNeighbor(direction))
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

    public void RemoveNeighbor(Tile tile, bool removeFromNeighbor = true)
    {
        neighbors.Remove(tile);
        if (removeFromNeighbor)
        {
            tile.RemoveNeighbor(this, false);
        }
    }

    public void RemoveRandomNeighbor()
    {
        int neighborIndex = new System.Random().Next(0, neighbors.Count);
        Tile neighborToRemove = neighbors[neighborIndex];

        if (neighbors.Count < 2 || neighborToRemove.GetNumberOfNeighbors() < 2)
        {
            return;
        }

        RemoveNeighbor(neighborToRemove);
    }

    public bool IsConnectedToTile(Tile tile)
    {
        return neighbors.Contains(tile);
    }

    public bool IsConnectedToTile(Vector2Int position)
    {
        return neighbors.Any(neighbor => neighbor.Position == position);
    }

    public bool ConnectsToNeighbor(Direction testDirection)
    {
        Vector2Int testPosition = Position;

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

        return IsConnectedToTile(testPosition);
    }

}
