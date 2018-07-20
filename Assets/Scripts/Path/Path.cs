using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path {

    PathStep head;

    public Path()
    {
        head = null;
    }

    public Path(EntityData entity, Vector2Int startingPosition)
    {
        head = new PathStep(entity, startingPosition, null);
    }

    #region basics
    public PathEnumerator GetEnumerator()
    {
        return new PathEnumerator(this);
    }

    public bool IsEmpty()
    {
        return head == null || head.nextStep == null;
    }

    public void AddStep(EntityData entity, Vector2Int newPosition, EntityData bumpedEntity = null, EntityData bumpedBy = null)
    {
        PathStep checkNode = PeekLast();
        PathStep newStep = new PathStep(entity, newPosition, checkNode, bumpedEntity, bumpedBy);
        if (head == null)
        {
            head = newStep;
            return;
        }

        checkNode.nextStep = newStep;
    }

    void AddStep(PathStep step)
    {
        PathStep checkNode = PeekLast();
        if (head == null)
        {
            head = step;
            return;
        }

        checkNode.nextStep = step;
    }

    public void AddPath(Path path)
    {
        if (path.head == null)
        {
            Debug.Log("adding null path");
            return;
        }
        AddStep(path.head);
    }

    public PathStep Peek()
    {
        return head;
    }
    
    public PathStep PeekLast()
    {
        PathStep checkNode = head;

        while (checkNode != null && checkNode.nextStep != null)
        {
            checkNode = checkNode.nextStep;
        }

        return checkNode;
    }

    public PathStep GetNext()
    {
        PathStep nextNode = head;
        if (nextNode == null)
        {
            return null;
        }
        head = head.nextStep;
        return nextNode;
    }

    public bool IsFirstStep(PathStep step)
    {
        return step == head;
    }
    #endregion

    #region debugging
    public int GetPathLength()
    {
        int result = 0;

        PathEnumerator iterNode = GetEnumerator();
        while (iterNode.Current != null)
        {
            result++;
            iterNode.MoveNext();
        }
        return result;
    }

    public void LogPath()
    {
        PathStep iterNode = head;

        while (iterNode != null)
        {
            Debug.Log(iterNode.newPosition);
            iterNode = iterNode.nextStep;
        }
    }

    public void LogPathLength()
    {
        int result = 0;
        if (head == null)
        {
            Debug.Log("Path has no length.");
            return;
        }

        PathEnumerator iterNode = GetEnumerator();
        while (iterNode.Current != null)
        {
            result++;
            iterNode.MoveNext();
        }
        Debug.Log(head.pathingEntity.ID + ": " + result);
    }
    #endregion

    #region querying
    public List<Vector2Int> GetAllPositions()
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        PathStep checkNode = head;

        while (checkNode != null)
        {
            positions.Add(checkNode.newPosition);
            checkNode = checkNode.nextStep;
        }

        return positions; 
    }

    public PathStep GetStepWhere(Predicate<PathStep> predicate)
    {
        PathStep currentNode = head;

        while (currentNode != null && !predicate(currentNode))
        {
            currentNode = currentNode.nextStep;
        }

        return currentNode;
    }

    #endregion
}
