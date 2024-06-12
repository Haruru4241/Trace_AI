using UnityEngine;
using System.Collections.Generic;

public class Pathfinding6 : MonoBehaviour
{
    public Grid6 grid6;

    public List<Node6> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node6 startNode = grid6.NodeFromWorldPoint(startPos);
        Node6 targetNode = grid6.NodeFromWorldPoint(targetPos);

        List<Node6> openSet = new List<Node6>();
        HashSet<Node6> closedSet = new HashSet<Node6>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node6 currentNode = openSet[0];
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

            foreach (Node6 neighbour in grid6.GetNeighbours(currentNode))
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

    List<Node6> RetracePath(Node6 startNode, Node6 endNode)
    {
        List<Node6> path = new List<Node6>();
        Node6 currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        return path;
    }

    int GetDistance(Node6 nodeA, Node6 nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    void OnDrawGizmos()
    {
        if (grid6 != null)
        {
            if (grid6.grid6 != null)
            {
                foreach (Node6 n in grid6.grid6)
                {
                    Gizmos.color = (n.movementPenalty >= grid6.unwalkablePenalty) ? Color.red : Color.white;
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (grid6.nodeDiameter - .1f));
                }
            }
        }
    }
}
