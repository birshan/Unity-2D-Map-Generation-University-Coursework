using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceManager : MonoBehaviour
{
    PerlinNoiseMap PNM;
    CheckPathExists CPE;
    AddObjects AOS;
    // Start is called before the first frame update
    void Start()
    {
        CPE = GetComponent<CheckPathExists>();
        AOS = GetComponent<AddObjects>();
        PNM = GetComponent<PerlinNoiseMap>();

        //PNM.CreateTileSet();
        //PNM.CreateMap();
        //Camera.main.transform.position = new Vector3(PNM.MapWidth / 2, PNM.MapWidth, PNM.MapHeight / 2);//centering the camera
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
