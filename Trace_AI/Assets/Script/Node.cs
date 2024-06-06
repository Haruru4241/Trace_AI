using UnityEngine;

public class Node
{
    public Vector3 position;
    public bool isWalkable;
    public bool isSlowZone;
    public Node parent;
    public float gCost;
    public float hCost;
    public float fCost { get { return gCost + hCost; } }
    public int gridX;
    public int gridY;

    public Node(Vector3 pos, bool walkable, bool slowZone, int gridX, int gridY)
    {
        position = pos;
        isWalkable = walkable;
        isSlowZone = slowZone;
        this.gridX = gridX;
        this.gridY = gridY;
    }
}
