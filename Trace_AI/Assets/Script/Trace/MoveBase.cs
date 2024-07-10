using UnityEngine;
using System.Collections.Generic;

public abstract class MoveBase : MonoBehaviour
{
    public bool simplePath=false;
    protected Grid grid;
    protected Transform player;
    protected AI ai;
    protected FSM fsm;

    public Dictionary<int, int> layerPenalties = new Dictionary<int, int>();
    void Awake()
    {
        ai = gameObject.GetComponent<AI>();
        grid = FindObjectOfType<Grid>();
        player = FindObjectOfType<PlayerMovement>().transform;
        fsm = GetComponent<FSM>();
    }

    public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
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

            List<Node> neighbours = grid.GetNeighbours(currentNode);

            foreach (Node neighbour in neighbours)
            {
                if (layerPenalties[neighbour.layerMask.value] >= 10000 || closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + layerPenalties[neighbour.layerMask.value];
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

    public List<Node> RetracePath(Node startNode, Node endNode)
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
    public bool IsValidPatrolPoint(Vector3 point, float Radius)
    {
        LayerMask obstacleMask = LayerMask.GetMask("unwalkableMask", "slowZoneMask");

        Collider[] hitColliders = Physics.OverlapSphere(point, Radius, obstacleMask);
        return hitColliders.Length == 0;
    }

    public int FindClosestPoint(Vector3 currentPosition, List<Vector3> points)
    {
        int closestIndex = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < points.Count; i++)
        {
            float distance = Vector3.Distance(currentPosition, points[i]);
            if (distance < closestDistance)
            {
                closestIndex = i;
                closestDistance = distance;
            }
        }

        return closestIndex;
    }

    public int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    public void MoveToNode(Transform aiTransform, Vector3 nodePosition, float moveSpeed)
    {
        Vector3 moveDirection = (nodePosition - aiTransform.position).normalized;
        aiTransform.position += moveDirection * moveSpeed * Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
        aiTransform.rotation = Quaternion.Slerp(aiTransform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    public abstract void UpdateTargetPosition(Vector3 currentPosition, ref Vector3 targetPosition);
    
    public abstract void UpdatePath(Vector3 currentPosition, Vector3 targetPosition, ref List<Node> currentPath);

    public abstract void HandleEvent(ref Vector3 targetPosition, ref List<Node> currentPath, string arrivalType);
    
    public abstract void Initialize();

    public abstract void Enter();

    public abstract void Execute();

    public abstract void Exit();
}
