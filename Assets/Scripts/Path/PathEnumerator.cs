using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathEnumerator : MonoBehaviour {

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
}
