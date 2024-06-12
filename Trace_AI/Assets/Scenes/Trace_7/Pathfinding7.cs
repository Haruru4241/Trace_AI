using UnityEngine;
using System.Collections.Generic;

public class Pathfinding7 : MonoBehaviour
{
    public Grid7 grid7;

    public List<Node7> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node7 startNode = grid7.NodeFromWorldPoint(startPos);
        Node7 targetNode = grid7.NodeFromWorldPoint(targetPos);

        List<Node7> openSet = new List<Node7>();
        HashSet<Node7> closedSet = new HashSet<Node7>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node7 currentNode = openSet[0];
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

            foreach (Node7 neighbour in grid7.GetNeighbours(currentNode))
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

    List<Node7> RetracePath(Node7 startNode, Node7 endNode)
    {
        List<Node7> path = new List<Node7>();
        Node7 currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        return path;
    }

    int GetDistance(Node7 nodeA, Node7 nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    public Node7 GetNodeFromPosition(Vector3 position)
    {
        return grid7.NodeFromWorldPoint(position);
    }
}
