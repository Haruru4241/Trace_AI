using UnityEngine;
using System.Collections.Generic;

public class Pathfinding9 : MonoBehaviour
{
    public GameManager gameManager;
    public Grid9 grid9; 

    void Awake()
    {
        if (gameManager != null)
        {
            grid9 = grid9 ?? gameManager.grid9;
        }
    }

    public List<Node9> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node9 startNode = grid9.NodeFromWorldPoint(startPos);
        Node9 targetNode = grid9.NodeFromWorldPoint(targetPos);

        List<Node9> openSet = new List<Node9>();
        HashSet<Node9> closedSet = new HashSet<Node9>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node9 currentNode = openSet[0];
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

            foreach (Node9 neighbour in grid9.GetNeighbours(currentNode))
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

    List<Node9> RetracePath(Node9 startNode, Node9 endNode)
    {
        List<Node9> path = new List<Node9>();
        Node9 currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        return path;
    }

    int GetDistance(Node9 nodeA, Node9 nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    public Node9 GetNodeFromPosition(Vector3 position)
    {
        return grid9.NodeFromWorldPoint(position);
    }
}
