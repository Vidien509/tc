using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using CodeMonkey.Utils;
using System.Linq;

public class Grid : MonoBehaviour
{

    private int width;
    private int height;
    private Celula[,] gridArray;
    private Vector3 originPosition;
    private float cellSize;

    private TextMesh[,] debugTextArray;
    public Grid(int width, int height, float cellSize, Vector3 originPosition, GameObject cellPrefab, Transform originTransform)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new Celula[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                Vector3 position = new Vector3(x, y, 0);
                GameObject cellObj = Instantiate(cellPrefab, getWorldPosition(x, y) + new Vector3(cellSize, cellSize) * .5f, Quaternion.identity);
                cellObj.transform.parent = originTransform;
                cellObj.transform.localScale = new Vector3(cellSize, cellSize, 0);
                gridArray[x, y] = cellObj.GetComponent<Celula>();
                gridArray[x, y].setValue("");
                gridArray[x, y].UpdateCellVisuals();
            }
        }
    }

    private Vector3 getWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize + originPosition;
    }

    private void getXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
    }

    public void setState(int x, int y, CellState state)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y].SetCellState(state);
        }
    }
    public void setState(Vector3 worldPosition, CellState state)
    {
        int x, y;
        getXY(worldPosition, out x, out y);
        setState(x, y, state);
    }
    public void setValue(int x, int y, string value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y].setValue(value);
        }
    }

    public void setValue(Vector3 worldPosition, string value)
    {
        int x, y;
        getXY(worldPosition, out x, out y);
        setValue(x, y, value);
    }

    public string getValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y].getValue();
        }
        else
        {
            return "";
        }
    }

    public string getValue(Vector3 worldPosition)
    {
        int x, y;
        getXY(worldPosition, out x, out y);
        return (getValue(x, y));
    }

    public Celula GetCelula(Vector3 worldPosition)
    {
        int x, y;
        getXY(worldPosition, out x, out y);
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y].GetComponent<Celula>();
        }
        else
        {
            return null;
        }
    }

    private Celula ultimaCelulaHigh = null;
    public void HighlightCelula(Vector3 worldPosition)
    {
        int x, y;
        Celula cel;
        getXY(worldPosition, out x, out y);
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            cel = gridArray[x, y].GetComponent<Celula>();
            if (cel == ultimaCelulaHigh) return;
            SpriteRenderer renderer = cel.GetComponent<SpriteRenderer>();
            renderer.color = Color.grey;

            if (ultimaCelulaHigh)
            {
                ultimaCelulaHigh.UpdateCellVisuals();
            }
            ultimaCelulaHigh = cel;
        }else if (ultimaCelulaHigh)
        {
            ultimaCelulaHigh.UpdateCellVisuals();
            ultimaCelulaHigh = null;
        }
    }

    public void SetRecursosAleatorios()
    {
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                Celula cel = gridArray[x, y].GetComponent<Celula>();
                cel.setValue("0");
                cel.SetCellState(CellState.vazia);
                cel.UpdateCellVisuals();

                int centerX = gridArray.GetLength(0) / 2;
                int centerY = gridArray.GetLength(1) / 2;

                if (x == centerX && y == centerY)
                {
                   cel.SetCellState(CellState.nucleo);
                }

                cel.UpdateCellVisuals();
            }
        }
    }
}
