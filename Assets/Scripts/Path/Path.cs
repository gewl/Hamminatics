using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path {

    PathStep head;

    public Path(EntityData entity, Vector2Int initialPosition)
    {
        head = new PathStep(entity, initialPosition);
    }

    public Path()
    {
        head = null;
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

    public void AddStep(PathStep step)
    {
        if (head == null)
        {
            head = step;
            return;
        }

        PathStep checkNode = PeekLast();

        step.lastPosition = checkNode.newPosition;
        checkNode.nextStep = step;
    }

    public void AddPath(Path path)
    {
        AddStep(path.head);
    }

    public PathStep Peek()
    {
        return head;
    }
    
    public PathStep PeekLast()
    {
        PathStep checkNode = head;

        while (checkNode.nextStep != null)
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
    #endregion

    #region debugging
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
        PathStep iterNode = head;
        if (head == null)
        {
            Debug.Log("Path has no length.");
            return;
        }

        while (iterNode != null)
        {
            result++;
            iterNode = iterNode.nextStep;
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
