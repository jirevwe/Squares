using UnityEngine;
using System.Collections;

[System.Serializable]
public class Node
{
    public bool walkable;
    public bool colored;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    
    public Node parent;

    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }

    public bool Done
    {
        get
        {
            return walkable && colored;
        }
    }

    public override string ToString()
    {
        return gridX + ", " + gridY;
    }
}
