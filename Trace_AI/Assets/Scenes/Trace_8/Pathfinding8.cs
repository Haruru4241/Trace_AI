using UnityEngine;
using System.Collections.Generic;

public class Pathfinding8 : MonoBehaviour
{
    public Grid8 grid8;

    public List<Node8> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node8 startNode = grid8.NodeFromWorldPoint(startPos);
        Node8 targetNode = grid8.NodeFromWorldPoint(targetPos);

        List<Node8> openSet = new List<Node8>();
        HashSet<Node8> closedSet = new HashSet<Node8>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node8 currentNode = openSet[0];
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

            foreach (Node8 neighbour in grid8.GetNeighbours(currentNode))
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

    List<Node8> RetracePath(Node8 startNode, Node8 endNode)
    {
        List<Node8> path = new List<Node8>();
        Node8 currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        return path;
    }

    int GetDistance(Node8 nodeA, Node8 nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    public Node8 GetNodeFromPosition(Vector3 position)
    {
        return grid8.NodeFromWorldPoint(position);
    }
}
