using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotacion : MonoBehaviour
{
    [SerializeField] float vel = 1.0f;
    
    void Update()
    {
        Vector3 pos = GetComponent<Transform>().position;
        Vector3 pos2 = new Vector3(0.0f, 10000000.0f, 0.0f);
        GetComponent<Transform>().RotateAround(pos, pos2, vel * Time.deltaTime);
    }
}
