using UnityEngine;
using System.Collections.Generic;

public class Pathfinding4 : MonoBehaviour
{
    public Grid4 grid4;

    public List<Node4> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node4 startNode = grid4.NodeFromWorldPoint(startPos);
        Node4 targetNode = grid4.NodeFromWorldPoint(targetPos);

        List<Node4> openSet = new List<Node4>();
        HashSet<Node4> closedSet = new HashSet<Node4>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node4 currentNode = openSet[0];
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

            foreach (Node4 neighbour in grid4.GetNeighbours(currentNode))
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

    List<Node4> RetracePath(Node4 startNode, Node4 endNode)
    {
        List<Node4> path = new List<Node4>();
        Node4 currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        return path;
    }

    int GetDistance(Node4 nodeA, Node4 nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    void OnDrawGizmos()
    {
        if (grid4 != null)
        {
            if (grid4.grid4 != null && path != null)
            {
                foreach (Node4 n in path)
                {
                    Gizmos.color = Color.black;
                    Vector3 gizmoPosition = new Vector3(n.worldPosition.x, 1, n.worldPosition.z);
                    Gizmos.DrawCube(gizmoPosition, Vector3.one * (grid4.GetNodeDiameter() - .1f));
                }
            }
        }
    }

    public List<Node4> path;
}
