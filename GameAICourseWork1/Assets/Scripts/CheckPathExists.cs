using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPathExists : MonoBehaviour
{

    public GameObject PathPrefab;
    Queue<Vector3> queue;
    List<Vector3> visited;
    Dictionary<Vector3, Vector3> ParentNode;
    AddObjects AOS;
    PerlinNoiseMap PNM;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void Awake()
    {
        queue = new Queue<Vector3>();
        visited = new List<Vector3>();
        ParentNode = new Dictionary<Vector3, Vector3>();
        AOS = GetComponent<AddObjects>();
        PNM = GetComponent<PerlinNoiseMap>();
    }

    //My approach involves first doing the 'flood-fill' process ONCE to get the entire 'frontier' parent list to the start node. -
    //Because there will be multiple objects to check path for to the same start node. - according to CW requirements.
    //This way its more efficient than doing the flood fill process for each and every object separately.
    //Idea from : https://www.redblobgames.com/pathfinding/a-star/introduction.html link was provided in tutorial 4
    public Dictionary<Vector3, Vector3> floodFill()
    {
        //queue.Enqueue add player position
        queue.Enqueue(AOS.StartPos);
        //visited.Add add player position
        visited.Add(AOS.StartPos);

        while(queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            var neighbours = GetNeighbours(currentNode);
            foreach (var neighbour in neighbours)
            {
                if (!visited.Contains(neighbour))
                {
                    queue.Enqueue(neighbour);
                    visited.Add(neighbour);
                    ParentNode[neighbour] = currentNode;
                }
            }
        }
        return ParentNode; //entire parent node dictionary 
        Debug.Log("Returned Parent node list");
    }


    public bool VisualizePath(Dictionary<Vector3, Vector3> parentNodeDict, Vector3 ObjectPosition)
    {
        if (!ParentNode.ContainsValue(ObjectPosition))
        {
            return false;
        }
        var path = new List<Vector3>();
        var current = parentNodeDict[ObjectPosition];

        path.Add(AOS.EndPos);

        while (current != AOS.StartPos)
        {
            path.Add(current);
            current = parentNodeDict[current];
        }

        for (int i = 1; i < path.Count; i++)
        {
            var pathCellPosition = path[i];
            pathCellPosition.y = PathPrefab.transform.position.y;
            Instantiate(PathPrefab, pathCellPosition, Quaternion.identity, transform);

        }

        return true;
        //MovePlayer(path);
    }

    private List<Vector3> GetNeighbours(Vector3 currentNode)
    {
        var neighbours = new List<Vector3>()
        {
            new Vector3(currentNode.x - 1, 0, currentNode.z), // Up
            new Vector3(currentNode.x + 1, 0, currentNode.z), // Down
            new Vector3(currentNode.x, 0, currentNode.z - 1), // Left
            new Vector3(currentNode.x, 0, currentNode.z + 1), // Right
        };

        var walkableNeighbours = new List<Vector3>();
        foreach (var neighbour in neighbours)
        {
            if (!AOS.IsPositionOccupied(neighbour) && IsInLevelBounds(neighbour))
                walkableNeighbours.Add(neighbour);
        }

        return walkableNeighbours;
    }

    private bool IsInLevelBounds(Vector3 neighbour)
    {
        if (neighbour.x > 0 && neighbour.x <= PNM.MapHeight - 1 && neighbour.z > 0 && neighbour.z <= PNM.MapWidth - 1)
            return true;

        return false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
