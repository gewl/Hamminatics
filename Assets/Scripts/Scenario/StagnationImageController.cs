using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StagnationImageController : MonoBehaviour {

    Vector2[] boardCorners;

    public void Initialize(Vector2[] _boardCorners)
    {
        boardCorners = _boardCorners;      
    }
}
