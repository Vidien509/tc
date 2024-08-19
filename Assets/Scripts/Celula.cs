using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum CellState { vazia, recurso, extrator, modulo, torre, menu}

public class Celula : MonoBehaviour
{
    public CellState state;
    public string value;

    private void Start()
    {
        if (Random.value < 0.02f)
        {
            SetCellState(CellState.recurso, "");  // Definindo um valor aleatório Random.Range(1, 10)
            setValue("1");
        }
        else
        {
            setValue("0");
            SetCellState(CellState.vazia, "");
        }
    }

    public void setValue(string newValue)
    {
        value = newValue;
        UpdateCellVisuals();
    }
    public string getValue()
    {
        return value;
    }

    public void SetCellState(CellState newState, string value)
    {
        Debug.Log(newState + " " + this.value + " " + state);
        if (newState == CellState.extrator && !this.value.Equals("0") && state != newState)
        {
            state = newState;
            this.value = "e;" + this.value;
        }
        else if (newState == CellState.modulo)
        {
            state = newState;
            this.value = "m;" + this.value;
        }
        else if (newState == CellState.torre)
        {
            state = newState;
            this.value = "t;" + this.value;
        }
        else if (newState == CellState.vazia)
        {
            state = newState;
        }
        else if (newState == CellState.recurso)
        {
            state = newState;
        }
        else if (newState == CellState.menu) 
        {
            this.value = value;
        }

        UpdateCellVisuals();
    }
    void UpdateCellVisuals()
    {
        // Aqui você pode mudar a cor da célula ou adicionar um sprite dependendo do estado
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        TextMeshPro texto = transform.GetChild(0).GetComponent<TextMeshPro>();
        texto.text = value.ToString();
        switch (state)
        {
            case CellState.vazia:
                renderer.color = Color.white;
                break;
            case CellState.recurso:
                renderer.color = Color.green;
                break;
            case CellState.extrator:
                renderer.color = Color.blue;
                break;
            case CellState.modulo:
                renderer.color = Color.yellow;
                break;
            case CellState.torre:
                renderer.color = Color.red;
                break;
        }
    }
}
