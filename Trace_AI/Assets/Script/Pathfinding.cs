using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Pathfinding
{
    private const float SLOW_ZONE_COST_MULTIPLIER = 2.0f; // 저속 구역의 비용 배수

    public List<Node> FindPath(Vector3 startPos, Vector3 targetPos, Grid grid)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        SortedSet<Node> openSet = new SortedSet<Node>(new NodeComparer());
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.First();
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            foreach (Node neighbor in grid.GetNeighbors(currentNode))
            {
                if (!neighbor.isWalkable || closedSet.Contains(neighbor))
                    continue;

                float newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (neighbor.isSlowZone)
                {
                    newCostToNeighbor *= SLOW_ZONE_COST_MULTIPLIER; // 저속 구역의 추가 비용 설정
                }

                if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
        return null;
    }

    private float GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    private List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    private class NodeComparer : IComparer<Node>
    {
        public int Compare(Node x, Node y)
        {
            int compare = x.fCost.CompareTo(y.fCost);
            if (compare == 0)
            {
                compare = x.hCost.CompareTo(y.hCost);
            }
            return compare;
        }
    }
}
