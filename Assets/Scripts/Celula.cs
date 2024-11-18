using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public enum CellState { vazia, torre, nucleo}

public class Celula : MonoBehaviour
{
    public CellState state;
    public string value = "";

    public void SetComponent()
    {
        switch (state)
        {
            case CellState.torre:
                this.AddComponent<Torre>();
                break;
            case CellState.nucleo:
                this.AddComponent<Nucleo>();
                break;
        }
    }

    public void setValue(string newValue)
    {
        value = newValue;
    }
    public string getValue()
    {
        return value;
    }

    public void SetCellState(CellState newState)
    {
        state = newState;
        SetComponent();
    }

    public CellState GetCellState()
    {
        return state;
    }

    public void UpdateCellVisuals()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        TextMeshPro texto = transform.GetChild(0).GetComponent<TextMeshPro>();
        if (value != "0")
        {
            texto.text = value;
        }
        switch (state)
        {
            case CellState.vazia:
                renderer.color = Color.white;
                break;
            case CellState.torre:
                renderer.color = Color.red;
                break;
            case CellState.nucleo:
                renderer.color = Color.blue;
                break;
        }
    }
}
