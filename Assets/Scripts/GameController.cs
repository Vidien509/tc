using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class GameController : MonoBehaviour
{
    public GameObject cellPrefab;
    private Grid gameGrid;
    private Grid menuGrid;
    private void Start()
    {
        gameGrid = new Grid(20, 10, 10f, new Vector3(0, 0, 0), cellPrefab, this.transform);
        menuGrid = new Grid(1, 5, 10f, new Vector3(220, 50, 0), cellPrefab, this.transform);
        gameGrid.SetRecursosAleatorios();
    }

    private void Update()
    {
        gameGrid.HighlightCelula(UtilsClass.GetMouseWorldPosition());
        menuGrid.HighlightCelula(UtilsClass.GetMouseWorldPosition());

        if (Input.GetMouseButtonDown(0))
        {
            Celula cel = gameGrid.GetCelula(UtilsClass.GetMouseWorldPosition());
            cel.SetCellState(CellState.extrator);
            this.GetComponent<ResourceController>().adicionaExtrator(cel);
        }
    }
}
