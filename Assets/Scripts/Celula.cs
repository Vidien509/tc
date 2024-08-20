using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public enum CellState { vazia, recurso, extrator, modulo, torre, menu}

public class Celula : MonoBehaviour
{
    public CellState state;
    public string value = "";

    public void SetComponent()
    {
        switch (state)
        {
            case CellState.extrator:
                this.AddComponent<Extrator>();
                break;
            case CellState.modulo:
                this.AddComponent<Modulo>();
                break;
            case CellState.torre:
                this.AddComponent<Menu>();
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
        if (newState == CellState.extrator)
        {
            /*
            if (value > 0)
            {
                state = newState;
            }
            */
        }
    }
    public void UpdateCellVisuals()
    {
        // Aqui você pode mudar a cor da célula ou adicionar um sprite dependendo do estado
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        TextMeshPro texto = transform.GetChild(0).GetComponent<TextMeshPro>();
        texto.text = value;
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
