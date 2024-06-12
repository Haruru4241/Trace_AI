using UnityEngine;

public class Node4
{
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;
    public Node4 parent;

    public int movementPenalty;

    public Node4(Vector3 _worldPos, int _gridX, int _gridY, int _movementPenalty)
    {
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        movementPenalty = _movementPenalty;
    }

    public int fCost
    {
        get { return gCost + hCost + movementPenalty; }
    }

    public int CompareTo(Node4 other)
    {
        int compare = fCost.CompareTo(other.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(other.hCost);
        }
        return -compare;
    }
}
