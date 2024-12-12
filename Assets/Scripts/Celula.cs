using UnityEngine;
using TMPro;
using System;

public enum CellState { vazia, torre, nucleo }

public class Celula : MonoBehaviour
{
    public CellState state;
    public string value = "";
    private LineRenderer borderRenderer;
    private GameObject torreVisual;

    [SerializeField] private float borderWidth = 0.1f;
    [SerializeField] private float borderInset = 0.05f;
    [SerializeField] private float torreSize = 0.4f;

    private Color[] coresTorre = new Color[]
    {
        new Color(0.5f, 0.5f, 0.5f), // Basic
        new Color(0.0f, 0.7f, 1.0f), // Gelo
        new Color(1.0f, 0.4f, 0.0f), // Fogo
        new Color(0.8f, 0.0f, 1.0f)  // Plasma
    };

    void Awake()
    {
        SetupBorderRenderer();
        SetupTorreVisual();
    }

    private void SetupBorderRenderer()
    {
        borderRenderer = gameObject.AddComponent<LineRenderer>();
        borderRenderer.positionCount = 5;
        borderRenderer.loop = true;
        borderRenderer.useWorldSpace = false;
        borderRenderer.material = new Material(Shader.Find("Sprites/Default"));
        UpdateBorderWidth();
    }

    private void SetupTorreVisual()
    {
        torreVisual = new GameObject("TorreVisual");
        torreVisual.transform.SetParent(transform);
        torreVisual.transform.localPosition = Vector3.zero;
        SpriteRenderer spriteRenderer = torreVisual.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateBasicTowerSprite();
        torreVisual.SetActive(false);
    }

    private Sprite CreateBasicTowerSprite()
    {
        Texture2D texture = new Texture2D(32, 32);
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                if (y > x * 0.8f && y < 32 - x * 0.8f && x < 24)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else if (x >= 24 && x < 28 && y > 12 && y < 20)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.25f, 0.5f));
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
                if (GetComponent<Torre>() == null)
                    gameObject.AddComponent<Torre>();
                break;
            case CellState.nucleo:
                if (GetComponent<Nucleo>() == null)
                    gameObject.AddComponent<Nucleo>();
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
        UpdateCellVisuals();
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
            if (state == CellState.nucleo)
            {
                Nucleo nucleo = GetComponent<Nucleo>();
                if (nucleo != null)
                {
                    texto.text = $"{nucleo.vida}/{nucleo.vidaMaxima}";
                }
            }
            else
            {
                texto.text = value;
            }
            texto.alignment = TextAlignmentOptions.Center; // Centraliza o texto
        }
        else
        {
            texto.text = "";
        }

        switch (state)
        {
            case CellState.vazia:
                corBorda = Color.white;
                torreVisual.SetActive(false);
                break;
            case CellState.torre:
                Torre torre = GetComponent<Torre>();
                if (torre != null)
                {
                    corBorda = coresTorre[(int)torre.tipo];
                    torreVisual.SetActive(true);
                    AtualizarVisualTorre(torre.tipo, torre.nivel);
                }
                else
                {
                    corBorda = coresTorre[0];
                    torreVisual.SetActive(false);
                }
                break;
            case CellState.nucleo:
                if (!UnityEngine.ColorUtility.TryParseHtmlString("#2EA951", out corBorda))
                {
                    corBorda = Color.green;
                    Debug.LogError("Erro ao converter a cor hexadecimal para núcleo!");
                }
                torreVisual.SetActive(false);
                break;
            default:
                corBorda = Color.white;
                torreVisual.SetActive(false);
                break;
        }

        borderRenderer.startColor = corBorda;
        borderRenderer.endColor = corBorda;

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

    public void AnimateCreation()
    {
        gameObject.SetActive(true);
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, Vector3.one, 0.3f).setEaseOutBack();
    }

    public void SetBorderWidth(float width)
    {
        borderWidth = width;
        UpdateBorderWidth();
    }

    public void SetBorderInset(float inset)
    {
        borderInset = inset;
        UpdateCellVisuals();
    }

    public void UpdateTorreDirection(Vector3 direction)
    {
        if (state == CellState.torre && torreVisual != null)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            torreVisual.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void AtualizarVisualTorre(TorreType tipo, int nivel)
    {
        if (state == CellState.torre && torreVisual != null)
        {
            SpriteRenderer spriteRenderer = torreVisual.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateTowerSprite(tipo);
            spriteRenderer.color = coresTorre[(int)tipo];

            // Aumentar o tamanho da torre com base no nível
            float escala = 1f + (nivel * 0.1f);
            torreVisual.transform.localScale = new Vector3(escala, escala, 1f);

            // Atualizar a borda
            borderRenderer.startColor = coresTorre[(int)tipo];
            borderRenderer.endColor = coresTorre[(int)tipo];
        }
    }

    private Sprite CreateTowerSprite(TorreType tipo)
    {
        Texture2D texture = new Texture2D(32, 32);
        Color cor = Color.white;

        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                switch (tipo)
                {
                    case TorreType.Basic:
                        if (y > x * 0.8f && y < 32 - x * 0.8f && x < 24)
                        {
                            texture.SetPixel(x, y, cor);
                        }
                        break;
                    case TorreType.Gelo:
                        if ((x - 16) * (x - 16) + (y - 16) * (y - 16) <= 12 * 12)
                        {
                            texture.SetPixel(x, y, cor);
                        }
                        break;
                    case TorreType.Fogo:
                        if (x >= 8 && x < 24 && y >= 16 - Math.Abs(x - 16) && y <= 16 + Math.Abs(x - 16))
                        {
                            texture.SetPixel(x, y, cor);
                        }
                        break;
                    case TorreType.Plasma:
                        if ((x - 16) * (x - 16) + (y - 16) * (y - 16) <= 12 * 12 &&
                            ((x - 16) * (x - 16) + (y - 16) * (y - 16) > 8 * 8 ||
                             (x >= 14 && x <= 18) || (y >= 14 && y <= 18)))
                        {
                            texture.SetPixel(x, y, cor);
                        }
                        break;
                }
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }


    public void HighlightCell()
    {
        // Store the current text
        string currentText = transform.GetChild(0).GetComponent<TextMeshPro>().text;

        // Clear the text temporarily
        transform.GetChild(0).GetComponent<TextMeshPro>().text = "";

        // Store this information to use when unhighlighting
        transform.GetChild(0).GetComponent<TextMeshPro>().SetText(currentText);
    }

    public void UnhighlightCell()
    {
        // Restore the text
        transform.GetChild(0).GetComponent<TextMeshPro>().text = transform.GetChild(0).GetComponent<TextMeshPro>().text;

        // Update visuals
        UpdateCellVisuals();
    }
}

