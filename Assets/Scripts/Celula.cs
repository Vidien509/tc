using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
public enum CellState { vazia, torre, nucleo }

public class Celula : MonoBehaviour
{
    public CellState state;
    public string value = "";
    private LineRenderer borderRenderer;

    [SerializeField]
    private float borderWidth;

    [SerializeField]
    private float borderInset; // Novo: inset para evitar sobreposição

    void Awake()
    {
        borderWidth = 0.5f;
        borderInset = 0.05f;
        borderRenderer = gameObject.AddComponent<LineRenderer>();
        borderRenderer.positionCount = 5;
        borderRenderer.loop = true;
        borderRenderer.useWorldSpace = false;
        borderRenderer.material = new Material(Shader.Find("Sprites/Default"));
        UpdateBorderWidth();
    }

    private void UpdateBorderWidth()
    {
        borderRenderer.startWidth = borderWidth;
        borderRenderer.endWidth = borderWidth;
    }

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
        Color corBorda;

        renderer.color = new Color(1f, 1f, 1f, 0f);

        if (value != "0")
        {
            texto.text = value;
        }
        else
        {
            texto.text = "";
        }

        switch (state)
        {
            case CellState.vazia:
                corBorda = Color.white;
                break;
            case CellState.torre:
                if (!UnityEngine.ColorUtility.TryParseHtmlString("#0DDBC2", out corBorda))
                {
                    corBorda = Color.cyan;
                    Debug.LogError("Erro ao converter a cor hexadecimal para torre!");
                }
                break;
            case CellState.nucleo:
                if (!UnityEngine.ColorUtility.TryParseHtmlString("#2EA951", out corBorda))
                {
                    corBorda = Color.green;
                    Debug.LogError("Erro ao converter a cor hexadecimal para núcleo!");
                }
                break;
            default:
                corBorda = Color.white;
                break;
        }

        borderRenderer.startColor = corBorda;
        borderRenderer.endColor = corBorda;

        // Ajuste nas posições da borda para evitar sobreposição
        float halfWidth = 0.5f - borderInset;
        borderRenderer.SetPositions(new Vector3[]
        {
            new Vector3(-halfWidth, -halfWidth, 0),
            new Vector3(halfWidth, -halfWidth, 0),
            new Vector3(halfWidth, halfWidth, 0),
            new Vector3(-halfWidth, halfWidth, 0),
            new Vector3(-halfWidth, -halfWidth, 0)
        });

        UpdateBorderWidth();
    }

    public void SetBorderWidth(float width)
    {
        borderWidth = width;
        UpdateBorderWidth();
    }

    // Novo método para ajustar o inset da borda
    public void SetBorderInset(float inset)
    {
        borderInset = inset;
        UpdateCellVisuals();
    }
}