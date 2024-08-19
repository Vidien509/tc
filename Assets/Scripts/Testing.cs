using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class Testing : MonoBehaviour
{

    private Grid grid;
    private void Start()
    {
        grid = new Grid(20, 10, 10f, new Vector3(0, 0, 0));
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            grid.setValue(UtilsClass.GetMouseWorldPosition(), 42);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log(grid.getValue(UtilsClass.GetMouseWorldPosition()));
        }
    }
}
