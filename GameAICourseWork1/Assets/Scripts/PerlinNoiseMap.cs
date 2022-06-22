using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseMap : MonoBehaviour
{
    Dictionary<int, GameObject> tileSet;
    public GameObject Plains;
    public GameObject Forests;
    public GameObject Hills;
    public GameObject Mountains;
    public GameObject Lake;

    public int MapWidth = 20;
    public int MapHeight = 20;
    public static List<Vector3> obstacles = new List<Vector3>();
    public static List<Vector3> walkables = new List<Vector3>();
    public static List<Vector3> plains = new List<Vector3>();
    public static List<Vector3> forests = new List<Vector3>();
    //perlin values
    float magnification = 9.0f;
    int offsetX = 0;
    int offsetY = 0;

    List<List<int>> TileMap = new List<List<int>>();
    // Start is called before the first frame update
    void Start()
    {
        obstacles = new List<Vector3>();
        walkables = new List<Vector3>();
        plains = new List<Vector3>();
        forests = new List<Vector3>();
        CreateTileSet();
        CreateMap();
        Camera.main.transform.position = new Vector3(MapWidth / 2, MapWidth, MapHeight / 2);//centering the camera

        //checkWalkables();
        
    }

    public void CreateTileSet()
    {
        tileSet = new Dictionary<int, GameObject>();
        tileSet.Add(0, Lake);//not walkable
        tileSet.Add(1, Plains);//walkable
        tileSet.Add(2, Forests);//walkable
        tileSet.Add(3, Hills); //not walkable
        tileSet.Add(4, Mountains); //not walkable
    }

    public void CreateMap()
    {
        for(int x=0; x<MapWidth; x++)
        {
            TileMap.Add(new List<int>());
            for(int y=0; y<MapHeight; y++)
            {
                int tileId = PerlinNoiseId(x, y);
                TileMap[x].Add(tileId); // add tile ID to tile map in list [X] at location y (index)
                if (tileId == 1 || tileId == 2)
                {
                    walkables.Add(new Vector3(x, 0, y));    // adds plains and forests to walkables
                    if(tileId == 1)
                    {
                        plains.Add(new Vector3(x, 0, y)); // to add objects to specific walkables
                    }
                    else
                    {
                        forests.Add(new Vector3(x, 0, y)); // to add objects to specific walkables
                    }
                    
                    
                }
                else
                {
                    obstacles.Add(new Vector3(x, 0, y));// adds lakes, hills and mountains to obstacles
                }


                GameObject tilePrefab = tileSet[tileId]; //get prefab object

                // this line from tutorial 4 - instantiate game object at location x,y - y is z here 
                var newTile = Instantiate(tilePrefab, new Vector3(x, 0, y), Quaternion.identity, transform);


                
            }
        }
    }

    private int PerlinNoiseId(int x, int y)
    {
        float PerlinRaw = Mathf.PerlinNoise(
            (x - offsetX) / magnification,
            (y - offsetY) / magnification
            );
        //normalise between 0 and 1
        float PerlinClamped = Mathf.Clamp(PerlinRaw, 0.0f, 1.0f);
        //scale with number of tile prefabs terrain types
        float PerlinScaled = PerlinClamped * tileSet.Count;
        Debug.Log(tileSet.Count);

        if(PerlinScaled == tileSet.Count)
        {
            PerlinScaled = tileSet.Count - 1;
        }
        return Mathf.FloorToInt(PerlinScaled);
    }
}
