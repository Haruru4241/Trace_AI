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
            //Shuffle(neighbours);

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

        if (simplePath) return SimplifyPath(path, 1);
        return path;

    }
    public bool IsValidPatrolPoint(Vector3 point, float Radius)
    {
        LayerMask obstacleMask = LayerMask.GetMask("unwalkableMask", "slowZoneMask");

        Collider[] hitColliders = Physics.OverlapSphere(point, Radius, obstacleMask);
        return hitColliders.Length == 0;
    }

    public List<Node> SimplifyPath(List<Node> path, float radius)
    {
        if (path == null || path.Count < 2) return path;

        List<Node> simplifiedPath = new List<Node>();
        simplifiedPath.Add(path[0]); // 첫 번째 노드는 무조건 추가

        int currentIndex = 0;

        while (currentIndex < path.Count - 1)
        {
            int nextIndex = currentIndex + 1;

            while (nextIndex < path.Count - 1 && IsPathClear(path[currentIndex].worldPosition, path[nextIndex + 1].worldPosition, radius))
            {
                nextIndex++;
            }

            simplifiedPath.Add(path[nextIndex]);
            currentIndex = nextIndex;
        }

        return simplifiedPath;
    }

    private bool IsPathClear(Vector3 start, Vector3 end, float radius)
    {
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        // 레이캐스트를 사용하여 두 점 사이에 장애물이 있는지 확인
        if (Physics.SphereCast(start, radius, direction, out RaycastHit hit, distance))
        {
            return false;
        }

        return true;
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

    private void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public abstract void UpdateTargetPosition(Vector3 currentPosition, ref Vector3 targetPosition);
    
    public abstract void UpdatePath(Vector3 currentPosition, Vector3 targetPosition, ref List<Node> currentPath);

    public abstract void HandleEvent(ref Vector3 targetPosition, ref List<Node> currentPath, string arrivalType);
    
    public abstract void Initialize(AI ai, Dictionary<int, int> layerMask);

    public abstract void Enter();

    public abstract void Execute();

    public abstract void Exit();
}
