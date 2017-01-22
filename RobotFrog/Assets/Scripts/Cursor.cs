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
        GameObject.Destroy(CubeProto);
        Bullseye.SetActive(false);
        ApplyTint();

    }

    public void ApplyTint()
    {
        for(int i=0; i<Cubes.Count; ++i)
        {
            Cubes[i].GetComponent<Renderer>().material.color = Tint;
        }
        Bullseye.GetComponent<Renderer>().material.color = Tint;
    }


    public void ShowLine(int size)
    {
        for(int i=0; i<Cubes.Count; ++i)
        {
            Cubes[i].SetActive(i<size - 1 && Level.levelState != LevelState.GameOver);
        }

        if (size > 0 && Level.levelState != LevelState.GameOver)
        {
            Bullseye.transform.localPosition = Cubes[size - 1].transform.localPosition;
            Bullseye.gameObject.SetActive(true);
        }
        else
        {
            Bullseye.gameObject.SetActive(false);
        }
    }
}
