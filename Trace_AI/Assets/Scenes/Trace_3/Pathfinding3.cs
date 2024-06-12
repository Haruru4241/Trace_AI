using UnityEngine;
using System.Collections.Generic;

public class Pathfinding3 : MonoBehaviour
{
    public Transform seeker, target;
    Grid3 grid3;
    public List<Node3> path;

    void Awake()
    {
        grid3 = GetComponent<Grid3>();
    }

    void Update()
    {
        FindPath(seeker.position, target.position);
    }

    public void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node3 startNode = grid3.NodeFromWorldPoint(startPos);
        Node3 targetNode = grid3.NodeFromWorldPoint(targetPos);

        List<Node3> openSet = new List<Node3>();
        HashSet<Node3> closedSet = new HashSet<Node3>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node3 currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                return;
            }

            foreach (Node3 neighbour in grid3.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }
    }

    void RetracePath(Node3 startNode, Node3 endNode)
    {
        path = new List<Node3>();
        Node3 currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
    }

    int GetDistance(Node3 nodeA, Node3 nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    void OnDrawGizmos()
    {
        if (path != null)
        {
            foreach (Node3 n in path)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (grid3.nodeDiameter - .1f));
            }
        }
    }
}
