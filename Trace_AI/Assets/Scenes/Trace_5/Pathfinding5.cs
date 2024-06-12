using UnityEngine;
using System.Collections.Generic;

public class Pathfinding5 : MonoBehaviour
{
    public Grid5 grid5;

    public List<Node5> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node5 startNode = grid5.NodeFromWorldPoint(startPos);
        Node5 targetNode = grid5.NodeFromWorldPoint(targetPos);

        List<Node5> openSet = new List<Node5>();
        HashSet<Node5> closedSet = new HashSet<Node5>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node5 currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            foreach (Node5 neighbour in grid5.GetNeighbours(currentNode))
            {
                if (neighbour.movementPenalty >= 10000 || closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + neighbour.movementPenalty;
                if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

        return null;
    }

    List<Node5> RetracePath(Node5 startNode, Node5 endNode)
    {
        List<Node5> path = new List<Node5>();
        Node5 currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        return path;
    }

    int GetDistance(Node5 nodeA, Node5 nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    void OnDrawGizmos()
    {
        if (grid5 != null)
        {
            if (grid5.grid5 != null && path != null)
            {
                foreach (Node5 n in path)
                {
                    Gizmos.color = Color.black;
                    Vector3 gizmoPosition = new Vector3(n.worldPosition.x, 1, n.worldPosition.z);
                    Gizmos.DrawCube(gizmoPosition, Vector3.one * (grid5.GetNodeDiameter() - .1f));
                }
            }
        }
    }

    public List<Node5> path;
}
