using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddObjects : MonoBehaviour
{
    public GameObject Player;
    public GameObject UniqueDeer;
    public Vector3 StartPos;
    public Vector3 EndPos;
    PerlinNoiseMap PNM;
    CheckPathExists CPE;

    Queue<Vector3> queue;
    List<Vector3> visited;
    Dictionary<Vector3, Vector3> ParentNode;

    //clustering and placing other objects 
    public GameObject Centroid;
    public GameObject ClusterPoint;
    List<Vector3> centroids = new List<Vector3>();
    public int KNumberOfCentroids = 6;
    List<Color> colors;
    Dictionary<Vector3, List<Vector3>> clustersDict;
    List<GameObject> CentroidObjectList;
    List<GameObject> PointObjectList;
    Dictionary<Vector3, Vector3> FloodFillMap;
    bool ranOnce;
    bool toggle;


    // ADDING OBJECTS ACCORDING TO CLUSTER ------
    public List<GameObject> Preys;//each cluster should have one type of prey
    public int PreysPerCluster = 1; // preys of same type per cluster

    public List<GameObject> Predators;//each cluster should have one type of Predators
    public int PredatorsPerCluster = 1; // Predators of same type per cluster

    public List<GameObject> Pickables;//each cluster should have one type of Pickables
    public int PickablesPerCluster = 1; // Pickables of same type per cluster

    public List<GameObject> Enemies;//each cluster should have one type of Enemies
    public int EnemiesPerCluster = 1; // Enemies of same type per cluster

    // Start is called before the first frame update
    void Start()
    {
        //test start - end path finding
        PNM = GetComponent<PerlinNoiseMap>();
        StartPos = PlaceObject(Player); //start
        Debug.Log(StartPos);
        EndPos = PlaceObject(UniqueDeer);//end
        //List<GameObject> Frogs = new List<GameObject>() { RedFrog, YellowFrog, WhiteFrog, BlackFrog };
        CPE = GetComponent<CheckPathExists>();
        queue = new Queue<Vector3>();
        visited = new List<Vector3>();
        ParentNode = new Dictionary<Vector3, Vector3>();
        clustersDict = new Dictionary<Vector3, List<Vector3>>();
        centroids = new List<Vector3>();
        ranOnce = false;
        toggle = false;
        CentroidObjectList = new List<GameObject>();
        PointObjectList = new List<GameObject>();
        FloodFillMap = new Dictionary<Vector3, Vector3>();

        FloodFillMap = CPE.floodFill();
        CPE.VisualizePath(FloodFillMap, EndPos); //test
        ClusterWalkables();

        //ToggleClusters();
        //addObjectsToClusters(true, Frogs, 2);
    }

   
    private Vector3 PlaceObject(GameObject Obj, bool centroid = false)
    {
        while (true)
        {
            var X = UnityEngine.Random.Range(1, PNM.MapHeight);
            var Z = UnityEngine.Random.Range(1, PNM.MapWidth);

            var position = new Vector3(X, 0, Z);

            if (!IsPositionOccupied(position)) // if position is walkable
            {
                var objectPosition = position;
                objectPosition.y = Obj.transform.position.y;

                if (!centroid) { Instantiate(Obj, objectPosition, Quaternion.identity, transform); }
                
                Debug.Log("INstanstiated.");

                return position;
            }
        }
    }

    public bool IsPositionOccupied(Vector3 position)
    {
        if (PerlinNoiseMap.obstacles.Contains(position))
        {
            return true;
        }
        else
        {
            return false;
        }
    }



    /// <summary>
    /// CLUSTER AND ADD DIFFERENT OBJECT TYPES ------------------------------------------------------
    /// </summary>
    /// 
    public void ClusterWalkables()
    {
        // Initialization
        //points = GenerateGameObjects(Point, Points, PointsHolder);     //points already exist, and a list of points -  they are the walkables
        //PerlinNoiseMap.walkables;
        

        for (int i=0; i<KNumberOfCentroids; i++)
        {
            centroids.Add(PlaceObject(Centroid, true)); // place object and return transform position to centroids (position list)
            Debug.Log(" placed centroid ran times");
        }
        
        
        colors = GenerateColors();

        // Start with an execution of the algorithm
        Cluster();
    }

    

    //didnt include getCentroidList because I already used a list of vectors instead of a list of objects to store everything

    private List<Color> GenerateColors()
    {
        var result = new List<Color>();

        for (int i = 0; i < KNumberOfCentroids; i++)
        {
            var color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f); // add color to point
            result.Add(color);
        }

        return result;
    }



    public void Cluster()
    {
        // Construct clusters dictionary
        clustersDict = CreateClusters(); 

        // Add points to clusters they belong
        AddPointsToClusters();



        // Take the sum of all the positions in the cluster and divide by the number of points
        bool ended;
        do
        {
            ended = AdjustClusters();//keep doing this in loop until check ended is true
        } while (!ended);


        // Set colors to points from each cluster
       //SetColorToClusterPoints();   // changed it to put it under toggle button 

    }

    

    private Dictionary<Vector3, List<Vector3>> CreateClusters()
    {
        var result = new Dictionary<Vector3, List<Vector3>>();

        for (int i = 0; i < KNumberOfCentroids; i++)
        {
            result.Add(centroids[i], new List<Vector3>());
        }

        return result;
    }

    private void AddPointsToClusters()
    {
        for (int i = 0; i < PerlinNoiseMap.walkables.Count; i++)
        {
            var pointPosition = PerlinNoiseMap.walkables[i];
            var minDistance = float.MaxValue;
            var closestCentroid = centroids[0]; //remembver we are using vector3 instead of gameobject !!

            for (int j = 0; j < KNumberOfCentroids; j++)
            {
                var distance = Vector3.Distance(pointPosition, centroids[j]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCentroid = centroids[j]; //remembver we are using vector3 instead of gameobject !! can instantiate a preefabb at location when needed
                }
            }

            clustersDict[closestCentroid].Add(PerlinNoiseMap.walkables[i]);//remembver we are using vector3 instead of gameobject !!
        }
    }


    private bool AdjustClusters()
    {
        bool ended = false;
        var clusterCounter = 0;
        foreach (var cluster in clustersDict)
        {
            var sum = Vector3.zero;

            foreach (var point in cluster.Value)
            {
                sum += point;
            }

            var average = sum / cluster.Value.Count;
            
            if(centroids[clusterCounter] == average)
            {
                ended = true;
                return ended;
            }
            centroids[clusterCounter] = average;
            clusterCounter++;
        }
        return ended;
    }
    

    private void SetColorToClusterPoints()
    {
        // Only use at the END of the Kmeans algorithm after the clustering process has ended
        // Instantiate clusterPointPrefabs at the values of the clustersDict dictionary. Iterate through and Put color according to the key.
        var clusterCounter = 0;
        foreach (var cluster in clustersDict)
        {
            var finalCluster = Instantiate(Centroid, cluster.Key, Quaternion.identity, transform);//add final cluster to list
            CentroidObjectList.Add(finalCluster);

            foreach (var point in cluster.Value)
            {
                //instantiate prefab at that point, with color.\
                var newPoint = Instantiate(ClusterPoint, point, Quaternion.identity, transform); //add new point to list - to delete when cluster button is not pressed
                newPoint.GetComponent<MeshRenderer>().material.color = colors[clusterCounter];
                PointObjectList.Add(newPoint);
            }
            clusterCounter++;
        }
    }

    public void ToggleClusters()
    {

        if (ranOnce == false)
        {
            SetColorToClusterPoints(); // where objects representing points and centroids are instantiated and colored.}    RUNS ONCE
            ranOnce = true;
        }
        else
        {

            foreach (var centroid in CentroidObjectList)
            {

                centroid.SetActive(toggle); // Will activate and deactivate the object.

                foreach (var point in PointObjectList)
                {
                    point.SetActive(toggle);
                }

            }
            toggle = !toggle;
        }


    }

    public void addObjectsToClusters(bool plains, List<GameObject> objects, int ObjectsPerCluster)
    {
        // add objects according to the cluster dictionary which holds key value pairs of <Centroids, ClusterPoints>. 
        int counter = 0;
        foreach (var cluster in clustersDict)
        {
            List<Vector3> filteredList = new List<Vector3>();
            GameObject objectToPlace = objects[counter];
            counter++;
            if(counter >= objects.Count)
            {
                counter = 0; // will cycle through the objects list [red frog, black frog, white frog etc) for each cluster
            }


            //pick a cluster point at random where the cluster point == the type of tile you want the object to be in
            foreach (var clusterpoint in cluster.Value)
            {
                if (plains && PerlinNoiseMap.plains.Contains(clusterpoint))
                {
                    filteredList.Add(clusterpoint); //for prey and predators
                }
                else if (!plains && PerlinNoiseMap.forests.Contains(clusterpoint))
                {
                    filteredList.Add(clusterpoint); //for Enemies and pickables
                }
            }
            for (int i = 0; i < ObjectsPerCluster; i++)
            {
                //visualise path before placing object at random location inside cluster - if returns false, it means there is no path, and randomize location and try again.
                int index;
                do
                {
                    index = (int)UnityEngine.Random.Range(0, filteredList.Count);
                } while (!CPE.VisualizePath(FloodFillMap, filteredList[index])); //As long as path does not exist, a new index will be randomly chosen from the filtered map range
                //visualisepath method creates the path regardless of whether the objects are placed..

                // use selected vector to add object 


                Instantiate(objectToPlace, filteredList[index], Quaternion.identity, transform);
            }


        }
    }

    public void AddPreyButton()
    {
        addObjectsToClusters(true, Preys, PreysPerCluster);
    }

    public void AddPredatorButton()
    {
        addObjectsToClusters(true, Predators, PredatorsPerCluster);
    }

    public void AddPickablesButton()
    {
        addObjectsToClusters(false, Pickables, PickablesPerCluster);
    }

    public void AddEnemiesButton()
    {
        addObjectsToClusters(false, Enemies, EnemiesPerCluster);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
