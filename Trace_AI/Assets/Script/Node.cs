using UnityEngine;

public class Node
{
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    public LayerMask layerMask;

    public int gCost;
    public int hCost;
    public Node parent;

    public Node(Vector3 _worldPos, int _gridX, int _gridY, LayerMask _layerMask)
    {
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        layerMask = _layerMask;
    }

    public int fCost
    {
        get { return gCost + hCost; }
    }

    public int CompareTo(Node other)
    {
        int compare = fCost.CompareTo(other.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(other.hCost);
        }
        return -compare;
    }
}
