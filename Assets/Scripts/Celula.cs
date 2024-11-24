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
        Color corHexadecimal;
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
                if (UnityEngine.ColorUtility.TryParseHtmlString("#0DDBC2", out corHexadecimal))
                {
                    renderer.color = corHexadecimal; // Define a cor convertida do hexadecimal
                }
                else
                {
                    Debug.LogError("Erro ao converter a cor hexadecimal!");
                }
                break;
            case CellState.nucleo:
                if (UnityEngine.ColorUtility.TryParseHtmlString("#2EA951", out corHexadecimal))
                {
                    renderer.color = corHexadecimal; // Define a cor convertida do hexadecimal
                }
                else
                {
                    Debug.LogError("Erro ao converter a cor hexadecimal!");
                }
                break;
        }
    }
}
