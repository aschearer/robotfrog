using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class Cursor : MonoBehaviour {


    internal Color Tint { get; set; }

    public GameObject CubeProto;
    public GameObject Bullseye;
    private List<GameObject> Cubes = new List<GameObject>();

    void Start()
    {
        for(int i=0; i<6; ++i)
        {
            GameObject cube = GameObject.Instantiate(CubeProto, 
                CubeProto.transform.position, CubeProto.transform.rotation, this.transform);
            cube.transform.SetParent(this.transform, false);
            cube.transform.localPosition = Vector3.forward*(i+1) - Vector3.up*0.40f;
            cube.name = "Cube"+i;
            cube.SetActive(false);
            Cubes.Add(cube);
        }
        CubeProto.SetActive(false);
        Bullseye.SetActive(false);

    }


    public void ShowLine(int size)
    {
        for(int i=0; i<Cubes.Count; ++i)
        {
            Cubes[i].SetActive(i<size);
        }
        if(Cubes.Count > 0)
        {
            int spot = Mathf.Clamp(size,0,Cubes.Count-1);
            Bullseye.transform.localPosition = Cubes[spot].transform.localPosition;
            Bullseye.SetActive(size > 0);
        }
    }
}
