using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathEnumerator {

    PathStep currentNode;

    public PathStep Current { get { return currentNode; } }

    public PathEnumerator(Path path)
    {
        currentNode = path.Peek();
    }

    public void MoveNext()
    {
        currentNode = currentNode.nextStep;
    }

    public bool IsBumpedStep()
    {
        return currentNode != null && Current.bumpedBy != null;
    }

    public bool IsBumpStep()
    {
        return currentNode != null && Current.bumpedEntity != null;
    }

    public bool IsNormalPathingStep()
    {
        return currentNode != null && currentNode.bumpedBy == null && currentNode.bumpedEntity == null;
    }
}
