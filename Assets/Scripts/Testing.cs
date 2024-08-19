using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class Testing : MonoBehaviour
{
    public GameObject cellPrefab;
    private Grid gameGrid;
    private Grid menuGrid;
    private void Start()
    {
        gameGrid = new Grid(20, 10, 10f, new Vector3(0, 0, 0), cellPrefab, this.transform);
        menuGrid = new Grid(1, 5, 10f, new Vector3(220, 50, 0), cellPrefab, this.transform);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            gameGrid.setState(UtilsClass.GetMouseWorldPosition(), CellState.extrator);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log(gameGrid.getValue(UtilsClass.GetMouseWorldPosition()));
        }
    }
}
