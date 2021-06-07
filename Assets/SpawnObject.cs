using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnObject : MonoBehaviour
{
    public GameObject[] objects;
    
    void Start()
    {
        int rand = Random.Range(0, objects.Length);
        GameObject temp=Instantiate(objects[rand], transform.position, quaternion.identity);
        temp.transform.parent = this.transform;
    }

    void Update()
    {
        
    }
}
